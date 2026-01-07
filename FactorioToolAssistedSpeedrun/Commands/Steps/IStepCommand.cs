using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IStepCommand
    {
        void Commit();

        void Commit(ObservableCollection<StepModel> steps);

        void Rollback();

        void Rollback(ObservableCollection<StepModel> steps);
    }
}