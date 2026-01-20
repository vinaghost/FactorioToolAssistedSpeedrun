using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IUICommand
    {
        void UICommit(ObservableCollection<StepModel> collection);

        void UIRollback(ObservableCollection<StepModel> collection);
    }
}