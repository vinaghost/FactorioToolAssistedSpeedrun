using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetBuildingsQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<Building> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return context.Query(
                "SELECT X, Y, Item, Location FROM Steps WHERE IsSkip = 0 AND Type = $type",
                reader => new Building(reader.GetDouble(0), reader.GetDouble(1), reader.GetString(2), reader.GetInt32(3)),
                new SqliteParameter("$type", StepTypeExtensions.ToString(StepType.Build)));
        }
    }
}