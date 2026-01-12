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
        public IServiceProvider Services { get; }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<StartupService>();
            services.AddSingleton<StepService>();
            services.AddSingleton<LoadingService>();

            services.AddSingleton<CommandStack>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DialogViewModel>();

            services.AddTransient<MenuBarViewModel>();
            services.AddTransient<StepTypePanelViewModel>();
            services.AddTransient<StepDetailPanelViewModel>();
            services.AddTransient<StepPanelViewModel>();

            services.AddTransient<ImportTabViewModel>();
            services.AddTransient<TemplatesTabViewModel>();

            return services.BuildServiceProvider();
        }
    }
}