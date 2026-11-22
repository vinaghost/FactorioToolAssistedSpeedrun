using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetCraftingStepQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<Step> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return context.QuerySteps("WHERE (Name IS NULL OR Name = '') AND Type = $type ORDER BY Location",
                new SqliteParameter("$type", StepTypeExtensions.ToString(StepType.Craft)));
        }
    }
}