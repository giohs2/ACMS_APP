using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class Cycle : INotifyPropertyChanged
    {
        private string? _name;
        private int _cycleId;
        private TimeSpan? _delay;
        private ObservableCollection<UnitProgram> _unitPrograms = new ObservableCollection<UnitProgram>();

        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int CycleId
        {
            get => _cycleId;
            set
            {
                if (_cycleId != value)
                {
                    _cycleId = value;
                    OnPropertyChanged(nameof(CycleId));
                }
            }
        }

        public TimeSpan? Delay
        {
            get => _delay;
            set
            {
                if (_delay != value)
                {
                    _delay = value;
                    OnPropertyChanged(nameof(Delay));
                }
            }
        }

        public ObservableCollection<UnitProgram> UnitPrograms
        {
            get => _unitPrograms;
            set
            {
                if (_unitPrograms != value)
                {
                    _unitPrograms = value;
                    OnPropertyChanged(nameof(UnitPrograms));
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
