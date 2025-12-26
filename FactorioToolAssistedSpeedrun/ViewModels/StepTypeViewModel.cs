using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepTypeViewModel : ObservableObject
    {
        [ObservableProperty]
        private StepType _selectedStepType = StepType.Walk;
    }
}