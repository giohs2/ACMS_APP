using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ACMS_Program_Planner.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        // Database connection settings
        /*private string _serverAddress;
        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        private string _databaseName;
        public string DatabaseName
        {
            get => _databaseName;
            set => SetProperty(ref _databaseName, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // Application settings
        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set => SetProperty(ref _darkModeEnabled, value);
        }

        private int _refreshInterval = 60; // Default 60 seconds
        public int RefreshInterval
        {
            get => _refreshInterval;
            set => SetProperty(ref _refreshInterval, value);
        }*/

        // RFID Settings
        private string _selectedComPort = "COM1";
        public string SelectedComPort
        {
            get => _selectedComPort;
            set => SetProperty(ref _selectedComPort, value);
        }

        private ObservableCollection<string> _comPorts = [ "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10" ];
        public ObservableCollection<string> ComPorts
        {
            get => _comPorts;
            set => SetProperty(ref _comPorts, value);
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

        // Singleton pattern to ensure the settings can be accessed globally
        private static SettingsModel? _instance;
        public static SettingsModel Instance => _instance ??= new SettingsModel();

        private SettingsModel()
        {
            // Initialize with default values or load from storage
            LoadSettings();
        }
    }
}
