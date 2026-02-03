using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<MenuBarViewModel>();
        }
    }
}