using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for StepDetailPanel.xaml
    /// </summary>
    public partial class StepDetailPanel : UserControl
    {
        public StepDetailPanel()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<StepDetailPanelViewModel>();
        }
    }
}