using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
            var vm = App.Current.Services.GetRequiredService<GoToLineViewModel>();
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