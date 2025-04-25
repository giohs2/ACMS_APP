using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ACMS_Program_Planner.Models
{
    public class ProgramItem : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private ObservableCollection<int> stepIds = new ObservableCollection<int>();

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

        public ObservableCollection<int> StepIds
        {
            get => stepIds;
            set
            {
                stepIds = value;

                if (stepIds != null)
                {
                    stepIds.CollectionChanged += StepIds_CollectionChanged;
                }

                OnPropertyChanged(nameof(StepIds));
            }
        }

        private void StepIds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(StepIds));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
