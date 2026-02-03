using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for GoToLineWindow.xaml
    /// </summary>
    public partial class GoToLineWindow : Window
    {
        public GoToLineWindow()
        {
            InitializeComponent();
            var vm = Ioc.Default.GetRequiredService<GoToLineViewModel>();
            vm.Close += Close;
            DataContext = vm;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            LineTxtBox.SelectAll();
            LineTxtBox.Focus();
        }
    }
}