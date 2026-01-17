using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class ReplacePointCommand : UndoCommand
    {
        public required double OldX { get; init; }
        public required double OldY { get; init; }
        public required double NewX { get; init; }
        public required double NewY { get; init; }

        protected override void DatabaseCommit(ProjectDbContext context)
        {
            context.Steps
                .Where(x => x.Name == Name)
                .Where(x => Math.Abs(x.X - OldX) < 0.0001 && Math.Abs(x.Y - OldY) < 0.0001)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => NewX)
                    .SetProperty(b => b.Y, b => NewY));

            context.Buildings
                .Where(x => Math.Abs(x.X - OldX) < 0.0001 && Math.Abs(x.Y - OldY) < 0.0001)
                 .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => NewX)
                    .SetProperty(b => b.Y, b => NewY));
        }

        protected override void UICommit(ObservableCollection<StepModel> collection)
        {
            var commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            var items = collection
                .Where(x => x.X == $"{OldX:F2}" && x.Y == $"{OldY:F2}")
                .ToList();
            commandStack.Lock();
            foreach (var item in items)
            {
                item.X = $"{NewX:F2}";
                item.Y = $"{NewY:F2}";
            }
            commandStack.Unlock();
        }

        protected override void DatabaseRollback(ProjectDbContext context)
        {
            context.Steps
                .Where(x => x.Name == Name)
                .Where(x => Math.Abs(x.X - NewX) < 0.0001 && Math.Abs(x.Y - NewY) < 0.0001)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => OldX)
                    .SetProperty(b => b.Y, b => OldY));

            context.Buildings
                .Where(x => Math.Abs(x.X - NewX) < 0.0001 && Math.Abs(x.Y - NewY) < 0.0001)
                 .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.X, b => OldX)
                    .SetProperty(b => b.Y, b => OldY));
        }

        protected override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            var items = collection
                .Where(x => x.X == $"{NewX:F2}" && x.Y == $"{NewY:F2}")
                .ToList();
            commandStack.Lock();
            foreach (var item in items)
            {
                item.X = $"{OldX:F2}";
                item.Y = $"{OldY:F2}";
            }
            commandStack.Unlock();
        }
    }
}