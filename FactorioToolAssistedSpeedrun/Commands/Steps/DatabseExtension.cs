using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public static class DatabseExtension
    {
        public static void AddSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            var minLocation = steps.Min(x => x.Location);
            context.RunInTransaction(() =>
            {
                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location + $count WHERE Location >= $minLocation AND Name = $name",
                    new SqliteParameter("$count", steps.Count), new SqliteParameter("$minLocation", minLocation), new SqliteParameter("$name", name));
                context.InsertSteps(steps);
            });
        }

        public static void DeleteSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            var idClause = ProjectDbContext.IdInClause(steps.Select(s => s.Id));
            var maxLocation = steps.Max(x => x.Location);
            context.RunInTransaction(() =>
            {
                context.ExecuteNonQuery($"DELETE FROM Steps WHERE Name = $name AND Id IN ({idClause})", new SqliteParameter("$name", name));
                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location - $count WHERE Location > $maxLocation AND Name = $name",
                    new SqliteParameter("$count", steps.Count), new SqliteParameter("$maxLocation", maxLocation), new SqliteParameter("$name", name));
            });
        }

        public static void MoveSteps(this ProjectDbContext context, string name, List<Guid> stepIds, int moveOffset)
        {
            if (stepIds.Count == 0 || moveOffset == 0) return;

            var steps = context.GetSteps(name);
            var selectedIds = stepIds.ToHashSet();
            var selectedIndices = steps
                .Select((step, index) => (step, index))
                .Where(x => selectedIds.Contains(x.step.Id))
                .Select(x => x.index)
                .ToList();
            if (selectedIndices.Count == 0) return;

            var newOrder = StepOrdering.Reorder(steps.Count, selectedIndices, moveOffset);
            var locations = steps.Select(x => x.Location).ToList();
            context.RunInTransaction(() =>
            {
                for (int position = 0; position < newOrder.Length; position++)
                {
                    var step = steps[newOrder[position]];
                    if (step.Location == locations[position]) continue;

                    context.ExecuteNonQuery(
                        "UPDATE Steps SET Location = $location WHERE Id = $id",
                        new SqliteParameter("$location", locations[position]), new SqliteParameter("$id", step.Id.ToString()));
                }
            });
        }

        public static void UpdatePosition(this ProjectDbContext context, string name, double oldX, double oldY, double newX, double newY)
        {
            context.ExecuteNonQuery(
                "UPDATE Steps SET X = $newX, Y = $newY WHERE Name = $name AND ABS(X - $oldX) < 0.0001 AND ABS(Y - $oldY) < 0.0001",
                new SqliteParameter("$newX", newX), new SqliteParameter("$newY", newY), new SqliteParameter("$name", name), new SqliteParameter("$oldX", oldX), new SqliteParameter("$oldY", oldY));
        }
    }
}