using FactorioToolAssistedSpeedrun;
using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

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
            _context.Dispose();
        }

        private static ProjectDbContext GetInMemoryDbContext()
        {
            var context = new ProjectDbContext("test");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SetupTriggers();
            return context;
        }

        public (ProjectDbContext, List<(Guid Id, int Location)>) SeedSteps(string name, int count)
        {
            var steps = GenerateSteps(count);
            AddSteps(name, steps);
            return (_context, steps);
        }

        private void AddSteps(string name, List<(Guid Id, int Location)> steps)
        {
            _context.Steps.ExecuteDelete();
            foreach (var (id, location) in steps)
            {
                _context.Steps.Add(new Step
                {
                    Id = id,
                    Name = name,
                    Location = location
                });
            }
            _context.SaveChanges();
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