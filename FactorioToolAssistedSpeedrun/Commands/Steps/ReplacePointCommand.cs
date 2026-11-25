using System.Collections.ObjectModel;

using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

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

    public static class ReplacePointCommandExtension
    {
        public static void UpdatePosition(this ObservableCollection<StepModel> collection, double oldX, double oldY, double newX, double newY)
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

        public static void UpdatePosition(this ProjectDbContext context, string name, double oldX, double oldY, double newX, double newY)
        {
            context.ExecuteNonQuery(
                "UPDATE Steps SET X = $newX, Y = $newY WHERE Name = $name AND ABS(X - $oldX) < 0.0001 AND ABS(Y - $oldY) < 0.0001",
                new SqliteParameter("$newX", newX), new SqliteParameter("$newY", newY), new SqliteParameter("$name", name), new SqliteParameter("$oldX", oldX), new SqliteParameter("$oldY", oldY));
        }
    }
}