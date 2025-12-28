using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepPanelViewModel : ObservableObject
    {
        public ObservableCollection<Step> StepCollection { get; set; } = [];

        public Action? StepsChangeStarted;

        public Action? StepsChangeCompleted;

        public void LoadSteps(IEnumerable<Step> steps)
        {
            StepsChangeStarted?.Invoke();
            StepCollection.Clear();
            foreach (var step in steps)
            {
                StepCollection.Add(step);
            }
            StepsChangeCompleted?.Invoke();
        }
    }
}