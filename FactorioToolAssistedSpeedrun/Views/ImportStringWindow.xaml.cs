using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for ImportStringWindow.xaml
    /// </summary>
    public partial class ImportStringWindow : Window
    {
        public ImportStringWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<ImportStringViewModel>();
        }
    }
}