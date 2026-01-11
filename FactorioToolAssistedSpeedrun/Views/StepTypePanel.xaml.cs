using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
            DataContext = App.Current.Services.GetRequiredService<StepTypePanelViewModel>();
        }
    }
}