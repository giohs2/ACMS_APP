using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using ACMS_Program_Planner.Models;

namespace ACMS_Program_Planner.Services
{
    public class RFIDService : IRFIDService
    {
        private SerialPort? serialPort;
        private bool isConnected = false;
        private string _lastReceivedData = string.Empty;
        private bool _awaitingResponse = false;
        private readonly RFIDCommandTWN4 twn4Command = new RFIDCommandTWN4();
        private readonly DispatcherQueue dispatcherQueue;

        public event Action<string>? DataReceived;
        public event Action<string>? ErrorOccurred;
        public event Action<bool>? ConnectionChanged;

        public RFIDCommandTWN4 Command => twn4Command;

        public RFIDService(DispatcherQueue dispatcherQueue)
        {
            this.dispatcherQueue = dispatcherQueue;
        }

        public bool IsConnected => isConnected;

        public void Connect(SettingsModel settings)
        {
            if (isConnected) return;
            try
            {
                serialPort = new SerialPort
                {
                    PortName = settings.SelectedComPort,
                    BaudRate = settings.SelectedBaudRate,
                    DataBits = settings.SelectedDataBit,
                    Parity = settings.SelectedParity,
                    StopBits = settings.SelectedStopBit,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
                isConnected = true;
                ConnectionChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Failed to connect: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (!isConnected) return;
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                }
                isConnected = false;
                ConnectionChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Failed to disconnect: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.DataReceived -= SerialPort_DataReceived;
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }
            isConnected = false;
        }

        public void SendData(string? data)
        {
            if (!isConnected || serialPort == null || !serialPort.IsOpen)
            {
                ErrorOccurred?.Invoke("Not connected to a serial port");
                return;
            }
            try
            {
                if (data == null)
                {
                    ErrorOccurred?.Invoke("No data to send");
                    return;
                }
                string textToSend = data + '\r';
                if (!string.IsNullOrEmpty(textToSend))
                {
                    serialPort.Write(textToSend);
                    DataReceived?.Invoke("SENT: " + textToSend);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Failed to send data: {ex.Message}");
            }
        }

        public async Task<string> SendCommandAndWaitAsync(string command)
        {
            _awaitingResponse = true;
            _lastReceivedData = string.Empty;
            SendData(command);
            while (_awaitingResponse)
            {
                await Task.Delay(50);
            }
            return _lastReceivedData;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
                return;
            try
            {
                string data = serialPort.ReadExisting();
                _lastReceivedData = data;
                _awaitingResponse = false;
                dispatcherQueue.TryEnqueue(() =>
                {
                    DataReceived?.Invoke("RECV: " + data);
                });
            }
            catch (Exception ex)
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    ErrorOccurred?.Invoke($"Error reading data: {ex.Message}");
                });
            }
        }
    }
}
