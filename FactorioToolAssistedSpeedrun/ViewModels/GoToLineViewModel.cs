using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class GoToLineViewModel : ObservableObject
    {
        private readonly PanelService _panelService;

        public GoToLineViewModel()
        {
            _panelService = Ioc.Default.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public GoToLineViewModel(PanelService panelService)
        {
            _panelService = panelService;
            MaxLine = _panelService.StepCollection.Count;
        }

        public event Action? Close;

        [ObservableProperty]
        private int _line = 1;

        partial void OnLineChanged(int value)
        {
            if (value < 1)
            {
                Line = 1;
            }
            else if (value > MaxLine)
            {
                Line = MaxLine;
            }
        }

        [ObservableProperty]
        private int _maxLine;

        [RelayCommand]
        private async Task OK()
        {
            _panelService.ScrollTo(Line);
            Close?.Invoke();
        }

        [RelayCommand]
        private async Task Cancel()
        {
            Close?.Invoke();
        }
    }
}