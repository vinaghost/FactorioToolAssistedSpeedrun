using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record ReplacePointCommandParameters(string Name, double OldX, double OldY, double NewX, double NewY) : CommandParameters(Name);

    public class ReplacePointCommand : Command<ReplacePointCommandParameters>
    {
        private readonly CommandStack _commandStack;

        public ReplacePointCommand(StartupService startupService, PanelService panelService, CommandStack commandStack)
            : base(startupService, panelService)
        {
            _commandStack = commandStack;
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, oldX, oldY, newX, newY) = Parameters;
            DatabaseCommit(context, oldX, oldY, newX, newY, name);
        }

        public static void DatabaseCommit(ProjectDbContext context, double oldX, double oldY, double newX, double newY, string name)
        {
            context.Steps
                .Where(x => x.Name == name)
                .Where(x => Math.Abs(x.X - oldX) < 0.0001 && Math.Abs(x.Y - oldY) < 0.0001)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => newX)
                    .SetProperty(b => b.Y, b => newY));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, oldX, oldY, newX, newY) = Parameters;

            _commandStack.Lock();
            UICommit(collection, oldX, oldY, newX, newY);
            _commandStack.Unlock();
        }

        public static void UICommit(ObservableCollection<StepModel> collection, double oldX, double oldY, double newX, double newY)
        {
            var items = collection
                .Where(x => x.X == $"{oldX:F2}" && x.Y == $"{oldY:F2}")
                .ToList();
            foreach (var item in items)
            {
                item.X = $"{newX:F2}";
                item.Y = $"{newY:F2}";
            }
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, oldX, oldY, newX, newY) = Parameters;
            DatabaseRollback(context, oldX, oldY, newX, newY, name);
        }

        public static void DatabaseRollback(ProjectDbContext context, double oldX, double oldY, double newX, double newY, string name)
        {
            context.Steps
                .Where(x => x.Name == name)
                .Where(x => Math.Abs(x.X - newX) < 0.0001 && Math.Abs(x.Y - newY) < 0.0001)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => oldX)
                    .SetProperty(b => b.Y, b => oldY));
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, oldX, oldY, newX, newY) = Parameters;

            _commandStack.Lock();
            UIRollback(collection, oldX, oldY, newX, newY);
            _commandStack.Unlock();
        }

        public static void UIRollback(ObservableCollection<StepModel> collection, double oldX, double oldY, double newX, double newY)
        {
            var items = collection
                .Where(x => x.X == $"{newX:F2}" && x.Y == $"{newY:F2}")
                .ToList();
            foreach (var item in items)
            {
                item.X = $"{oldX:F2}";
                item.Y = $"{oldY:F2}";
            }
        }
    }
}