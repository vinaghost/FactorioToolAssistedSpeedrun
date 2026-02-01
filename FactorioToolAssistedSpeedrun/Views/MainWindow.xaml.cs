using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>();
            WindowPositionManager.Load(this, nameof(MainWindow));
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowPositionManager.Save(this, nameof(MainWindow));
            base.OnClosed(e);
        }
    }
}