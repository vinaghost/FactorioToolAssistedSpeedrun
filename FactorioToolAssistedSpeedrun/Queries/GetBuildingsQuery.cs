using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetBuildingsQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<Building> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return [.. context.Steps
                .Where(x => !x.IsSkip && x.Type == StepType.Build)
                .Select(x => new Building(x.X, x.Y, x.Item, x.Location))];
        }
    }
}