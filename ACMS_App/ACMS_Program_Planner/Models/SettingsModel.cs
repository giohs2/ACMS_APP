using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace ACMS_Program_Planner.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        private DispatcherQueue? _dispatcherQueue;

        // Singleton pattern to ensure the settings can be accessed globally
        private static SettingsModel? _instance;
        public static SettingsModel Instance => _instance ??= new SettingsModel();

        public static void Initialize(DispatcherQueue dispatcherQueue)
        {
            if (_instance == null)
            {
                _instance = new SettingsModel();
            }
            _instance._dispatcherQueue = dispatcherQueue;
        }

        private SettingsModel()
        {
            // Initialize with default values or load from storage
            InitializeComPorts();
            StartComPortMonitoring();
            LoadSettings();
        }

        // RFID Settings
        private ObservableCollection<string> _comPorts = new ObservableCollection<string>();
        public ObservableCollection<string> ComPorts
        {
            get => _comPorts;
            set => SetProperty(ref _comPorts, value);
        }

        private string _selectedComPort = string.Empty;
        public string SelectedComPort
        {
            get => _selectedComPort;
            set => SetProperty(ref _selectedComPort, value);
        }

        private ObservableCollection<int> _baudRates = [300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200];
        public ObservableCollection<int> BaudRates
        {
            get => _baudRates;
        }

        private int _selectedBaudRate = 9600;
        public int SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
        }

        public IEnumerable<System.IO.Ports.Parity> Parities
        {
            get
            {
                return Enum.GetValues(typeof(System.IO.Ports.Parity)).Cast<System.IO.Ports.Parity>();
            }
        }

        private System.IO.Ports.Parity _selectedParity = System.IO.Ports.Parity.None;
        public System.IO.Ports.Parity SelectedParity
        {
            get => _selectedParity;
            set => SetProperty(ref _selectedParity, value);
        }

        private ObservableCollection<int> _dataBits = [5, 6, 7, 8];
        public ObservableCollection<int> DataBits
        {
            get => _dataBits;
        }

        private int _selectedDataBit = 8;
        public int SelectedDataBit
        {
            get => _selectedDataBit;
            set => SetProperty(ref _selectedDataBit, value);
        }

        public IEnumerable<System.IO.Ports.StopBits> StopBits
        {
            get
            {
                return Enum.GetValues(typeof(System.IO.Ports.StopBits)).Cast<System.IO.Ports.StopBits>();
            }
        }

        private System.IO.Ports.StopBits _selectedStopBit = System.IO.Ports.StopBits.One;
        public System.IO.Ports.StopBits SelectedStopBit
        {
            get => _selectedStopBit;
            set => SetProperty(ref _selectedStopBit, value);
        }

        public string? AppVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                }
                return string.Empty;
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Methods to save and load settings
        public void SaveSettings()
        {
            // Implement saving settings to local storage or configuration file
        }

        public void LoadSettings()
        {
            // Implement loading settings from local storage or configuration file
        }

        private void InitializeComPorts()
        {
            _comPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            if (ComPorts.Count > 0)
                SelectedComPort = ComPorts.First();
        }

        private Timer? _comPortMonitorTimer;

        private void StartComPortMonitoring()
        {
            _comPortMonitorTimer = new Timer(2000); // Check every 2 seconds
            _comPortMonitorTimer.Elapsed += CheckForNewComPorts;
            _comPortMonitorTimer.Start();
        }

        private void CheckForNewComPorts(object? sender, ElapsedEventArgs e)
        {
            var currentPorts = SerialPort.GetPortNames();
            var newPorts = currentPorts.Except(ComPorts ?? Enumerable.Empty<string>()).ToList();

            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (currentPorts.Length == 0)
                    {
                        ComPorts?.Clear();
                        OnPropertyChanged(nameof(ComPorts));
                    }
                    else
                    {
                        foreach (var port in newPorts)
                        {
                            ComPorts?.Add(port);
                        }
                        if (newPorts.Count > 0)
                        {
                            OnPropertyChanged(nameof(ComPorts));
                            // Automatically select the first available COM port if not already selected
                            if (ComPorts != null && ComPorts.Any() && string.IsNullOrEmpty(SelectedComPort))
                            {
                                SelectedComPort = ComPorts.First();
                            }
                        }
                    }
                });
            }
        }
    }
}
