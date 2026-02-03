using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace TestProject.Commands.Steps.Database
{
    public class DeleteStepsTests : DatabaseBaseTests
    {
        public DeleteStepsTests(DatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void DeleteSingleStep_RemovesStepsAndUpdatesLocations(int deleteAt)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var stepsToDelete = context.Steps.Where(s => s.Location == deleteAt && s.Name == StepCategoryName).ToList();

            // Act
            context.DeleteSteps(StepCategoryName, stepsToDelete);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount - 1, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount - 1), steps.Select(x => x.Location));

            Assert.DoesNotContain(steps, s => s.Id == stepsToDelete[0].Id);
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(2, 2)]
        [InlineData(3, 2)]
        public void DeleteMultipleSteps_RemovesStepsAndUpdatesLocations(int deleteAt, int deleteCount)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var stepsToDelete = context.Steps.Where(s => s.Location >= deleteAt && s.Location < deleteAt + deleteCount && s.Name == StepCategoryName).ToList();
            // Act
            context.DeleteSteps(StepCategoryName, stepsToDelete);
            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount - deleteCount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount - deleteCount), steps.Select(x => x.Location));
            foreach (var step in stepsToDelete)
            {
                Assert.DoesNotContain(steps, s => s.Id == step.Id);
            }
        }
    }
}