using Microsoft.EntityFrameworkCore;

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
            return context.Steps
                .AsNoTracking()
                .Where(x => x.Name == "")
                .Where(x => Math.Abs(x.X - X) < 0.0001 && Math.Abs(x.Y - Y) < 0.0001)
                .Count();
        }
    }
}