using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ACMS_Program_Planner.Models
{
    public class RFIDUnit : INotifyPropertyChanged
    {
        public string? CycleUnit { get; set; }
        public string? ProgramName
        {
            get
            {
                return Program.Name;
            }
        }

        public ProgramItem? Program { get; set; }

        private bool _isDone { get; set; }
        public bool IsDone
        {
            get => _isDone;
            set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    OnPropertyChanged(nameof(IsDone));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
