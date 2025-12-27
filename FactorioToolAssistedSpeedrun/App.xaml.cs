using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            InitializeComponent();
        }

        public new static App Current => (App)Application.Current;
        public GameData? GameData { get; set; }
        public string? ProjectDataFile { get; set; }
        public IServiceProvider Services { get; }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoadingViewModel>();
            services.AddSingleton<StepTypeViewModel>();
            services.AddSingleton<MenuBarViewModel>();

            return services.BuildServiceProvider();
        }
    }
}