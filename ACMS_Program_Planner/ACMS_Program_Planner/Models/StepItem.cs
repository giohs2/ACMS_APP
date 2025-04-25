using System.ComponentModel;

namespace ACMS_Program_Planner.Models
{
    public class StepItem : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private double finalTemperature;
        private int durationSeconds;
        private bool isSteamerActive;
        private bool terminateIfTemperatureReached;
        private bool terminateIfTimeExpired;

        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public double FinalTemperature
        {
            get => finalTemperature;
            set
            {
                if (finalTemperature != value)
                {
                    finalTemperature = value;
                    OnPropertyChanged(nameof(FinalTemperature));
                }
            }
        }

        public int DurationSeconds
        {
            get => durationSeconds;
            set
            {
                if (durationSeconds != value)
                {
                    durationSeconds = value;
                    OnPropertyChanged(nameof(DurationSeconds));
                }
            }
        }

        public bool IsSteamerActive
        {
            get => isSteamerActive;
            set
            {
                if (isSteamerActive != value)
                {
                    isSteamerActive = value;
                    OnPropertyChanged(nameof(IsSteamerActive));
                }
            }
        }

        public bool TerminateIfTemperatureReached
        {
            get => terminateIfTemperatureReached;
            set
            {
                if (terminateIfTemperatureReached != value)
                {
                    terminateIfTemperatureReached = value;
                    OnPropertyChanged(nameof(TerminateIfTemperatureReached));
                }
            }
        }

        public bool TerminateIfTimeExpired
        {
            get => terminateIfTimeExpired;
            set
            {
                if (terminateIfTimeExpired != value)
                {
                    terminateIfTimeExpired = value;
                    OnPropertyChanged(nameof(TerminateIfTimeExpired));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
