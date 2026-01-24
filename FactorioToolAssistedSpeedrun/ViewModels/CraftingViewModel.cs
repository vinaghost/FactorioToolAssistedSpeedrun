using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class CraftingViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        public PanelService PanelService => _panelService;

        public CraftingViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public CraftingViewModel(PanelService panelService)
        {
            _panelService = panelService;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await Task.Run(_panelService.LoadCraft);
        }
    }
}