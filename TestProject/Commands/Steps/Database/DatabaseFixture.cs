using FactorioToolAssistedSpeedrun;
using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace TestProject.Commands.Steps.Database
{
    public class DatabaseFixture : IDisposable
    {
        public ProjectDbContext Context { get; private set; }

        public DatabaseFixture()
        {
            Context = GetInMemoryDbContext();
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        private static ProjectDbContext GetInMemoryDbContext()
        {
            var context = new ProjectDbContext("test");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SetupTriggers();
            return context;
        }

        public void SeedSteps(string name, List<(Guid id, int location)> steps)
        {
            Context.Steps.ExecuteDelete();
            foreach (var (id, location) in steps)
            {
                Context.Steps.Add(new Step
                {
                    Id = id,
                    Name = name,
                    Location = location
                });
            }
            Context.SaveChanges();
        }
    }
}