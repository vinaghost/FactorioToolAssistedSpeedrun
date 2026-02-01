using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for CraftingWindow.xaml
    /// </summary>
    public partial class CraftingWindow : Window
    {
        public CraftingWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<CraftingViewModel>();
            WindowPositionManager.Load(this, nameof(CraftingWindow));
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowPositionManager.Save(this, nameof(CraftingWindow));
            base.OnClosed(e);
        }
    }
}