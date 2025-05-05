using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ACMS_Program_Planner.Models;
using ACMS_Program_Planner.Services;
using Microsoft.UI.Composition.SystemBackdrops;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACMS_Program_Planner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgramTimeline : Window
    {
        public ProgramTimeline()
        {
            InitializeComponent();
        }

        public ProgramTimeline(ProgramItem program)
        {
            InitializeComponent();

            int totalTime = 0;
            double[] x = new double[program.StepIds.Count + 1];
            int counter = 0;
            double[] y = new double[program.StepIds.Count + 1];

            foreach (var stepId in program.StepIds)
            {
                var step = DataService.Instance.Steps.FirstOrDefault(s => s.Id == stepId);
                if (step != null)
                {
                    if (!step.TerminateIfTimeExpired)
                    {
                        x[counter] = totalTime;
                        totalTime += 20;
                        y[counter] = step.FinalTemperature;
                        ProgramPlot.Plot.Add.Text("Indefinite", x[counter] + 12, y[counter] - 1);
                    }
                    else
                    {
                        x[counter] = totalTime;
                        totalTime += step.DurationSeconds / 60;
                        y[counter] = step.FinalTemperature;
                    }

                    var hs = ProgramPlot.Plot.Add.HorizontalSpan(x[counter], totalTime);
                    hs.LineStyle.Width = 1;
                    hs.LineStyle.Color = Colors.Gray;
                    hs.FillStyle.Color = step.IsSteamerActive ? Colors.Magenta.WithAlpha(.2) : Colors.DarkBlue.WithAlpha(.2);
                    //hs.LegendText = step.IsSteamerActive ? "Steaming Active" : "Steaming Inactive";
                    //hs.IsResizable = true;
                    ProgramPlot.Plot.Add.Text(step.Name, x[counter] + 1, 195);
                    /*var anno = ProgramPlot.Plot.Add.Annotation(step.Name);
                    anno.LabelFontSize = 26;
                    anno.LabelBackgroundColor = Colors.RebeccaPurple.WithAlpha(.3);
                    anno.LabelFontColor = Colors.RebeccaPurple;
                    anno.LabelBorderColor = Colors.Green;
                    anno.LabelBorderWidth = 1;
                    anno.LabelShadowColor = Colors.Transparent;
                    anno.OffsetY = hs.;
                    anno.OffsetX = (float)x[counter];
                    */
                    counter++;
                }
            }

            x[counter] = totalTime;
            y[counter] = y[counter - 1];

            TitleText1.Text = "Program Number: " + program.Id;
            TitleText2.Text = "Number of Steps: " + program.StepIds.Count;
            TitleText3.Text = "Program Name: " + program.Name;

            var sp2 = ProgramPlot.Plot.Add.Scatter(x, y);
            sp2.ConnectStyle = ConnectStyle.StepHorizontal;
            sp2.MarkerShape = MarkerShape.None;
            sp2.LineWidth = 3;
            sp2.Color = Colors.Red;

            ProgramPlot.Plot.XLabel("Duration (minutes)");
            ProgramPlot.Plot.YLabel("Target Temperature (°C)");
            ProgramPlot.Plot.Axes.SetLimits(0, totalTime, 0, 200);

            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark:
                {
                    ProgramPlot.Plot.FigureBackground.Color = Colors.Black;
                    ProgramPlot.Plot.DataBackground.Color = Colors.White;
                    ProgramPlot.Plot.Axes.Color(Colors.White);
                    break;
                }
                case ElementTheme.Light:
                case ElementTheme.Default:
                {
                    ProgramPlot.Plot.FigureBackground.Color = Colors.White;
                    ProgramPlot.Plot.DataBackground.Color = Colors.White;
                    ProgramPlot.Plot.Axes.Color(Colors.Black);
                    break;
                }
            }

            ProgramPlot.Refresh();
        }
    }
}
