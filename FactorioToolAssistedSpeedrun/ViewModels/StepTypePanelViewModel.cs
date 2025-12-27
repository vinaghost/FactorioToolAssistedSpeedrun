using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepTypePanelViewModel : ObservableObject
    {
        [ObservableProperty]
        private StepType _selectedStepType = StepType.Walk;
    }
}