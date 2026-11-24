using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
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
            if (stepIds.Count == 0 || moveOffset == 0) return;

            var selectedIds = stepIds.ToHashSet();
            var selectedIndices = collection
                .Select((step, index) => (step, index))
                .Where(x => selectedIds.Contains(x.step.Id))
                .Select(x => x.index)
                .ToList();
            if (selectedIndices.Count == 0) return;

            var items = collection.ToList();
            var newOrder = StepOrdering.Reorder(items.Count, selectedIndices, moveOffset);
            var locations = items.Select(x => x.Location).OrderBy(x => x).ToList();
            for (int position = 0; position < newOrder.Length; position++)
            {
                var item = items[newOrder[position]];
                item.Location = locations[position];

                var currentIndex = collection.IndexOf(item);
                if (currentIndex != position)
                {
                    collection.Move(currentIndex, position);
                }
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