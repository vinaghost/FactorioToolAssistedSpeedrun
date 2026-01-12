using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetStepsQuery
    {
        public required string ProjectDataFile { get; init; }
        public required string Name { get; init; }

        public List<Step> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return [.. context.Steps
                .AsNoTracking()
                .Where(x => x.Name == Name)
                .OrderBy(x => x.Location)];
        }
    }
}