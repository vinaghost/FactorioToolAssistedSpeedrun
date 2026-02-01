using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        public ReplaceWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<ReplaceViewModel>();
            WindowPositionManager.Load(this, nameof(ReplaceWindow));
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowPositionManager.Save(this, nameof(ReplaceWindow));
            base.OnClosed(e);
        }
    }
}