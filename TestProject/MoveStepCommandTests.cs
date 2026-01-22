using FactorioToolAssistedSpeedrun;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace TestProject
{
    public class MoveStepCommandTests
    {
        private static ProjectDbContext GetInMemoryDbContext()
        {
            var context = new ProjectDbContext("test");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SetupTriggers();
            return context;
        }

        private static void SeedSteps(ProjectDbContext context, string name, List<(Guid id, int location)> steps)
        {
            foreach (var (id, location) in steps)
            {
                context.Steps.Add(new Step
                {
                    Id = id,
                    Name = name,
                    Location = location
                });
            }
            context.SaveChanges();
        }

        [Fact]
        public void DatabaseCommit_MovesStepsDown_UpdatesLocationsCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var name = "";
            var stepIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            SeedSteps(context, name,
            [
                (Guid.NewGuid(), 1),
                (stepIds[0], 2),
                (stepIds[1], 3),
                (Guid.NewGuid(), 4),
                (Guid.NewGuid(), 5),
            ]);

            // Act
            MoveStepCommand.DatabaseCommit(context, stepIds, 1, name);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == name).OrderBy(s => s.Location).ToList();
            Assert.Equal(5, steps.Count);
            Assert.Contains(steps, s => s.Location == 1);
            Assert.Contains(steps, s => s.Id == stepIds[0] && s.Location == 3);
            Assert.Contains(steps, s => s.Id == stepIds[1] && s.Location == 4);
            Assert.Contains(steps, s => s.Location == 4);
            Assert.Contains(steps, s => s.Location == 5);
        }

        [Fact]
        public void DatabaseCommit_MovesStepsUp_UpdatesLocationsCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var name = "";
            var stepIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            SeedSteps(context, name,
            [
                (Guid.NewGuid(), 1),
                (Guid.NewGuid(), 2),
                (stepIds[0], 3),
                (stepIds[1], 4),
                (Guid.NewGuid(), 5),
            ]);

            // Act
            MoveStepCommand.DatabaseCommit(context, stepIds, -1, name);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == name).OrderBy(s => s.Location).ToList();
            Assert.Equal(5, steps.Count);
            Assert.Contains(steps, s => s.Location == 1);
            Assert.Contains(steps, s => s.Id == stepIds[0] && s.Location == 2);
            Assert.Contains(steps, s => s.Id == stepIds[1] && s.Location == 3);
            Assert.Contains(steps, s => s.Location == 4);
            Assert.Contains(steps, s => s.Location == 5);
        }

        [Fact]
        public void DatabaseCommit_EmptyStepIds_DoesNothing()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var name = "";
            SeedSteps(context, name,
            [
                (Guid.NewGuid(), 1),
                (Guid.NewGuid(), 2)
            ]);

            // Act
            MoveStepCommand.DatabaseCommit(context, [], 1, name);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == name).OrderBy(s => s.Location).ToList();
            Assert.Equal(2, steps.Count);
            Assert.Equal(1, steps[0].Location);
            Assert.Equal(2, steps[1].Location);
        }
    }
}