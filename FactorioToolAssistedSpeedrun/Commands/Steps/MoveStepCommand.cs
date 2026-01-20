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
            DatabaseCommit(context, stepIds, moveOffset, name);
        }

        public static void DatabaseCommit(ProjectDbContext context, List<Guid> stepIds, int moveOffset, string name)
        {
            var chosenSteps = context.Steps
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            if (moveOffset > 0)
            {
                var lastLocation = chosenSteps.Last().Location;
                GoDown(context, name, lastLocation, moveOffset, chosenSteps.Count);
            }
            else
            {
                var firstLocation = chosenSteps.First().Location;
                GoUp(context, name, firstLocation, moveOffset, chosenSteps.Count);
            }
            context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + moveOffset));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            UICommit(collection, stepIds, moveOffset);
        }

        public static void UICommit(ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            var chosenSteps = collection
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            if (chosenSteps.Count == 0) return;
            if (moveOffset > 0)
            {
                var lastLocation = chosenSteps.Last().Location;
                GoDown(collection, lastLocation, moveOffset, chosenSteps.Count);
            }
            else
            {
                var firstLocation = chosenSteps.First().Location;
                GoUp(collection, firstLocation, moveOffset, chosenSteps.Count);
            }
            foreach (var step in chosenSteps)
            {
                step.Location += moveOffset;
            }
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;

            DatabaseRollback(context, name, stepIds, moveOffset);
        }

        public static void DatabaseRollback(ProjectDbContext context, string name, List<Guid> stepIds, int moveOffset)
        {
            var rollbackOffset = -moveOffset;
            var chosenSteps = context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .OrderBy(x => x.Location)
                .ToList();
            if (rollbackOffset > 0)
            {
                var lastLocation = chosenSteps.Last().Location;
                GoDown(context, name, lastLocation, rollbackOffset, chosenSteps.Count);
            }
            else
            {
                var firstLocation = chosenSteps.First().Location;
                GoUp(context, name, firstLocation, rollbackOffset, chosenSteps.Count);
            }
            context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + rollbackOffset));
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (name, stepIds, moveOffset) = Parameters;
            UIRollback(collection, name, stepIds, moveOffset);
        }

        public static void UIRollback(ObservableCollection<StepModel> collection, string name, List<Guid> stepIds, int moveOffset)
        {
            var rollbackOffset = -moveOffset;
            var chosenSteps = collection
                .Where(x => stepIds.Contains(x.Id))
                .OrderBy(x => x.Location)
                .ToList();
            if (chosenSteps.Count == 0) return;
            if (rollbackOffset > 0)
            {
                var lastLocation = chosenSteps.Last().Location;
                GoDown(collection, lastLocation, rollbackOffset, chosenSteps.Count);
            }
            else
            {
                var firstLocation = chosenSteps.First().Location;
                GoUp(collection, firstLocation, rollbackOffset, chosenSteps.Count);
            }
            foreach (var step in chosenSteps)
            {
                step.Location += rollbackOffset;
            }
        }

        private static void GoDown(ProjectDbContext context, string name, int lastLocation, int moveOffset, int stepCount)
        {
            context.Steps
                   .Where(x => x.Location > lastLocation && x.Location <= lastLocation + moveOffset && x.Name == name)
                   .ExecuteUpdate(setters => setters
                       .SetProperty(b => b.Location, b => b.Location - stepCount));
        }

        private static void GoUp(ProjectDbContext context, string name, int firstLocation, int moveOffset, int stepCount)
        {
            context.Steps
                .Where(x => x.Location < firstLocation && x.Location >= firstLocation + moveOffset && x.Name == name)
                 .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + stepCount));
        }

        private static void GoDown(ObservableCollection<StepModel> collection, int lastLocation, int moveOffset, int stepCount)
        {
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
                step.Location -= stepCount;
                collection.Insert(lastLocation - stepCount, step);
            }
        }

        private static void GoUp(ObservableCollection<StepModel> collection, int firstLocation, int moveOffset, int stepCount)
        {
            var sadSteps = collection
                .Select((step, index) => (index, step))
                .Where(x => x.step.Location < firstLocation && x.step.Location >= firstLocation + moveOffset)
                .OrderByDescending(x => x.index)
                .ToList();
            foreach (var (_, step) in sadSteps)
            {
                step.Location += stepCount;
                collection.Insert(firstLocation, step);
            }
            foreach (var (index, step) in sadSteps)
            {
                collection.RemoveAt(index);
            }
        }
    }
}