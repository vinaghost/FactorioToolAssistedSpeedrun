using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record ReplacePointCommandParameters(string Name, double OldX, double OldY, double NewX, double NewY) : CommandParameters(Name);

    public class ReplacePointCommand : Command<ReplacePointCommandParameters>
    {
        private readonly ICommandStack _commandStack;

        public ReplacePointCommand(IDataService dataService, PanelService panelService, ICommandStack commandStack)
            : base(dataService, panelService)
        {
            _commandStack = commandStack;
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, oldX, oldY, newX, newY) = Parameters;
            context.UpdatePosition(name, oldX, oldY, newX, newY);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, oldX, oldY, newX, newY) = Parameters;

            _commandStack.Lock();
            collection.UpdatePosition(oldX, oldY, newX, newY);
            _commandStack.Unlock();
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, oldX, oldY, newX, newY) = Parameters;
            context.UpdatePosition(name, newX, newY, oldX, oldY);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, oldX, oldY, newX, newY) = Parameters;

            _commandStack.Lock();
            collection.UpdatePosition(newX, newY, oldX, oldY);
            _commandStack.Unlock();
        }
    }
}