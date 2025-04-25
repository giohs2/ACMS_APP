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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StepsPage : Page
    {
        private StepItem selectedStep;

        public ObservableCollection<StepItem> Steps => DataService.Instance.Steps;

        public StepsPage()
        {
            this.InitializeComponent();
            StepsListView.ItemsSource = Steps;
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var newStep = new StepItem
            {
                Id = DataService.Instance.NextStepId,
                Name = string.Empty,
                FinalTemperature = 0,
                DurationSeconds = 0,
                IsSteamerActive = false,
                TerminateIfTemperatureReached = false,
                TerminateIfTimeExpired = false
            };

            DataService.Instance.IncrementNextStepId();
            Steps.Add(newStep);
            StepsListView.SelectedItem = newStep;
            FormPanel.Visibility = Visibility.Visible;
            StepId.Text = "Step Id: " + newStep.Id.ToString();
            DataContext = newStep;
        }

        private void StepsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StepsListView.SelectedItem is StepItem selectedItem)
            {
                selectedStep = selectedItem;
                FormPanel.Visibility = Visibility.Visible;
                StepId.Text = "Step Id: " + selectedStep.Id.ToString();
                DataContext = selectedStep;
            }
        }
    }
}
