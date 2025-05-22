using ACMS_Program_Planner.Models;
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
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RFIDPage : Page
{
    private bool isConnected = false;
    private SerialPort? serialPort;

    public SettingsModel ViewModel { get; } = SettingsModel.Instance;


    public RFIDPage()
    {
        InitializeComponent();
        DataContext = ViewModel;

        // Initialize UI state
        UpdateUIState();
    }

    private void Connect_Button_Click(object sender, RoutedEventArgs e)
    {
        if (!isConnected)
        {
            try
            {
                // Create and configure the serial port
                serialPort = new SerialPort
                {
                    PortName = ViewModel.SelectedComPort,
                    BaudRate = ViewModel.SelectedBaudRate,
                    DataBits = ViewModel.SelectedDataBit,
                    Parity = ViewModel.SelectedParity,
                    StopBits = ViewModel.SelectedStopBit,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                // Set up data received event handler
                serialPort.DataReceived += SerialPort_DataReceived;

                // Open the connection
                serialPort.Open();

                isConnected = true;
                StatusTextBlock.Text = $"Connected to {ViewModel.SelectedComPort}";

                // Update UI
                UpdateUIState();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to connect: {ex.Message}");
            }
        }
        else
        {
            try
            {
                // Close the connection
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                }

                isConnected = false;
                StatusTextBlock.Text = "Disconnected";

                // Update UI
                UpdateUIState();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to disconnect: {ex.Message}");
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (serialPort == null || !serialPort.IsOpen)
            return;

        try
        {
            string data = serialPort.ReadExisting();

            // We need to use dispatcher to update UI from a different thread
            DispatcherQueue.TryEnqueue(() =>
            {
                ReceivedDataTextBox.Text += data;

                // Auto-scroll to the bottom
                ReceivedDataTextBox.SelectionStart = ReceivedDataTextBox.Text.Length;
                ReceivedDataTextBox.SelectionLength = 0;
            });
        }
        catch (Exception ex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ShowErrorMessage($"Error reading data: {ex.Message}");
            });
        }
    }

    private void Send_Button_Click(object sender, RoutedEventArgs e)
    {
        if (!isConnected || serialPort == null || !serialPort.IsOpen)
        {
            ShowErrorMessage("Not connected to a serial port");
            return;
        }

        try
        {
            string textToSend = SendDataTextBox.Text;
            if (!string.IsNullOrEmpty(textToSend))
            {
                serialPort.Write(textToSend);
                StatusTextBlock.Text = "Data sent successfully";
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to send data: {ex.Message}");
        }
    }

    private void Clear_Received_Button_Click(object sender, RoutedEventArgs e)
    {
        ReceivedDataTextBox.Text = string.Empty;
    }

    private void UpdateUIState()
    {
        ConnectButton.Content = isConnected ? "Disconnect" : "Connect";
        SendDataTextBox.IsEnabled = isConnected;
        Send_Button.IsEnabled = isConnected;
        ComPortComboBox.IsEnabled = !isConnected;
        BaudRateComboBox.IsEnabled = !isConnected;
        DataBitsComboBox.IsEnabled = !isConnected;
        ParityComboBox.IsEnabled = !isConnected;
        StopBitsComboBox.IsEnabled = !isConnected;
    }

    private void ShowErrorMessage(string message)
    {
        StatusTextBlock.Text = message;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Clean up resources when navigating away
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DataReceived -= SerialPort_DataReceived;
            serialPort.Close();
            serialPort.Dispose();
        }
    }
}
