using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class ServicePlanItem : INotifyPropertyChanged
    {
        private int id;
        private string? name;
        private Flight? flight;

        private ObservableCollection<Cycle> cycles = new ObservableCollection<Cycle>();

        public ServicePlanItem()
        {
            cycles.CollectionChanged += (s, args) => { 
                OnPropertyChanged(nameof(Cycles));
                OnPropertyChanged(nameof(IsLockButtonEnabled));
            };
        }

        public Dictionary<int, string> MealtimeType = new Dictionary<int, string>
        {
            { 0, "Breakfast" },
            { 1, "Lunch" },
            { 2, "Dinner" }
        };

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

        public string? Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(IsLockButtonEnabled));
                }
            }
        }

        public Flight? Flight
        {
            get => flight;
            set
            {
                if (flight != value)
                {
                    flight = value;
                    OnPropertyChanged(nameof(Flight));
                    OnPropertyChanged(nameof(FlightName));
                    OnPropertyChanged(nameof(IsLockButtonEnabled));
                }
            }
        }

        public string? FlightName => Flight?.CombinedFlightName;

        public ObservableCollection<Cycle> Cycles
        {
            get => cycles;
            set
            {
                if (cycles != value)
                {
                    cycles = value;
                    OnPropertyChanged(nameof(Cycles));
                    OnPropertyChanged(nameof(IsLockButtonEnabled));
                }
            }
        }

        private bool _isEditable = true;
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged(nameof(IsEditable));
                }
            }
        }

        private bool IsServicePlanValid()
        {
            if (!IsEditable ||
                string.IsNullOrWhiteSpace(Name) ||
                Flight == null ||
                Cycles.Count == 0)
            {
                return false;
            }

            return true;
        }

        public bool IsLockButtonEnabled
        {
            get => IsServicePlanValid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
