using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record MoveStepCommandParameters(string Name, List<Guid> StepIds, int MoveOffset) : CommandParameters(Name);

    public class MoveStepCommand : Command<MoveStepCommandParameters>
    {
        public MoveStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;
            context.MoveSteps(name, stepIds, moveOffset);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            collection.MoveSteps(stepIds, moveOffset);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, stepIds, moveOffset) = Parameters;
            context.MoveSteps(name, stepIds, -moveOffset);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds, moveOffset) = Parameters;
            collection.MoveSteps(stepIds, -moveOffset);
        }
    }

    public static class MoveStepCommandExtensions
    {
        public static void MoveSteps(this ProjectDbContext context, string name, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0 || moveOffset == 0) return;

            var idClause = ProjectDbContext.IdInClause(stepIds);

            context.RunInTransaction(() =>
            {
                int selectedCount = (int)context.ExecuteScalar<long>(
                    $"SELECT COUNT(*) FROM Steps WHERE Name = $name AND Id IN ({idClause})",
                    new SqliteParameter("$name", name));
                if (selectedCount == 0) return;

                int minLoc = (int)context.ExecuteScalar<long>(
                    $"SELECT MIN(Location) FROM Steps WHERE Name = $name AND Id IN ({idClause})",
                    new SqliteParameter("$name", name));

                int maxLoc = (int)context.ExecuteScalar<long>(
                    $"SELECT MAX(Location) FROM Steps WHERE Name = $name AND Id IN ({idClause})",
                    new SqliteParameter("$name", name));

                int totalCount = (int)context.ExecuteScalar<long>(
                    "SELECT COUNT(*) FROM Steps WHERE Name = $name",
                    new SqliteParameter("$name", name));

                if (moveOffset > 0)
                {
                    // Clamp moveOffset so we don't move past the end of the list
                    int actualOffset = Math.Min(moveOffset, totalCount - maxLoc);
                    if (actualOffset <= 0) return;

                    // Shift displaced items below the selection UP
                    context.ExecuteNonQuery(
                        @"UPDATE Steps
                  SET Location = Location - $selectedCount
                  WHERE Name = $name
                    AND Location > $maxLoc
                    AND Location <= $maxLoc + $actualOffset",
                        new SqliteParameter("$selectedCount", selectedCount),
                        new SqliteParameter("$name", name),
                        new SqliteParameter("$maxLoc", maxLoc),
                        new SqliteParameter("$actualOffset", actualOffset));

                    // Shift selected items DOWN
                    context.ExecuteNonQuery(
                        $"UPDATE Steps SET Location = Location + $actualOffset WHERE Name = $name AND Id IN ({idClause})",
                        new SqliteParameter("$actualOffset", actualOffset),
                        new SqliteParameter("$name", name));
                }
                else
                {
                    // Clamp moveOffset so we don't move above Location = 1
                    int actualOffset = Math.Min(-moveOffset, minLoc - 1);
                    if (actualOffset <= 0) return;

                    // Shift displaced items above the selection DOWN
                    context.ExecuteNonQuery(
                        @"UPDATE Steps
                  SET Location = Location + $selectedCount
                  WHERE Name = $name
                    AND Location >= $minLoc - $actualOffset
                    AND Location < $minLoc",
                        new SqliteParameter("$selectedCount", selectedCount),
                        new SqliteParameter("$name", name),
                        new SqliteParameter("$minLoc", minLoc),
                        new SqliteParameter("$actualOffset", actualOffset));

                    // Shift selected items UP
                    context.ExecuteNonQuery(
                        $"UPDATE Steps SET Location = Location - $actualOffset WHERE Name = $name AND Id IN ({idClause})",
                        new SqliteParameter("$actualOffset", actualOffset),
                        new SqliteParameter("$name", name));
                }
            });
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
    }
}