using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class CountPointQuery
    {
        public required string ProjectDataFile { get; init; }
        public required double X { get; init; }
        public required double Y { get; init; }

        public int Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return context.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Steps WHERE Name = '' AND ABS(X - $x) < 0.0001 AND ABS(Y - $y) < 0.0001",
                new SqliteParameter("$x", X), new SqliteParameter("$y", Y));
        }
    }
}