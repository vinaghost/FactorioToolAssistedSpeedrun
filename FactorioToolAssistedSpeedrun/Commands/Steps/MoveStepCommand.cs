using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class MoveStepCommand : UndoCommand
    {
        public required string Name { get; init; }
        public required List<Guid> StepIds { get; init; }

        public required int MoveOffset { get; init; }

        protected override void DatabaseCommit(ProjectDbContext context)
        {
            var chosenSteps = context.Steps
                .Where(x => StepIds.Contains(x.Id) && x.Name == Name)
                .OrderBy(x => x.Location)
                .ToList();
            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (MoveOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + MoveOffset && x.Name == Name)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + MoveOffset && x.Name == Name)
                     .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }

            context.Steps
                .Where(x => StepIds.Contains(x.Id) && x.Name == Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + MoveOffset));
        }

        protected override void UICommit(ObservableCollection<StepModel> collection)
        {
            var chosenSteps = collection
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            if (chosenSteps.Count == 0) return;
            var name = chosenSteps.First().Name;
            if (!string.IsNullOrEmpty(name) && name != Name)
                return;
            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (MoveOffset > 0)
            {
                // go down
                var sadSteps = collection
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location > lastLocation && x.step.Location <= lastLocation + MoveOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (index, step) in sadSteps)
                {
                    collection.RemoveAt(index);
                }

                foreach (var (_, step) in sadSteps)
                {
                    step.Location -= chosenSteps.Count;
                    collection.Insert(firstLocation - 1, step);
                }
            }
            else
            {
                // go up
                var sadSteps = collection
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + MoveOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (_, step) in sadSteps)
                {
                    step.Location += chosenSteps.Count;
                    collection.Insert(lastLocation, step);
                }

                foreach (var (index, step) in sadSteps)
                {
                    collection.RemoveAt(index);
                }
            }
            foreach (var step in chosenSteps)
            {
                step.Location += MoveOffset;
            }
        }

        protected override void DatabaseRollback(ProjectDbContext context)
        {
            var rollbackOffset = -MoveOffset;
            var chosenSteps = context.Steps
                .Where(x => StepIds.Contains(x.Id) && x.Name == Name)
                .OrderBy(x => x.Location)
                .ToList();

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;

            if (rollbackOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + rollbackOffset && x.Name == Name)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + rollbackOffset && x.Name == Name)
                     .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }
            context.Steps
                .Where(x => StepIds.Contains(x.Id) && x.Name == Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + rollbackOffset));
        }

        protected override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var rollbackOffset = -MoveOffset;
            var chosenSteps = collection
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();

            if (chosenSteps.Count == 0) return;
            var name = chosenSteps.First().Name;
            if (!string.IsNullOrEmpty(name) && name != Name)
                return;

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (rollbackOffset > 0)
            {
                // go down
                var sadSteps = collection
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location > lastLocation && x.step.Location <= lastLocation + rollbackOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (index, step) in sadSteps)
                {
                    collection.RemoveAt(index);
                }

                foreach (var (_, step) in sadSteps)
                {
                    step.Location -= chosenSteps.Count;
                    collection.Insert(firstLocation - 1, step);
                }
            }
            else
            {
                // go up
                var sadSteps = collection
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + rollbackOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (_, step) in sadSteps)
                {
                    step.Location += chosenSteps.Count;
                    collection.Insert(lastLocation, step);
                }

                foreach (var (index, step) in sadSteps)
                {
                    collection.RemoveAt(index);
                }
            }
            foreach (var step in chosenSteps)
            {
                step.Location += rollbackOffset;
            }
        }
    }
}