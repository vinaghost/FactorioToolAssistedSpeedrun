using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record MoveStepCommandParameters(string Name, List<Guid> StepIds, int MoveOffset) : CommandParameters(Name);

    public class MoveStepCommand : Command<MoveStepCommandParameters>
    {
        public MoveStepCommand(IStartupService startupService, PanelService panelService)
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
            if (stepIds.Count == 0) return;
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
                .Where(x => stepIds.Contains(x.Id))
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.Location, b => b.Location + moveOffset));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            UICommit(collection, stepIds, moveOffset);
        }

        public void UICommit(ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0) return;
            if (moveOffset > 0)
            {
                GoDown(collection, stepIds, moveOffset);
            }
            else
            {
                GoUp(collection, stepIds, moveOffset);
            }
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;

            DatabaseRollback(context, name, stepIds, moveOffset);
        }

        public static void DatabaseRollback(ProjectDbContext context, string name, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0) return;

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
            var (_, stepIds, moveOffset) = Parameters;
            UIRollback(collection, stepIds, moveOffset);
        }

        public void UIRollback(ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0) return;
            var rollbackOffset = -moveOffset;
            if (rollbackOffset > 0)
            {
                GoDown(collection, stepIds, moveOffset);
            }
            else
            {
                GoUp(collection, stepIds, moveOffset);
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

        public static void GoDown(ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0 || moveOffset <= 0) return;

            // Find the indices of the selected steps, sorted ascending
            var selectedIndices = collection
                .Select((step, index) => (index, step))
                .Where(x => stepIds.Contains(x.step.Id))
                .OrderBy(x => x.index)
                .Select(x => x.index)
                .ToList();

            if (selectedIndices.Count == 0) return;

            int firstSelectedIndex = selectedIndices.First();
            int lastSelectedIndex = selectedIndices.Last();

            // Find the range of items below the selected steps to move up
            int startMoveIndex = lastSelectedIndex + 1;
            int endMoveIndex = Math.Min(startMoveIndex + moveOffset - 1, collection.Count - 1);

            if (startMoveIndex > endMoveIndex) return; // Nothing to move

            // Extract the items to move
            var itemsToMove = new List<StepModel>();
            for (int i = startMoveIndex; i <= endMoveIndex; i++)
            {
                itemsToMove.Add(collection[startMoveIndex]); // Always remove at startMoveIndex as collection shrinks
                collection.RemoveAt(startMoveIndex);
            }

            // Insert the moved items above the first selected step
            int insertIndex = firstSelectedIndex;
            foreach (var item in itemsToMove)
            {
                collection.Insert(insertIndex++, item);
            }

            // Update Location property for moved items
            foreach (var item in itemsToMove)
            {
                item.Location -= (selectedIndices.Count);
            }
            // Update Location property for selected steps
            foreach (var idx in selectedIndices)
            {
                collection[idx + itemsToMove.Count].Location += itemsToMove.Count;
            }
        }

        public static void GoUp(ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0 || moveOffset >= 0) return;

            // Find the indices of the selected steps, sorted ascending
            var selectedIndices = collection
                .Select((step, index) => (index, step))
                .Where(x => stepIds.Contains(x.step.Id))
                .OrderBy(x => x.index)
                .Select(x => x.index)
                .ToList();

            if (selectedIndices.Count == 0) return;

            int firstSelectedIndex = selectedIndices.First();
            int lastSelectedIndex = selectedIndices.Last();

            // Find the range of items above the selected steps to move down
            int blockSize = Math.Min(-moveOffset, firstSelectedIndex);
            if (blockSize == 0) return;

            int startMoveIndex = firstSelectedIndex - blockSize;
            int endMoveIndex = firstSelectedIndex - 1;

            // Extract the items to move
            var itemsToMove = new List<StepModel>();
            for (int i = 0; i < blockSize; i++)
            {
                itemsToMove.Add(collection[startMoveIndex]); // Always remove at startMoveIndex as collection shrinks
                collection.RemoveAt(startMoveIndex);
            }

            // Insert the moved items below the last selected step
            int insertIndex = lastSelectedIndex - blockSize + 1;
            foreach (var item in itemsToMove)
            {
                collection.Insert(insertIndex++, item);
            }

            // Update Location property for moved items
            foreach (var item in itemsToMove)
            {
                item.Location += selectedIndices.Count;
            }
            // Update Location property for selected steps
            foreach (var idx in selectedIndices)
            {
                collection[idx - itemsToMove.Count].Location -= itemsToMove.Count;
            }
        }
    }
}