using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for TemplatesTab.xaml
    /// </summary>
    public partial class TemplatesTab : UserControl
    {
        public TemplatesTab()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<TemplatesTabViewModel>();
        }
    }
}