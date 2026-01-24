using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class CraftingViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly StartupService _startupService;
        public PanelService PanelService => _panelService;
        public StartupService StartupService => _startupService;

        public CraftingViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
        }

        [ActivatorUtilitiesConstructor]
        public CraftingViewModel(PanelService panelService, StartupService startupService)
        {
            _panelService = panelService;
            _startupService = startupService;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await Task.Run(_panelService.LoadCraft);
        }
    }
}