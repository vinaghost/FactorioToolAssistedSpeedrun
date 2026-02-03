using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for StepTypePanel.xaml
    /// </summary>
    public partial class StepTypePanel : UserControl
    {
        public StepTypePanel()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<StepTypePanelViewModel>();
        }
    }
}