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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static SkiaSharp.HarfBuzz.SKShaper;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RFIDPage : Page
{
    private bool isConnected = false;
    private readonly RFIDService rfidService;

    public SettingsModel ViewModel { get => SettingsModel.Instance; }

    public ObservableCollection<ServicePlanItem> ServicePlans => new ObservableCollection<ServicePlanItem>(DataService.Instance.ServicePlans.Where(p => !p.IsEditable));

    public ObservableCollection<RFIDUnit> RFIDUnits = new ObservableCollection<RFIDUnit>();

    public RFIDPage()
    {
        InitializeComponent();
        SettingsModel.Initialize(DispatcherQueue);

        DataContext = ViewModel;

        // Use RFIDService
        rfidService = new RFIDService(DispatcherQueue);
        rfidService.DataReceived += OnRFIDDataReceived;
        rfidService.ErrorOccurred += ShowErrorMessage;
        rfidService.ConnectionChanged += OnRFIDConnectionChanged;

        // Initialize UI state
        UpdateUIState();
    }

    private void Connect_Button_Click(object sender, RoutedEventArgs e)
    {
        if (!rfidService.IsConnected)
        {
            rfidService.Connect(ViewModel);
            // After connect, initialize LEDs and wait for response
            _ = InitializeRFIDAsync();
        }
        else
        {
            rfidService.Disconnect();
        }
    }

    private async Task InitializeRFIDAsync()
    {
        var response = await rfidService.SendCommandAndWaitAsync(rfidService.Command.InitializeLEDs(), 2);
        if (response.StartsWith("00"))
        {
            rfidService.SendData(rfidService.Command.TurnOnGreenLED());
        }
    }

    private void OnRFIDConnectionChanged(bool connected)
    {
        isConnected = connected;
        StatusTextBlock.Text = connected ? $"Connected to {ViewModel.SelectedComPort}" : "Disconnected";
        UpdateUIState();
    }

    private void OnRFIDDataReceived(string data)
    {
        DataTextBox.Text += data;
        DataTextBox.SelectionStart = DataTextBox.Text.Length;
        DataTextBox.SelectionLength = 0;
    }

    private void UpdateUIState()
    {
        ConnectButton.Content = isConnected ? "Disconnect" : "Connect";
        DataTextBox.IsEnabled = isConnected;
        ComPortComboBox.IsEnabled = !isConnected;
        BaudRateComboBox.IsEnabled = !isConnected;
        DataBitsComboBox.IsEnabled = !isConnected;
        ParityComboBox.IsEnabled = !isConnected;
        StopBitsComboBox.IsEnabled = !isConnected;
        OvenProgramListView.IsEnabled = isConnected;
    }

    private void ShowErrorMessage(string message)
    {
        StatusTextBlock.Text = message;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        rfidService.Cleanup();
    }

    private void PlansListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Populate RFIDUnits
        if (e.AddedItems.Count > 0)
        {
            var selectedPlan = e.AddedItems[0] as ServicePlanItem;
            if (selectedPlan != null)
            {
                RFIDUnits.Clear();

                foreach (var cycle in selectedPlan.Cycles)
                {
                    foreach (var unitprogram in cycle.UnitPrograms)
                    {
                        if (unitprogram.ProgramId > 0)
                        {
                            var item = new RFIDUnit()
                            {
                                Cycle = cycle,
                                UnitProgram = unitprogram,
                                IsDone = false
                            };
                            RFIDUnits.Add(item);
                        }
                    }
                }
            }

        }
    }

    private byte[] GetFixedLengthString(string? input, int length = 20)
    {
        byte[] result = new byte[length];

        // Start with all zeros (null bytes)
        for (int i = 0; i < length; i++)
        {
            result[i] = 0;
        }

        // If input is null or empty, return array of nulls
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        // Get ASCII bytes
        byte[] stringBytes = System.Text.Encoding.ASCII.GetBytes(input);
        int copyLength = Math.Min(stringBytes.Length, length);

        // Copy bytes to result array
        Array.Copy(stringBytes, result, copyLength);

        return result;
    }

    private struct StepData
    {
        public byte STEP_ID; // full byte (value range: 0-255)
        public byte[] STEP_NAME; // 20 bytes
        public UInt16 STEP_DURATION; // full 2 bytes (value range: 0-65535)
        public byte STEP_PHYSICAL_UNIT; // full byte (value range: 0-255)
        public Int16 STEP_TARGET_VALUE; // full 2 bytes (value range: -32768-32768)
        public byte STEP_MODE; // full byte (value range: 0-255)
        public byte STEP_TERMINATION1; // full byte (value range: 0-255)
        public byte STEP_TERMINATION2; // full byte (value range: 0-255)
    }

    private struct ProgramData
    {
        public byte PROGRAM_DEVICE_CLASS; // first 5 bits (value range: 0-31)
        public byte PROGRAM_DEVICE_GROUP; // first 5 bits (value range: 0-31)
        public byte NUMBER_OF_PROGRAMS; // first 6 bits (value range: 0-63)
        public byte PROGRAM_NUMBER; // full byte (value range: 0-255)
        public byte PROGRAM_NUMBER_OF_STEPS; // full byte (value range: 0-255)
        public byte[] PROGRAM_NAME; // 20 bytes
        public StepData[] STEPS;
    }

    private string? ProgramToString(RFIDUnit rfidunit)
    {
        if (rfidunit == null)
        {
            return null;
        }

        ProgramData programData = new ProgramData();
        if (rfidunit.UnitProgram != null && rfidunit.UnitProgram.Unit != null)
        {
            programData.PROGRAM_DEVICE_CLASS = rfidunit.UnitProgram.Unit.DeviceClass;
            programData.PROGRAM_DEVICE_GROUP = rfidunit.UnitProgram.Unit.DeviceGroup;
        }
        else
        {
            ShowErrorMessage("Program data is incomplete");
            return null;
        }
        programData.NUMBER_OF_PROGRAMS = 1;
        if (rfidunit.Program == null)
        {
            ShowErrorMessage("No program found for RFID unit");
            return null;
        }
        if (rfidunit.Program.StepIds.Count == 0)
        {
            ShowErrorMessage("Program has no steps defined");
            return null;
        }
        programData.PROGRAM_NUMBER = (byte)rfidunit.Program.Id;
        programData.PROGRAM_NUMBER_OF_STEPS = (byte)rfidunit.Program.StepIds.Count;
        programData.PROGRAM_NAME = GetFixedLengthString(rfidunit.Program.Name);
        programData.STEPS = new StepData[rfidunit.Program.StepIds.Count];
        for (int i = 0; i < rfidunit.Program.StepIds.Count; i++)
        {
            var step = DataService.Instance.Steps.FirstOrDefault(s => s.Id == rfidunit.Program.StepIds[i]);
            if (step != null)
            {
                programData.STEPS[i].STEP_ID = (byte)step.Id;
                programData.STEPS[i].STEP_NAME = GetFixedLengthString(step.Name);
                programData.STEPS[i].STEP_DURATION = (UInt16)step.DurationSeconds;
                programData.STEPS[i].STEP_PHYSICAL_UNIT = 0;
                programData.STEPS[i].STEP_TARGET_VALUE = (Int16)step.FinalTemperature;
                programData.STEPS[i].STEP_MODE = (byte)(step.IsSteamerActive ? 1 : 0);
                programData.STEPS[i].STEP_TERMINATION1 = (byte)(step.TerminateIfTimeExpired ? 1 : 0);
                programData.STEPS[i].STEP_TERMINATION2 = (byte)(step.TerminateIfTemperatureReached ? 2 : 0);
            }
        }

        // Convert ProgramData to hex data

        // Calculate exact size needed: 
        // 2 bytes (header) + 1 byte (program number) + 1 byte (step count) + 
        // 20 bytes (program name) + (steps * 29 bytes per step)
        int bytesPerStep = 1 + 20 + 2 + 1 + 2 + 1 + 1 + 1; // 29 bytes per step
        int dataSize = 2 + 1 + 1 + 20 + (programData.STEPS.Length * bytesPerStep);
        byte[] hexData = new byte[dataSize];
        int index = 0;

        ushort combinedValue =  (ushort)(
                                ((programData.PROGRAM_DEVICE_CLASS & 0x1F) << 11) |  // 5 bits, shifted to positions 11-15
                                ((programData.PROGRAM_DEVICE_GROUP & 0x1F) << 6) |   // 5 bits, shifted to positions 6-10
                                (programData.NUMBER_OF_PROGRAMS & 0x3F)              // 6 bits in positions 0-5
                                );

        hexData[index++] = programData.PROGRAM_NUMBER;
        hexData[index++] = programData.PROGRAM_NUMBER_OF_STEPS;
        for (int i = 0; i < programData.PROGRAM_NAME.Length; i++)
        {
            hexData[index++] = programData.PROGRAM_NAME[i];
        }
        for (int i = 0; i < programData.STEPS.Length; i++)
        {
            hexData[index++] = programData.STEPS[i].STEP_ID;
            for (int j = 0; j < programData.STEPS[i].STEP_NAME.Length; j++)
            {
                hexData[index++] = programData.STEPS[i].STEP_NAME[j];
            }
            hexData[index++] = (byte)(programData.STEPS[i].STEP_DURATION >> 8);
            hexData[index++] = (byte)(programData.STEPS[i].STEP_DURATION & 0xFF);
            hexData[index++] = programData.STEPS[i].STEP_PHYSICAL_UNIT;
            hexData[index++] = (byte)(programData.STEPS[i].STEP_TARGET_VALUE >> 8);
            hexData[index++] = (byte)(programData.STEPS[i].STEP_TARGET_VALUE & 0xFF);
            hexData[index++] = programData.STEPS[i].STEP_MODE;
            hexData[index++] = programData.STEPS[i].STEP_TERMINATION1;
            hexData[index++] = programData.STEPS[i].STEP_TERMINATION2;
        }

        // return hexData as string
        string hexString = BitConverter.ToString(hexData).Replace("-", "");
        return hexString;
    }

    private async Task<string> WriteDataToTag(int startBlock, string data)
    {
        // Split the data into blocks of 4 bytes each (8 hex characters per block)
        int blockSize = 8;
        int totalBlocks = (int)Math.Ceiling(data.Length / (double)blockSize);
        int rem = data.Length % blockSize; // Remaining characters in the last block
        if (rem > 0)
        {
            // Pad the last block with zeros if it has less than 8 characters
            data = data.PadRight(totalBlocks * blockSize, '0');
        }

        for (int i = 0; i < totalBlocks; i++)
        {
            // Extract the block data (4 bytes = 8 hex characters)
            string blockData = data.Substring(i * 8, 8);

            var writeResponse = await rfidService.SendCommandAndWaitAsync(rfidService.Command.ISO15693_WriteSingleBlock((ushort)i, blockData), 4);
            if (!writeResponse.StartsWith("0001"))
            {
                // If any block write fails, return the error response
                return writeResponse;
            }
        }
        return "0001"; // Return success response
    }

    private async void Program_Button_Click(object sender, RoutedEventArgs e)
    {
        // First send SearchTag
        var response = await rfidService.SendCommandAndWaitAsync(rfidService.Command.SearchTag(), 18);

        if (response.StartsWith("0001"))
        {
            var button = sender as Button;
            if (button == null) return;

            var rfidUnit = button.DataContext as RFIDUnit;
            if (rfidUnit == null) return;

            var r = ProgramToString(rfidUnit);
            if (r == null) return;

            // Write to RFID tag (ISO15693_WriteSingleBlock)
            var writeResponse = await WriteDataToTag(0, r);

            if (writeResponse.StartsWith("0001"))
            {
                // Send a beep
                rfidService.SendData(rfidService.Command.BeepShort());
                // Mark as done
                rfidUnit.IsDone = true;
            }
            else
            {
                ShowErrorMessage("Failed to write program to RFID tag");
                rfidService.SendData(rfidService.Command.TurnOffGreenLED());
                rfidService.SendData(rfidService.Command.TurnOnRedLED());
                rfidService.SendData(rfidService.Command.BeepLong());
                await Task.Delay(3000);
                rfidService.SendData(rfidService.Command.TurnOffRedLED());
                rfidService.SendData(rfidService.Command.TurnOnGreenLED());
            }
        }
        else
        {
            ShowErrorMessage("Failed to detect RFID tag");
            rfidService.SendData(rfidService.Command.TurnOffGreenLED());
            rfidService.SendData(rfidService.Command.TurnOnRedLED());
            rfidService.SendData(rfidService.Command.BeepLong());
            await Task.Delay(3000);
            rfidService.SendData(rfidService.Command.TurnOffRedLED());
            rfidService.SendData(rfidService.Command.TurnOnGreenLED());
        }
    }
}
