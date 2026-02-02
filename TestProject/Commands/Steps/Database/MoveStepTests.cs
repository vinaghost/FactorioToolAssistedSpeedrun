using FactorioToolAssistedSpeedrun.Commands.Steps;
using Microsoft.EntityFrameworkCore;

namespace TestProject.Commands.Steps.Database
{
    public class MoveStepTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public MoveStepTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void DatabaseCommit_MovesStepsDown_UpdatesLocationsCorrectly()
        {
            // Arrange
            var name = "";
            var stepIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _fixture.SeedSteps(name,
            [
                (Guid.NewGuid(), 1),
                (stepIds[0], 2),
                (stepIds[1], 3),
                (Guid.NewGuid(), 4),
                (Guid.NewGuid(), 5),
            ]);
            var context = _fixture.Context;

            // Act
            context.MoveSteps(name, stepIds, 1);

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
            var name = "";
            var stepIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _fixture.SeedSteps(name,
            [
                (Guid.NewGuid(), 1),
                (Guid.NewGuid(), 2),
                (stepIds[0], 3),
                (stepIds[1], 4),
                (Guid.NewGuid(), 5),
            ]);
            var context = _fixture.Context;

            // Act
            context.MoveSteps(name, stepIds, -1);

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
            var name = "";
            _fixture.SeedSteps(name,
            [
                (Guid.NewGuid(), 1),
                (Guid.NewGuid(), 2)
            ]);
            var context = _fixture.Context;

            // Act
            context.MoveSteps(name, [], 1);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == name).OrderBy(s => s.Location).ToList();
            Assert.Equal(2, steps.Count);
            Assert.Equal(1, steps[0].Location);
            Assert.Equal(2, steps[1].Location);
        }
    }
}