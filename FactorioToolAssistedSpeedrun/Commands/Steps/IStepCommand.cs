using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IStepCommand
    {
        void Commit();

        void Rollback(ObservableCollection<StepModel> steps);
    }
}