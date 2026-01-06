using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Services;
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
            services.AddSingleton<DialogViewModel>();

            services.AddSingleton<LoadingViewModel>();
            services.AddSingleton<MenuBarViewModel>();
            services.AddSingleton<StepTypePanelViewModel>();
            services.AddSingleton<StepDetailPanelViewModel>();
            services.AddSingleton<StepPanelViewModel>();

            services.AddSingleton<ImportTabViewModel>();
            services.AddSingleton<TemplatesTabViewModel>();

            services.AddSingleton<CommandStack>();

            return services.BuildServiceProvider();
        }
    }
}