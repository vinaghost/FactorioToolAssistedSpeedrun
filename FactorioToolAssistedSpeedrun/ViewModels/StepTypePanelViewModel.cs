using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepTypePanelViewModel : ObservableObject
    {
        private readonly StepDetailPanelViewModel _stepDetailPanelViewModel;

        [ObservableProperty]
        private StepType _selectedStepType;

        public StepTypePanelViewModel()
        {
            _stepDetailPanelViewModel = new();
            SelectedStepType = StepType.Walk;
        }

        [ActivatorUtilitiesConstructor]
        public StepTypePanelViewModel(StepDetailPanelViewModel stepDetailPanelViewModel)
        {
            _stepDetailPanelViewModel = stepDetailPanelViewModel;
            SelectedStepType = StepType.Walk;
        }

        partial void OnSelectedStepTypeChanged(StepType value)
        {
            _stepDetailPanelViewModel.Load(value);
        }
    }
}