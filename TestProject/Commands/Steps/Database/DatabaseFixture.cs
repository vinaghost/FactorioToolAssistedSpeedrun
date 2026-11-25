using FactorioToolAssistedSpeedrun;
using FactorioToolAssistedSpeedrun.Entities;

namespace TestProject.Commands.Steps.Database
{
    public sealed class DatabaseFixture : IDisposable
    {
        private readonly ProjectDbContext _context;

        public DatabaseFixture()
        {
            _context = GetInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.EnsureDeleted();
            _context.Dispose();
        }

        private static ProjectDbContext GetInMemoryDbContext()
        {
            var context = new ProjectDbContext($"{Guid.NewGuid()}");
            context.EnsureDeleted();
            context.EnsureCreated();
            context.SetupTriggers();
            return context;
        }

        public (ProjectDbContext, List<(Guid Id, int Location)>) SeedSteps(string name, int count, Func<int, (double X, double Y)>? position = null)
        {
            var steps = GenerateSteps(count);
            AddSteps(name, steps, position ?? RandomPosition);
            return (_context, steps);
        }

        private static (double X, double Y) RandomPosition(int location)
        {
            return (Random.Shared.Next(-100, 100), Random.Shared.Next(-100, 100));
        }

        private void AddSteps(string name, List<(Guid Id, int Location)> steps, Func<int, (double X, double Y)> position)
        {
            _context.ExecuteNonQuery("DELETE FROM Steps");
            _context.InsertSteps(steps.Select(step =>
            {
                var (x, y) = position(step.Location);
                return new Step
                {
                    Id = step.Id,
                    Name = name,
                    Location = step.Location,
                    X = x,
                    Y = y,
                };
            }));
        }

        private static List<(Guid Id, int Location)> GenerateSteps(int count)
        {
            var steps = new List<(Guid Id, int Location)>();
            for (int i = 1; i <= count; i++)
            {
                steps.Add((Guid.NewGuid(), i));
            }
            return steps;
        }
    }
}