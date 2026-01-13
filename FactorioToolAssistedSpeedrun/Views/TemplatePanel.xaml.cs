using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for TemplatePanel.xaml
    /// </summary>
    public partial class TemplatePanel : UserControl
    {
        public TemplatePanel()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<TemplatePanelViewModel>();
        }
    }
}