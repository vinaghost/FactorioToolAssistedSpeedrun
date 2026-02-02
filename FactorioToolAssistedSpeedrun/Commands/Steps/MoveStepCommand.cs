using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record MoveStepCommandParameters(string Name, List<Guid> StepIds, int MoveOffset) : CommandParameters(Name);

    public class MoveStepCommand : Command<MoveStepCommandParameters>
    {
        public MoveStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;
            context.MoveSteps(name, stepIds, moveOffset);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            collection.MoveSteps(stepIds, moveOffset);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;
            context.MoveSteps(name, stepIds, -moveOffset);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            collection.MoveSteps(stepIds, -moveOffset);
        }
    }
}