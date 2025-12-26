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
            var vm = App.Current.Services.GetService<MainViewModel>()!;

            DataContext = vm;
            vm.StepsChangeStarted += () => Steps.BeginInit();
            vm.StepsChangeCompleted += () => Steps.EndInit();
        }
    }
}