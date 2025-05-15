using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using ACMS_Program_Planner.Models;
using ACMS_Program_Planner.Services;
using Windows.ApplicationModel.DataTransfer;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgramsPage : Page, INotifyPropertyChanged
    {
        private ProgramItem? selectedProgram;
        private StepItem? selectedStep;

        public ProgramsPage()
        {
            InitializeComponent();
        }

        public ObservableCollection<ProgramItem> Programs => DataService.Instance.Programs;
        public ObservableCollection<StepItem> Steps => DataService.Instance.Steps;

        private ObservableCollection<StepItem> selectedSteps { get; set; } = new ObservableCollection<StepItem>();

        public ObservableCollection<StepItem> SelectedSteps
        {
            get => selectedSteps;
            set
            {
                selectedSteps = value;
                OnPropertyChanged(nameof(SelectedSteps));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            var newProgram = new ProgramItem
            {
                Id = DataService.Instance.NextProgramId,
                Name = "",
                StepIds = new ObservableCollection<int>()
            };

            DataService.Instance.IncrementNextProgramId();
            Programs.Add(newProgram);
            ProgramsListView.SelectedItem = newProgram;
        }

        private void UpdateSelectedStepsList()
        {
            if (selectedProgram != null && selectedProgram.StepIds != null)
            {
                selectedSteps.Clear();
                foreach (var stepId in selectedProgram.StepIds)
                {
                    var step = Steps.FirstOrDefault(s => s.Id == stepId);
                    if (step != null)
                    {
                        selectedSteps.Add(step);
                    }
                }
                OnPropertyChanged(nameof(SelectedSteps));
            }
        }

        private void ProgramsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProgramsListView.SelectedItem is ProgramItem selectedItem)
            {
                selectedProgram = selectedItem;
                ProgramGrid.Visibility = Visibility.Visible;
                ProgramId.Text = "Program Id: " + selectedProgram.Id.ToString();
                DataContext = selectedProgram;

                UpdateSelectedStepsList();

                AvailableStepsListView.SelectedItem = null;
                SelectedStepsListView.SelectedItem = null;
                InfoPanel.Visibility = Visibility.Visible;
                StepDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void AvailableStepsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableStepsListView.SelectedItem is StepItem selectedItem)
            {
                selectedStep = selectedItem;
                InfoPanel.Visibility = Visibility.Collapsed;
                StepDetails.Visibility = Visibility.Visible;
                StepId.Text = "Step Id: " + selectedStep.Id.ToString();
                StepName.Text = "Step Name: " + selectedStep.Name;
                StepTime.Text = "Step Time: " + selectedStep.DurationSeconds.ToString() + " seconds";
                StepTemperature.Text = "Step Temperature: " + selectedStep.FinalTemperature.ToString() + " °C";
                IsSteamerActive.Text = "Steamer Active: " + (selectedStep.IsSteamerActive ? "Yes" : "No");
                TerminateIfTemperatureReached.Text = "Temperature Reached: " + (selectedStep.TerminateIfTemperatureReached ? "Yes" : "No");
                TerminateIfTimeExpired.Text = "Time Expired: " + (selectedStep.TerminateIfTimeExpired ? "Yes" : "No");

                SelectedStepsListView.SelectedItem = null; // Deselect any selected item in SelectedStepsListView
            }
        }

        private void SelectedStepsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedStepsListView.SelectedItem is StepItem selectedItem)
            {
                selectedStep = selectedItem;
                InfoPanel.Visibility = Visibility.Collapsed;
                StepDetails.Visibility = Visibility.Visible;
                StepId.Text = "Step Id: " + selectedStep.Id.ToString();
                StepName.Text = "Step Name: " + selectedStep.Name;
                StepTime.Text = "Step Time: " + selectedStep.DurationSeconds.ToString() + " seconds";
                StepTemperature.Text = "Step Temperature: " + selectedStep.FinalTemperature.ToString() + " °C";
                IsSteamerActive.Text = "Steamer Active: " + (selectedStep.IsSteamerActive ? "Yes" : "No");
                TerminateIfTemperatureReached.Text = "Temperature Reached: " + (selectedStep.TerminateIfTemperatureReached ? "Yes" : "No");
                TerminateIfTimeExpired.Text = "Time Expired: " + (selectedStep.TerminateIfTimeExpired ? "Yes" : "No");

                AvailableStepsListView.SelectedItem = null; // Deselect any selected item in AvailableStepsListView
            }
        }

        private async void SelectedStepsListView_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text) && selectedProgram != null)
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                var stepIdString = await e.DataView.GetTextAsync();
                if (int.TryParse(stepIdString, out int stepId))
                {
                    if (selectedProgram.StepIds == null)
                    {
                        selectedProgram.StepIds = new ObservableCollection<int>();
                    }
                    selectedProgram.StepIds.Add(stepId);
                    UpdateSelectedStepsList();
                }
            }
        }

        private void AvailableStepsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var stepItem = e.Items[0] as StepItem;
            if (stepItem == null) return;
            e.Data.SetText(stepItem.Id.ToString());
            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        private void SelectedStepsListView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.Text) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        private void AvailableStepsListView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.Text) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        private void SelectedStepsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var stepItem = e.Items[0] as StepItem;
            if (stepItem == null) return;
            e.Data.SetText(stepItem.Id.ToString());
        }

        private void SelectedStepsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var stepItem = args.Items[0] as StepItem;

            // Check if the item was dropped on AvailableStepsListView
            if (args.DropResult == DataPackageOperation.Copy && selectedProgram != null && stepItem != null)
            {
                selectedProgram.StepIds.Remove(stepItem.Id);
                UpdateSelectedStepsList();
            } 
        }

        private void Visualise_Button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProgram != null)
            {
                Window timelineWin = new ProgramTimeline(selectedProgram);
                var scale = App.windowScaling(timelineWin);
                timelineWin.AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(800 * scale), (int)(600 * scale)));
                timelineWin.Activate();
            }
        }
    }
}
