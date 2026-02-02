using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record AddStepCommandParameters(string Name, List<Step> Steps) : CommandParameters(Name);

    public class AddStepCommand : Command<AddStepCommandParameters>
    {
        public AddStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.AddSteps(name, steps);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.AddSteps(steps);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.DeleteSteps(name, steps);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.DeleteSteps(steps);
        }
    }
}