using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ACMS_Program_Planner.Models;

namespace ACMS_Program_Planner.Services
{
    public class DataService
    {
        private static DataService _instance;
        public static DataService Instance => _instance ??= new DataService();

        private static readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ipeco", "ACMS_Program_Planner");
        private static readonly string FilePath = Path.Combine(DirectoryPath, "data.json");

        public ObservableCollection<StepItem> Steps { get; }
        public ObservableCollection<ProgramItem> Programs { get; }
        public int NextStepId { get; private set; } = 1;
        public int NextProgramId { get; private set; } = 1;

        private DataService()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            var data = LoadData();
            Steps = data.Steps ?? new ObservableCollection<StepItem>();
            Programs = data.Programs ?? new ObservableCollection<ProgramItem>();
            NextStepId = data.NextStepId;
            NextProgramId = data.NextProgramId;

            Steps.CollectionChanged += OnStepsCollectionChanged;
            Programs.CollectionChanged += OnProgramsCollectionChanged;

            foreach (var step in Steps)
            {
                step.PropertyChanged += OnStepItemPropertyChanged;
            }

            foreach (var program in Programs)
            {
                program.PropertyChanged += OnProgramPropertyChanged;
            }
        }

        private void OnStepsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StepItem newItem in e.NewItems)
                {
                    newItem.PropertyChanged += OnStepItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StepItem oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= OnStepItemPropertyChanged;
                }
            }

            SaveData();
        }

        private void OnProgramsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProgramItem newProgram in e.NewItems)
                {
                    newProgram.PropertyChanged += OnProgramPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProgramItem oldProgram in e.OldItems)
                {
                    oldProgram.PropertyChanged -= OnProgramPropertyChanged;
                }
            }

            SaveData();
        }

        private void OnStepItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveData();
        }

        private void OnProgramPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveData();
        }

        private (ObservableCollection<StepItem> Steps, ObservableCollection<ProgramItem> Programs, int NextStepId, int NextProgramId) LoadData()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize<DataModel>(json);
                return (data?.Steps ?? new ObservableCollection<StepItem>(), data?.Programs ?? new ObservableCollection<ProgramItem>(), data?.NextStepId ?? 1, data?.NextProgramId ?? 1);
            }

            return (new ObservableCollection<StepItem>(), new ObservableCollection<ProgramItem>(), 1, 1);
        }

        private void SaveData()
        {
            var data = new DataModel
            {
                Steps = Steps,
                Programs = Programs,
                NextStepId = NextStepId,
                NextProgramId = NextProgramId
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public void IncrementNextStepId()
        {
            NextStepId++;
            SaveData();
        }

        public void IncrementNextProgramId()
        {
            NextProgramId++;
            SaveData();
        }
    }

    public class DataModel
    {
        public ObservableCollection<StepItem> Steps { get; set; }
        public ObservableCollection<ProgramItem> Programs { get; set; }
        public int NextStepId { get; set; }
        public int NextProgramId { get; set; }
    }
}
