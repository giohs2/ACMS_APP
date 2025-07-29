using ACMS_Program_Planner.Models;
using ACMS_Program_Planner.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsModel ViewModel { get; } = SettingsModel.Instance;

        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation to user
            var dialog = new ContentDialog
            {
                Title = "Reset to Defaults",
                Content = "Are you sure you want to reset to default settings?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            // Check if the user clicked "Yes" (PrimaryButton)
            if (result == ContentDialogResult.Primary)
            {
                // User confirmed, so reset to defaults
                DataService.Instance.ResetToDefaults();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.SaveSettings();

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // Initialize the file picker with WinRT interop
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            // Configure save picker
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON Documents", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "database";

            // Show the file picker and get the selected file
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                try
                {
                    // Write the data to the selected file
                    await Windows.Storage.FileIO.WriteTextAsync(file, DataService.Instance.GetDataInJson());
                }
                catch (Exception)
                {
                }
            }


            // Show confirmation to user
            /*var dialog = new ContentDialog
            {
                Title = "Settings Saved",
                Content = "Your settings have been saved successfully.",
                CloseButtonText = "OK"
            };

            _ = dialog.ShowAsync();*/
        }
    }
}
