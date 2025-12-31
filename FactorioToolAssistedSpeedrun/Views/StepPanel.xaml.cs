using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows.Controls;

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