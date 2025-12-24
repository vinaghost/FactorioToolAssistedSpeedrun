using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepTypeViewModel : ObservableObject
    {
        [ObservableProperty]
        private StepType _selectedStepType = StepType.Walk;
    }
}