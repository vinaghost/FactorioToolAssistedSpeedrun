using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepTypePanelViewModel : ObservableObject
    {
        private readonly StepService _stepService;

        public StepService StepService => _stepService;

        public StepTypePanelViewModel()
        {
            _stepService = Ioc.Default.GetRequiredService<StepService>();
        }

        [ActivatorUtilitiesConstructor]
        public StepTypePanelViewModel(StepService stepService)
        {
            _stepService = stepService;
        }
    }
}