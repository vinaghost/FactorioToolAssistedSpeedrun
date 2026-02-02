using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public static class UIExtension
    {
        public static void AddSteps(this ObservableCollection<StepModel> collection, List<Step> steps)
        {
            if (collection.Count == 0)
            {
                foreach (var step in steps.OrderBy(x => x.Location))
                {
                    var model = new StepModel();
                    model.FromEntity(step);
                    collection.Add(model);
                }
            }
            else
            {
                var minLocation = steps.Min(x => x.Location);
                foreach (var step in collection.Where(x => x.Location >= minLocation))
                {
                    step.Location += steps.Count;
                }
                foreach (var step in steps.OrderByDescending(x => x.Location))
                {
                    var model = new StepModel();
                    model.FromEntity(step);
                    collection.Insert(minLocation - 1, model);
                }
            }
        }

        public static void DeleteSteps(this ObservableCollection<StepModel> collection, List<Step> steps)
        {
            var ids = steps.Select(x => x.Id).ToList();
            var locations = collection.Select((x, index) => (x, index)).Where(x => ids.Contains(x.x.Id)).Select(x => x.index).OrderByDescending(x => x).ToList();
            foreach (var location in locations)
            {
                collection.RemoveAt(location);
            }

            var maxLocation = locations.Max();
            foreach (var step in collection.Where(x => x.Location > maxLocation))
            {
                step.Location -= locations.Count;
            }
        }

        public static void MoveSteps(this ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0) return;
            if (moveOffset > 0)
            {
                collection.GoDown(stepIds, moveOffset);
            }
            else
            {
                collection.GoUp(stepIds, moveOffset);
            }
        }

        private static void GoDown(this ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
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

        private static void GoUp(this ObservableCollection<StepModel> collection, List<Guid> stepIds, int moveOffset)
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

        public static void UpdatePosition(this ObservableCollection<StepModel> collection, double oldX, double oldY, double newX, double newY)
        {
            var item = collection
                .Where(x => x.X == $"{oldX:F2}" && x.Y == $"{oldY:F2}")
                .FirstOrDefault();
            if (item is null) return;

            item.X = $"{newX:F2}";
            item.Y = $"{newY:F2}";
        }
    }
}