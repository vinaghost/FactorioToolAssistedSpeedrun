using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for StepPanel.xaml
    /// </summary>
    public partial class StepPanel : UserControl
    {
        public StepPanel()
        {
            InitializeComponent();

            if (DataContext is not StepPanelViewModel vm)
                return;
            vm.StepsChangeStarted = Steps.BeginInit;
            vm.StepsChangeCompleted = Steps.EndInit;
        }
    }
}