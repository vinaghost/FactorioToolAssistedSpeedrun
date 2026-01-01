using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepPanelViewModel : ObservableObject
    {
        public ObservableCollection<StepModel> StepCollection { get; set; } = [];

        public Action? StepsChangeStarted;

        public Action? StepsChangeCompleted;

        public void LoadSteps(List<Step> steps)
        {
            StepsChangeStarted?.Invoke();

            if (steps.Count < StepCollection.Count)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                while (StepCollection.Count > steps.Count)
                {
                    StepCollection.RemoveAt(StepCollection.Count - 1);
                }
            }
            else if (steps.Count > StepCollection.Count)
            {
                for (int i = 0; i < StepCollection.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                for (int i = StepCollection.Count; i < steps.Count; i++)
                {
                    StepModel model = new();
                    model.FromEntity(steps[i]);
                    StepCollection.Add(model);
                }
            }
            else
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
            }
            StepsChangeCompleted?.Invoke();
        }
    }
}