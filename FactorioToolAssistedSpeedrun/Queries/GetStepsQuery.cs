using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetStepsQuery
    {
        public required string ProjectDataFile { get; init; }
        public required string Name { get; init; }

        public List<Step> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return context.GetSteps(Name);
        }
    }
}