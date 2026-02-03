using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
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
            DataContext = Ioc.Default.GetRequiredService<ImportStringViewModel>();
            WindowPositionManager.Load(this, nameof(ImportStringWindow));
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowPositionManager.Save(this, nameof(ImportStringWindow));
            base.OnClosed(e);
        }
    }
}