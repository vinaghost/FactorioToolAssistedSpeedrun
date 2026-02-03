using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace TestProject.Commands.Steps.Database
{
    public class AddStepsTests : DatabaseBaseTests
    {
        public AddStepsTests(DatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void AddSingleStep_InsertsCorrectly(int insertAt)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var newStep = new Step { Id = Guid.NewGuid(), Name = StepCategoryName, Location = insertAt };

            var expectedSteps = new List<Guid>();
            expectedSteps.AddRange(initialSteps.Where(s => s.Location < insertAt).OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.Add(newStep.Id);
            expectedSteps.AddRange(initialSteps.Where(s => s.Location >= insertAt).OrderBy(s => s.Location).Select(s => s.Id));

            // Act
            context.AddSteps(StepCategoryName, [newStep]);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount + 1, steps.Count);
            Assert.Equal(expectedSteps, steps.Select(x => x.Id));
            Assert.Equal(Enumerable.Range(1, Amount + 1), steps.Select(x => x.Location));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(3, 3)]
        [InlineData(5, 3)]
        public void AddMultipleStep_InsertsCorrectly(int insertAt, int count)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);

            var newSteps = new List<Step>();
            for (var i = 0; i < count; i++)
            {
                newSteps.Add(new Step { Id = Guid.NewGuid(), Name = StepCategoryName, Location = insertAt + i });
            }

            var expectedSteps = new List<Guid>();
            expectedSteps.AddRange(initialSteps.Where(s => s.Location < insertAt).OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.AddRange(newSteps.OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.AddRange(initialSteps.Where(s => s.Location >= insertAt).OrderBy(s => s.Location).Select(s => s.Id));

            // Act
            context.AddSteps(StepCategoryName, newSteps);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount + count, steps.Count);
            Assert.Equal(expectedSteps, steps.Select(x => x.Id));
            Assert.Equal(Enumerable.Range(1, Amount + count), steps.Select(x => x.Location));
        }
    }
}