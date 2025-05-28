using ACMS_Program_Planner.Models;
using ACMS_Program_Planner.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServicePlanPage : Page, INotifyPropertyChanged
    {
        private ServicePlanItem? selectedServicePlan;
        private Cycle? selectedStep;
        private UnitProgram? selectedUnitProgram;

        public ServicePlanPage()
        {
            InitializeComponent();
        }

        public ObservableCollection<ServicePlanItem> ServicePlans => DataService.Instance.ServicePlans;

        private ObservableCollection<Cycle> _serviceSteps = new ObservableCollection<Cycle>();
        public ObservableCollection<Cycle> ServiceSteps
        {
            get => _serviceSteps;
            set
            {
                if (_serviceSteps != value)
                {
                    _serviceSteps = value;
                    OnPropertyChanged(nameof(ServiceSteps));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newPlan = new ServicePlanItem
            {
                Id = DataService.Instance.NextServicePlanId,
                Name = string.Empty
            };

            DataService.Instance.IncrementNextServicePlanId();
            ServicePlans.Add(newPlan);
            PlansListView.SelectedItem = newPlan;
            FormPanel.Visibility = Visibility.Visible;
            PlanId.Text = "Service Plan Id: " + newPlan.Id.ToString();
            DataContext = newPlan;
        }

        private void PlansListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlansListView.SelectedItem is ServicePlanItem selectedItem)
            {
                selectedServicePlan = selectedItem;
                FormPanel.Visibility = Visibility.Visible;
                PlanId.Text = "Service Plan Id: " + selectedServicePlan.Id.ToString();
                DataContext = selectedServicePlan;
                ServiceSteps = selectedServicePlan.Cycles;
                FlightsComboBox.ItemsSource = DataService.Instance.Flights;
                var selectedFlight = DataService.Instance.Flights.FirstOrDefault(f => f.CombinedFlightName == selectedServicePlan?.Flight?.CombinedFlightName);
                if (selectedFlight != null)
                {
                    FlightsComboBox.SelectedItem = selectedFlight;
                }
                else
                {
                    FlightsComboBox.SelectedIndex = -1;
                    AddCycleButton.IsEnabled = false;
                }
                ItemDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void Add_CycleButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedServicePlan == null) return;

            ObservableCollection<UnitProgram> unitPrograms = new ObservableCollection<UnitProgram>();
            foreach (var unit in DataService.Instance.Flights[FlightsComboBox.SelectedIndex].Units)
            {
                unitPrograms.Add(new UnitProgram
                {
                    Unit = unit,
                    CycleId = ServiceSteps.Count + 1,
                    Name = "Cycle " + (ServiceSteps.Count + 1).ToString() + ": " + unit.UnitName,
                    ProgramId = -1,
                    //StartOffset = 0.0f
                });
            }

            var newCycle = new Cycle
            {
                CycleId = ServiceSteps.Count + 1,
                Name = "Cycle " + (ServiceSteps.Count + 1).ToString(),
                UnitPrograms = unitPrograms
            };

            selectedServicePlan.Cycles.Add(newCycle);
            CyclesTreeView.SelectedItem = newCycle;
        }

        private void FlightsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlightsComboBox.SelectedItem is Flight selectedItem)
            {
                if (selectedServicePlan != null)
                {
                    selectedServicePlan.Flight = selectedItem;
                    AddCycleButton.IsEnabled = true;
                }
            }
            else
            {
                AddCycleButton.IsEnabled = false;
            }
        }

        private void ProgramsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as ProgramItem;
                if (item != null)
                {
                    var step = ServiceSteps.FirstOrDefault(x => x.CycleId == selectedStep?.CycleId);
                    if (step != null)
                    {
                        var unitProgram = step.UnitPrograms.FirstOrDefault(x => x.Unit?.UnitNumber == selectedUnitProgram?.Unit?.UnitNumber);
                        if (unitProgram != null)
                        {
                            unitProgram.ProgramId = item.Id;
                        }
                    }
                }
            }
        }

        private void DeletePlan_Click(object sender, RoutedEventArgs e)
        {
            // Get the clicked item
            var menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem == null) return;

            // Get the data context (ServicePlanItem) of the right-clicked item
            var servicePlanItem = (menuFlyoutItem.DataContext as ServicePlanItem);
            if (servicePlanItem == null) return;

            // Remove the item from the collection
            ServicePlans.Remove(servicePlanItem);

            // Clear the form if the deleted item was selected
            if (FormPanel.DataContext == servicePlanItem)
            {
                FormPanel.Visibility = Visibility.Collapsed;
                FormPanel.DataContext = null;
            }
        }

        private void CyclesTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
        {
            ItemDetails.Visibility = Visibility.Visible;

            if (args.AddedItems.Count > 0 && args.AddedItems[0] is Cycle selectedItem)
            {
                selectedStep = selectedItem;
                ItemTitle.Text = selectedItem.Name;

                CycleSchedule.SelectedTime = selectedItem.Delay;
                CycleSchedule.Visibility = Visibility.Visible;

                ProgramsComboBoxTitle.Visibility = Visibility.Collapsed;
                ProgramsComboBox.Visibility = Visibility.Collapsed;
                //StartOffsetNumberBoxTitle.Visibility = Visibility.Collapsed;
                //StartOffsetNumberBox.Visibility = Visibility.Collapsed;
            }
            else if (args.AddedItems.Count > 0 && args.AddedItems[0] is UnitProgram selectedUnit)
            {
                selectedStep = ServiceSteps.FirstOrDefault(x => x.CycleId == selectedUnit.CycleId);
                CycleSchedule.Visibility = Visibility.Collapsed;
                selectedUnitProgram = selectedUnit;
                ItemTitle.Text = selectedUnit.Name;

                ProgramsComboBoxTitle.Visibility = Visibility.Visible;
                ProgramsComboBoxTitle.Text = "Program";
                ProgramsComboBox.Visibility = Visibility.Visible;
                ProgramsComboBox.ItemsSource = DataService.Instance.Programs;
                var selectedProgram = DataService.Instance.Programs.FirstOrDefault(f => f.Id == selectedUnit?.ProgramId);
                ProgramsComboBox.SelectedItem = selectedProgram;

                //StartOffsetNumberBoxTitle.Visibility = Visibility.Visible;
                //StartOffsetNumberBoxTitle.Text = "Start Offset [sec]";
                //StartOffsetNumberBox.Visibility = Visibility.Visible;
                //StartOffsetNumberBox.Value = selectedUnit.StartOffset;
            }
        }

        private void CycleSchedule_SelectedTimeChanged(TimePicker sender, TimePickerSelectedValueChangedEventArgs args)
        {
            var cycle = ServiceSteps.FirstOrDefault(x => x.CycleId == selectedStep?.CycleId);
            if (cycle != null)
            {
                cycle.Delay = args.NewTime;
            }
        }

        /*private void StartOffsetNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var step = ServiceSteps.FirstOrDefault(x => x.CycleId == selectedStep?.CycleId);
            if (step != null)
            {
                var unitProgram = step.UnitPrograms.FirstOrDefault(x => x.Unit?.UnitNumber == selectedUnitProgram?.Unit?.UnitNumber);
                if (unitProgram != null)
                {
                    unitProgram.StartOffset = (float)args.NewValue;
                }
            }
        }*/
    }

    class CycleItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? CycleTemplate { get; set; }
        public DataTemplate? UnitTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is Cycle && CycleTemplate != null)
                return CycleTemplate;
            else if (item is UnitProgram && UnitTemplate != null)
                return UnitTemplate;
            else
                return base.SelectTemplateCore(item);
        }
    }
}
