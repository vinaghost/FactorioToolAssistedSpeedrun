using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
            DataContext = App.Current.Services.GetRequiredService<StepDetailPanelViewModel>();
        }
    }
}