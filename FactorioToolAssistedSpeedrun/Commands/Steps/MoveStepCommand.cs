using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record MoveStepCommandParameters(string Name, List<Guid> StepIds, int MoveOffset) : CommandParameters(Name);

    public class MoveStepCommand : Command<MoveStepCommandParameters>
    {
        public MoveStepCommand(StartupService startupService, PanelService panelService)
            : base(startupService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;
            var chosenSteps = context.Steps
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (moveOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + moveOffset && x.Name == name)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + moveOffset && x.Name == name)
                     .ExecuteUpdate(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }

            context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + moveOffset));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            var chosenSteps = collection
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            if (chosenSteps.Count == 0) return;

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;
            if (moveOffset > 0)
            {
                // go down
                var sadSteps = collection
                    .Select((step, index) => (index, step))
                    .Where(x => x.step.Location > lastLocation && x.step.Location <= lastLocation + moveOffset)
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
                    .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + moveOffset)
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
                step.Location += moveOffset;
            }
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;

            var rollbackOffset = -moveOffset;
            var chosenSteps = context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .OrderBy(x => x.Location)
                .ToList();

            var firstLocation = chosenSteps.First().Location;
            var lastLocation = chosenSteps.Last().Location;

            if (rollbackOffset > 0)
            {
                // go down
                context.Steps
                    .Where(x => x.Location > lastLocation && x.Location <= lastLocation + rollbackOffset && x.Name == name)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(b => b.Location, b => b.Location - chosenSteps.Count));
            }
            else
            {
                // go up
                context.Steps
                    .Where(x => x.Location < firstLocation && x.Location >= firstLocation + rollbackOffset && x.Name == name)
                     .ExecuteUpdate(setters => setters
                        .SetProperty(b => b.Location, b => b.Location + chosenSteps.Count));
            }
            context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + rollbackOffset));
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (name, stepIds, moveOffset) = Parameters;

            var rollbackOffset = -moveOffset;
            var chosenSteps = collection
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();

            if (chosenSteps.Count == 0) return;

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