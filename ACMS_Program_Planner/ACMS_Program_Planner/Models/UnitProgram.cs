using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class UnitProgram : INotifyPropertyChanged
    {
        private Unit? _unit;
        private int _cycleId;
        private string? _name;
        private int _programId;
        //private double _startOffset;

        public Unit? Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged(nameof(Unit));
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

        public int ProgramId
        {
            get => _programId;
            set
            {
                if (_programId != value)
                {
                    _programId = value;
                    OnPropertyChanged(nameof(ProgramId));
                }
            }
        }

        /*public double StartOffset
        {
            get => _startOffset;
            set
            {
                if (_startOffset != value)
                {
                    _startOffset = value;
                    OnPropertyChanged(nameof(StartOffset));
                }
            }
        }*/

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
