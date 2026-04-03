using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace TestProject.Commands.Steps.Database
{
    public class MoveStepTests : DatabaseBaseTests
    {
        public MoveStepTests(DatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(2, 1, 3)]
        [InlineData(3, -1, 2)]
        [InlineData(3, Amount, Amount)]
        [InlineData(3, -Amount, 1)]
        public void MoveSingleStep_UpdateLocationCorrectly(int index, int moveOffset, int expectedIndex)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var (Id, _) = initialSteps.First(s => s.Location == index);

            // Act
            context.MoveSteps(StepCategoryName, [Id], moveOffset);

            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            Assert.Equal(expectedIndex, steps.First(s => s.Id == Id).Location);
        }

        [Theory]
        [InlineData(1, 2, 2, 3)]
        [InlineData(2, 3, 1, 3)]
        [InlineData(3, 2, -1, 2)]
        [InlineData(3, 2, Amount, Amount - 1)]
        [InlineData(3, 2, -Amount, 1)]
        public void MoveMultipleSteps_UpdateLocationsCorrectly(int startIndex, int moveCount, int moveOffset, int expectedIndex)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var stepIds = initialSteps.Where(s => s.Location >= startIndex && s.Location < startIndex + moveCount).Select(s => s.Id).ToList();
            // Act
            context.MoveSteps(StepCategoryName, [.. stepIds], moveOffset);
            // Assert
            context.ChangeTracker.Clear();
            var steps = context.Steps.Where(s => s.Name == StepCategoryName).OrderBy(s => s.Location).ToList();
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            for (int i = 0; i < moveCount; i++)
            {
                var stepId = stepIds[i];
                Assert.Equal(expectedIndex + i, steps.First(s => s.Id == stepId).Location);
            }
        }
    }
}