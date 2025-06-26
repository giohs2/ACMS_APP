using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ACMS_Program_Planner.Models;

namespace ACMS_Program_Planner.Services
{
    /**
     * @class DataService
     * @brief A service class responsible for managing and persisting data.
     *
     * This class provides functionality for saving the current state of the data model to a file in JSON format.
     */
    public class DataService
    {
        private static DataService? _instance;
        public static DataService Instance => _instance ??= new DataService();

        private static readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ipeco", "ACMS_Program_Planner");
        private static readonly string FilePath = Path.Combine(DirectoryPath, "data.json");

        private ObservableCollection<Unit> units1 = new ObservableCollection<Unit>();
        private ObservableCollection<Unit> units2 = new ObservableCollection<Unit>();

        public ObservableCollection<StepItem> Steps { get; } = new ObservableCollection<StepItem>();
        public ObservableCollection<ProgramItem> Programs { get; } = new ObservableCollection<ProgramItem>();
        public ObservableCollection<ServicePlanItem> ServicePlans { get; } = new ObservableCollection<ServicePlanItem>();

        public ObservableCollection<Flight> Flights { get; } = new ObservableCollection<Flight>();

        public int NextStepId { get; private set; } = 1;
        public int NextProgramId { get; private set; } = 1;
        public int NextServicePlanId { get; private set; } = 1;

        private DataService()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            var data = LoadData();
            Steps = data.Steps ?? new ObservableCollection<StepItem>();
            Programs = data.Programs ?? new ObservableCollection<ProgramItem>();
            ServicePlans = data.ServicePlans ?? new ObservableCollection<ServicePlanItem>();
            NextStepId = data.NextStepId;
            NextProgramId = data.NextProgramId;
            NextServicePlanId = data.NextServicePlanId;

            Steps.CollectionChanged += OnStepsCollectionChanged;
            Programs.CollectionChanged += OnProgramsCollectionChanged;
            ServicePlans.CollectionChanged += OnServicePlanCollectionChanged;

            foreach (var step in Steps)
            {
                step.PropertyChanged += (s, args) => SaveData();
            }

            foreach (var program in Programs)
            {
                program.PropertyChanged += (s, args) => SaveData();
            }

            foreach (var servicePlan in ServicePlans)
            {
                servicePlan.PropertyChanged += (s, args) => SaveData();
                servicePlan.Cycles.CollectionChanged += OnCyclesCollectionChanged;
                foreach (var cycle in servicePlan.Cycles)
                {
                    cycle.PropertyChanged += (s, args) => SaveData();
                    cycle.UnitPrograms.CollectionChanged += OnUnitProgramsCollectionChanged;
                    foreach (var unitProgram in cycle.UnitPrograms)
                    {
                        unitProgram.PropertyChanged += (s, args) => SaveData();
                    }
                }
            }


            // Fill dummy list of Units
            units1.Add(new Unit { UnitNumber = 1, UnitName = "Oven 1", DeviceClass = 1, DeviceGroup = 2, DeviceId = 3 });
            units1.Add(new Unit { UnitNumber = 2, UnitName = "Oven 2", DeviceClass = 1, DeviceGroup = 2, DeviceId = 4 });
            units2.Add(new Unit { UnitNumber = 3, UnitName = "Oven 3", DeviceClass = 1, DeviceGroup = 2, DeviceId = 5 });
            units2.Add(new Unit { UnitNumber = 4, UnitName = "Oven 4", DeviceClass = 1, DeviceGroup = 2, DeviceId = 6 });

            // fill dummy Flights data
            Flights.Add(new Flight { FlightNumber = "XY123", FlightDate = "01/06/2025", FlightTime = "14:00", Units = units1 });
            Flights.Add(new Flight { FlightNumber = "XY456", FlightDate = "02/06/2025", FlightTime = "15:00", Units = units2 });

            // Some defaults for demo
            if (Steps.Count == 0)
            {
                ResetToDefaults();
            }

        }

        public void ResetToDefaults()
        {
            // Delete the data file if it exists
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            // Clear all collections
            Steps.Clear();
            Programs.Clear();
            ServicePlans.Clear();

            // Reset IDs
            NextStepId = 1;
            NextProgramId = 1;
            NextServicePlanId = 1;

            // Add default steps
            var preheatStep = new StepItem
            {
                Id = NextStepId,
                Name = "Preheat Standard",
                FinalTemperature = 120,
                DurationSeconds = 600,
                IsSteamerActive = true,
                TerminateIfTemperatureReached = true,
                TerminateIfTimeExpired = true
            };
            IncrementNextStepId();
            Steps.Add(preheatStep);

            var steamingStep = new StepItem
            {
                Id = NextStepId,
                Name = "Steaming Standard",
                FinalTemperature = 130,
                DurationSeconds = 1200,
                IsSteamerActive = true,
                TerminateIfTemperatureReached = false,
                TerminateIfTimeExpired = true
            };
            IncrementNextStepId();
            Steps.Add(steamingStep);

            var keepHotStep = new StepItem
            {
                Id = NextStepId,
                Name = "Keep Hot Standard",
                FinalTemperature = 75,
                DurationSeconds = 0,
                IsSteamerActive = true,
                TerminateIfTemperatureReached = false,
                TerminateIfTimeExpired = false
            };
            IncrementNextStepId();
            Steps.Add(keepHotStep);

            // Add default program
            var defaultProgram = new ProgramItem
            {
                Id = NextProgramId,
                Name = "Pasta with Chicken",
                StepIds = new ObservableCollection<int>() { 1, 2, 3 }
            };
            IncrementNextProgramId();
            Programs.Add(defaultProgram);

            var defaultProgram2 = new ProgramItem
            {
                Id = NextProgramId,
                Name = "Beef Stew",
                StepIds = new ObservableCollection<int>() { 1, 2, 3 }
            };
            IncrementNextProgramId();
            Programs.Add(defaultProgram2);

            // Add default service plan
            var newPlan = new ServicePlanItem
            {
                Id = NextServicePlanId,
                Name = "Breakfast",
                Flight = Flights[0]
            };
            IncrementNextServicePlanId();

            ObservableCollection<UnitProgram> unitPrograms = new ObservableCollection<UnitProgram>();
            unitPrograms.Add(new UnitProgram
            {
                Unit = Flights[0].Units[0],
                CycleId = 1,
                Name = "Cycle 1: " + Flights[0].Units[0].UnitName,
                ProgramId = 1,
            });
            unitPrograms.Add(new UnitProgram
            {
                Unit = Flights[0].Units[1],
                CycleId = 1,
                Name = "Cycle 1: " + Flights[0].Units[1].UnitName,
                ProgramId = 2,
            });

            var newCycle = new Cycle
            {
                CycleId = 1,
                Name = "Cycle 1",
                UnitPrograms = unitPrograms,
                Delay = new TimeSpan(1, 15, 0)
            };

            newPlan.Cycles.Add(newCycle);

            ServicePlans.Add(newPlan);

            // Save the default data
            SaveData();
        }


        private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StepItem newItem in e.NewItems)
                {
                    newItem.PropertyChanged += (s, args) => SaveData();
                }
            }

            if (e.OldItems != null)
            {
                foreach (StepItem oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= (s, args) => SaveData();
                }
            }

            SaveData();
        }

        private void OnProgramsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProgramItem newProgram in e.NewItems)
                {
                    newProgram.PropertyChanged += (s, args) => SaveData();
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProgramItem oldProgram in e.OldItems)
                {
                    oldProgram.PropertyChanged -= (s, args) => SaveData();
                }
            }

            SaveData();
        }

        private void OnServicePlanCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ServicePlanItem newServicePlan in e.NewItems)
                {
                    newServicePlan.PropertyChanged += (s, args) => SaveData();

                    // Subscribe to changes in the Cycles collection
                    newServicePlan.Cycles.CollectionChanged += OnCyclesCollectionChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ServicePlanItem oldServicePlan in e.OldItems)
                {
                    oldServicePlan.PropertyChanged -= (s, args) => SaveData();

                    // Unsubscribe from changes in the Cycles collection
                    oldServicePlan.Cycles.CollectionChanged -= OnCyclesCollectionChanged;
                }
            }

            SaveData();
        }

        private void OnCyclesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Cycle newCycle in e.NewItems)
                {
                    newCycle.PropertyChanged += (s, args) => SaveData();

                    newCycle.UnitPrograms.CollectionChanged += OnUnitProgramsCollectionChanged;
                    foreach (var unitProgram in newCycle.UnitPrograms)
                    {
                        unitProgram.PropertyChanged += (s, args) => SaveData();
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Cycle oldCycle in e.OldItems)
                {
                    oldCycle.PropertyChanged -= (s, args) => SaveData();

                    oldCycle.UnitPrograms.CollectionChanged -= OnUnitProgramsCollectionChanged;
                    foreach (var unitProgram in oldCycle.UnitPrograms)
                    {
                        unitProgram.PropertyChanged -= (s, args) => SaveData();
                    }
                }
            }

            SaveData();
        }

        private void OnUnitProgramsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (UnitProgram newUnitProgram in e.NewItems)
                {
                    newUnitProgram.PropertyChanged += (s, args) => SaveData();
                }
            }

            if (e.OldItems != null)
            {
                foreach (UnitProgram oldUnitProgram in e.OldItems)
                {
                    oldUnitProgram.PropertyChanged -= (s, args) => SaveData();
                }
            }

            SaveData();
        }

        
        private (ObservableCollection<StepItem> Steps,
                 ObservableCollection<ProgramItem> Programs,
                 ObservableCollection<ServicePlanItem> ServicePlans,
                 int NextStepId, int NextProgramId, int NextServicePlanId) LoadData()
        {
            if (File.Exists(FilePath))
            {
                var wrapperJson = File.ReadAllText(FilePath);
                var fileWithCrc = JsonSerializer.Deserialize<DataFileWithCrc>(wrapperJson);

                if (fileWithCrc?.DataJson == null)
                {
                    return (new ObservableCollection<StepItem>(), new ObservableCollection<ProgramItem>(), new ObservableCollection<ServicePlanItem>(), 1, 1, 1);
                }

                uint actualCrc = Crc32Helper.Compute(System.Text.Encoding.UTF8.GetBytes(fileWithCrc.DataJson));
                if (actualCrc != fileWithCrc.Crc32)
                    throw new Exception("Data file CRC check failed. The file may have been tampered with.");

                var data = JsonSerializer.Deserialize<DataModel>(fileWithCrc.DataJson);
                return (data?.Steps ?? new ObservableCollection<StepItem>(), 
                        data?.Programs ?? new ObservableCollection<ProgramItem>(),
                        data?.ServicePlans ?? new ObservableCollection<ServicePlanItem>(),
                        data?.NextStepId ?? 1, data?.NextProgramId ?? 1, data?.NextServicePlanId ?? 1);
            }

            return (new ObservableCollection<StepItem>(), new ObservableCollection<ProgramItem>(), new ObservableCollection<ServicePlanItem>(), 1, 1, 1);
        }

        private void SaveData()
        {
            var header = new DataHeader
            {
                Date = DateTime.Now,
                User = System.Environment.MachineName,
                Version = SettingsModel.Instance.AppVersion
            };

            var data = new DataModel
            {
                Header = header,
                Steps = Steps,
                Programs = Programs,
                ServicePlans = ServicePlans,
                NextStepId = NextStepId,
                NextProgramId = NextProgramId,
                NextServicePlanId = NextServicePlanId
            };

            var dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            uint crc = Crc32Helper.Compute(System.Text.Encoding.UTF8.GetBytes(dataJson));

            var fileWithCrc = new DataFileWithCrc
            {
                DataJson = dataJson,
                Crc32 = crc
            };

            var wrapperJson = JsonSerializer.Serialize(fileWithCrc, new JsonSerializerOptions { WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(FilePath, wrapperJson);
        }

        public string GetDataInJson()
        {
            var header = new DataHeader
            {
                Date = DateTime.Now,
                User = System.Environment.MachineName,
                Version = SettingsModel.Instance.AppVersion
            };

            var data = new DataModel
            {
                Header = header,
                Steps = Steps,
                Programs = Programs,
                ServicePlans = ServicePlans,
                NextStepId = NextStepId,
                NextProgramId = NextProgramId,
                NextServicePlanId = NextServicePlanId
            };

            var dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            uint crc = Crc32Helper.Compute(System.Text.Encoding.UTF8.GetBytes(dataJson));

            var fileWithCrc = new DataFileWithCrc
            {
                DataJson = dataJson,
                Crc32 = crc
            };

            return JsonSerializer.Serialize(fileWithCrc, new JsonSerializerOptions { WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
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

        public void IncrementNextServicePlanId()
        {
            NextServicePlanId++;
            SaveData();
        }
    }

    public class DataHeader
    {
        public DateTime Date { get; set; }
        public string? User { get; set; }
        public string? Version { get; set; }
    }

    public class DataModel
    {
        public DataHeader? Header { get; set; }
        public ObservableCollection<StepItem>? Steps { get; set; }
        public ObservableCollection<ProgramItem>? Programs { get; set; }
        public ObservableCollection<ServicePlanItem>? ServicePlans { get; set; }
        public int NextStepId { get; set; }
        public int NextProgramId { get; set; }
        public int NextServicePlanId { get; set; }
    }

    public class PreformattedJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }
            else
            {
                // Read the raw JSON as a string
                using var document = JsonDocument.ParseValue(ref reader);
                return document.RootElement.GetRawText();
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value);
        }
    }

    public class DataFileWithCrc
    {
        [JsonConverter(typeof(PreformattedJsonConverter))]
        public string? DataJson { get; set; }
        public uint Crc32 { get; set; }
    }

    public static class Crc32Helper
    {
        private static readonly uint[] Table = Enumerable.Range(0, 256).Select(i =>
        {
            uint crc = (uint)i;
            for (int j = 0; j < 8; j++)
                crc = (crc & 1) != 0 ? (0xEDB88320 ^ (crc >> 1)) : (crc >> 1);
            return crc;
        }).ToArray();

        public static uint Compute(byte[] bytes)
        {
            uint crc = 0xFFFFFFFF;
            foreach (var b in bytes)
                crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
            return ~crc;
        }
    }
}
