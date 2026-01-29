using FactorioToolAssistedSpeedrun.Commands.Steps;
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
            services.AddSingleton<StepService>();
            services.AddSingleton<PanelService>();
            services.AddSingleton<LoadingService>();

            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<ICommandStack, CommandStack>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<GoToLineViewModel>();
            services.AddTransient<ImportStringViewModel>();
            services.AddTransient<ReplaceViewModel>();
            services.AddTransient<CraftingViewModel>();

            services.AddTransient<MenuBarViewModel>();
            services.AddTransient<StepTypePanelViewModel>();
            services.AddTransient<StepDetailPanelViewModel>();
            services.AddTransient<StepPanelViewModel>();
            services.AddTransient<TemplatePanelViewModel>();

            services.AddTransient<AddStepCommand>();
            services.AddTransient<ApplySkipCommand>();
            services.AddTransient<DeleteStepCommand>();
            services.AddTransient<MoveStepCommand>();
            services.AddTransient<ReplacePointCommand>();
            services.AddTransient(typeof(UpdateStepPropertyCommand<,>));

            return services.BuildServiceProvider();
        }
    }
}