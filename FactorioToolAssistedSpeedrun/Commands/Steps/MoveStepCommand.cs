using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class MoveStepCommand : IStepCommand
    {
        public required List<Guid> StepIds { get; init; } //selected blocks

        public required int MoveOffset { get; init; }

        public void Commit()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            var chosenSteps = context.Steps
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (MoveOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + MoveOffset)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + MoveOffset)
                     .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }

            context.Steps
                .Where(x => StepIds.Contains(x.Id))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + MoveOffset));
        }

        public void Commit(ObservableCollection<StepModel> steps)
        {
            Commit();

            var chosenSteps = steps
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (MoveOffset > 0)
            {
                // go down
                var sadSteps = steps
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location > lastLocation && x.step.Location <= lastLocation + MoveOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (index, step) in sadSteps)
                {
                    steps.RemoveAt(index);
                }

                foreach (var (_, step) in sadSteps)
                {
                    step.Location -= chosenSteps.Count;
                    steps.Insert(firstLocation - 1, step);
                }
            }
            else
            {
                // go up
                var sadSteps = steps
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + MoveOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (_, step) in sadSteps)
                {
                    step.Location += chosenSteps.Count;
                    steps.Insert(lastLocation, step);
                }

                foreach (var (index, step) in sadSteps)
                {
                    steps.RemoveAt(index);
                }
            }
            foreach (var step in chosenSteps)
            {
                step.Location += MoveOffset;
            }
        }

        public void Rollback()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);

            var rollbackOffset = -MoveOffset;
            var chosenSteps = context.Steps
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;

            if (rollbackOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + rollbackOffset)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + rollbackOffset)
                     .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }
            context.Steps
                .Where(x => StepIds.Contains(x.Id))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + rollbackOffset));
        }

        public void Rollback(ObservableCollection<StepModel> steps)
        {
            Rollback();

            var rollbackOffset = -MoveOffset;
            var chosenSteps = steps
                .Where(x => StepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (rollbackOffset > 0)
            {
                // go down
                var sadSteps = steps
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location > lastLocation && x.step.Location <= lastLocation + rollbackOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (index, step) in sadSteps)
                {
                    steps.RemoveAt(index);
                }

                foreach (var (_, step) in sadSteps)
                {
                    step.Location -= chosenSteps.Count;
                    steps.Insert(firstLocation - 1, step);
                }
            }
            else
            {
                // go up
                var sadSteps = steps
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + rollbackOffset)
                    .OrderByDescending(x => x.index)
                    .ToList();

                foreach (var (_, step) in sadSteps)
                {
                    step.Location += chosenSteps.Count;
                    steps.Insert(lastLocation, step);
                }

                foreach (var (index, step) in sadSteps)
                {
                    steps.RemoveAt(index);
                }
            }
            foreach (var step in chosenSteps)
            {
                step.Location += rollbackOffset;
            }
        }
    }
}