using FactorioToolAssistedSpeedrun.DbContexts;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetTemplatesQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<string> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return [.. context.Steps
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)];
        }
    }
}