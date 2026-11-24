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
            if (stepIds.Count == 0) return;
            if (context.ExecuteScalar<int>("SELECT COUNT(*) FROM Steps WHERE Name = $name", new SqliteParameter("$name", name)) == 0) return;

            var idClause = ProjectDbContext.IdInClause(stepIds);
            var count = context.ExecuteScalar<int>($"SELECT COUNT(*) FROM Steps WHERE Id IN ({idClause})");

            if (count == 0) return;
            if (moveOffset > 0)
            {
                // go down
                var lastStepLocation = context.ExecuteScalar<int>("SELECT MAX(Location) FROM Steps WHERE Name = $name", new SqliteParameter("$name", name));
                var lastLocation = context.ExecuteScalar<int>($"SELECT MAX(Location) FROM Steps WHERE Id IN ({idClause})");

                if (lastStepLocation == lastLocation)
                {
                    return;
                }

                if (lastLocation + moveOffset > lastStepLocation)
                {
                    moveOffset = lastStepLocation - lastLocation;
                }

                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location - $count WHERE Location > $lastLocation AND Location <= $maxLocation AND Name = $name",
                    new SqliteParameter("$count", count), new SqliteParameter("$lastLocation", lastLocation), new SqliteParameter("$maxLocation", lastLocation + moveOffset), new SqliteParameter("$name", name));
            }
            else
            {
                // go up
                var firstStepLocation = context.ExecuteScalar<int>("SELECT MIN(Location) FROM Steps WHERE Name = $name", new SqliteParameter("$name", name));
                var firstLocation = context.ExecuteScalar<int>($"SELECT MIN(Location) FROM Steps WHERE Id IN ({idClause})");

                if (firstStepLocation == firstLocation)
                {
                    return;
                }
                if (firstLocation + moveOffset < firstStepLocation)
                {
                    moveOffset = firstStepLocation - firstLocation;
                }
                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location + $count WHERE Location < $firstLocation AND Location >= $minLocation AND Name = $name",
                    new SqliteParameter("$count", count), new SqliteParameter("$firstLocation", firstLocation), new SqliteParameter("$minLocation", firstLocation + moveOffset), new SqliteParameter("$name", name));
            }

            context.ExecuteNonQuery(
                $"UPDATE Steps SET Location = Location + $moveOffset WHERE Id IN ({idClause})",
                new SqliteParameter("$moveOffset", moveOffset));
        }

        public static void UpdatePosition(this ProjectDbContext context, string name, double oldX, double oldY, double newX, double newY)
        {
            context.ExecuteNonQuery(
                "UPDATE Steps SET X = $newX, Y = $newY WHERE Name = $name AND ABS(X - $oldX) < 0.0001 AND ABS(Y - $oldY) < 0.0001",
                new SqliteParameter("$newX", newX), new SqliteParameter("$newY", newY), new SqliteParameter("$name", name), new SqliteParameter("$oldX", oldX), new SqliteParameter("$oldY", oldY));
        }
    }
}