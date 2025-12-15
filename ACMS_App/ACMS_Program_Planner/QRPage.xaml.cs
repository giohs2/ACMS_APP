using ACMS_Program_Planner.Models;
using ACMS_Program_Planner.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QRCoder;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ACMS_Program_Planner;

/// <summary>
/// QR Code Generator page - creates QR codes from program data for iOS app scanning.
/// CSV format compatible with ServiceCarouselViewModel.singleService(fromScannedCSV:)
/// Uses QRCoder library for fully offline QR generation.
/// </summary>
public sealed partial class QRPage : Page
{
    private byte[]? currentQrImageBytes;
    private string? currentCsvData;
    private RFIDUnit? selectedUnit;

    public SettingsModel ViewModel => SettingsModel.Instance;

    public ObservableCollection<ServicePlanItem> ServicePlans =>
        new(DataService.Instance.ServicePlans.Where(p => !p.IsEditable));

    public ObservableCollection<RFIDUnit> RFIDUnits { get; } = [];

    public QRPage()
    {
        InitializeComponent();
        SettingsModel.Initialize(DispatcherQueue);
        DataContext = ViewModel;
    }

    private void PlansListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RFIDUnits.Clear();
        ClearQRPreview();

        if (e.AddedItems.Count == 0) return;

        var selectedPlan = e.AddedItems[0] as ServicePlanItem;
        if (selectedPlan == null) return;

        foreach (var cycle in selectedPlan.Cycles)
        {
            foreach (var unitProgram in cycle.UnitPrograms)
            {
                if (unitProgram.ProgramId > 0)
                {
                    RFIDUnits.Add(new RFIDUnit
                    {
                        Cycle = cycle,
                        UnitProgram = unitProgram,
                        IsDone = false
                    });
                }
            }
        }

        StatusTextBlock.Text = $"Found {RFIDUnits.Count} unit(s). Select one to generate QR code.";
    }

    private void UnitsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedUnit = UnitsListView.SelectedItem as RFIDUnit;

        if (selectedUnit != null)
        {
            StatusTextBlock.Text = $"Selected: {selectedUnit.CycleUnit} - {selectedUnit.ProgramName}. Click 'Generate QR Code'.";
        }
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedUnit == null)
        {
            StatusTextBlock.Text = "Please select a unit first.";
            return;
        }

        GenerateButton.IsEnabled = false;
        StatusTextBlock.Text = "Generating QR code...";

        try
        {
            // Generate CSV data
            currentCsvData = ProgramCsvEncoder.ToCsv(selectedUnit, out string? error);

            if (currentCsvData == null)
            {
                StatusTextBlock.Text = $"Error: {error}";
                GenerateButton.IsEnabled = true;
                return;
            }

            // Show CSV preview
            CsvPreviewTextBox.Text = currentCsvData;

            // Generate QR code locally using QRCoder (fully offline)
            currentQrImageBytes = GenerateQRCodePng(currentCsvData);

            if (currentQrImageBytes == null || currentQrImageBytes.Length == 0)
            {
                StatusTextBlock.Text = "Error: Failed to generate QR code.";
                GenerateButton.IsEnabled = true;
                return;
            }

            // Display QR code in preview
            await DisplayQRImage(currentQrImageBytes);

            SaveButton.IsEnabled = true;
            StatusTextBlock.Text = $"QR code generated successfully (offline). CSV length: {currentCsvData.Length} chars.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error: {ex.Message}";
        }
        finally
        {
            GenerateButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Generates a QR code PNG image from the given data string using QRCoder.
    /// Uses PngByteQRCode for direct PNG byte output without System.Drawing dependency.
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <returns>PNG image bytes</returns>
    private static byte[] GenerateQRCodePng(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        
        // Create QR code data with error correction level Q (25% recovery)
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        
        // Use PngByteQRCode for direct PNG output (no System.Drawing needed)
        var pngQrCode = new PngByteQRCode(qrCodeData);
        
        // Generate PNG with 20 pixels per module (each QR "square")
        // This produces a ~400x400 pixel image for typical data sizes
        byte[] pngBytes = pngQrCode.GetGraphic(20);
        
        return pngBytes;
    }

    private async Task DisplayQRImage(byte[] imageBytes)
    {
        using var stream = new MemoryStream(imageBytes);
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());

        QRCodeImage.Source = bitmapImage;
        QRCodeImage.Visibility = Visibility.Visible;
        PlaceholderText.Visibility = Visibility.Collapsed;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentQrImageBytes == null || currentQrImageBytes.Length == 0)
        {
            StatusTextBlock.Text = "No QR code to save. Generate one first.";
            return;
        }

        if (selectedUnit?.Program == null)
        {
            StatusTextBlock.Text = "No program selected.";
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            // Use FileSavePicker for user to choose location
            var savePicker = new FileSavePicker();

            // Get window handle for WinUI 3 desktop
            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("PNG Image", [".png"]);
            savePicker.SuggestedFileName = $"QR_Program_{selectedUnit.Program.Id}_{DateTime.Now:yyyyMMddHHmmss}";

            StorageFile? file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, currentQrImageBytes);
                StatusTextBlock.Text = $"Saved: {file.Path}";

                // Mark unit as done
                selectedUnit.IsDone = true;
            }
            else
            {
                StatusTextBlock.Text = "Save cancelled.";
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Save error: {ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void ClearQRPreview()
    {
        QRCodeImage.Source = null;
        QRCodeImage.Visibility = Visibility.Collapsed;
        PlaceholderText.Visibility = Visibility.Visible;
        CsvPreviewTextBox.Text = string.Empty;
        SaveButton.IsEnabled = false;
        currentQrImageBytes = null;
        currentCsvData = null;
        selectedUnit = null;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // Cleanup if needed
    }
}
