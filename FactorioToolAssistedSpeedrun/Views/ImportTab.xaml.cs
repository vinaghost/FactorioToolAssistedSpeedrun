using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for ImportTab.xaml
    /// </summary>
    public partial class ImportTab : UserControl
    {
        public ImportTab()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<ImportTabViewModel>();
        }
    }
}