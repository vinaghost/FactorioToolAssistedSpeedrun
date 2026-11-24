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
            var steps = context.GetSteps(StepCategoryName);
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
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            for (int i = 0; i < moveCount; i++)
            {
                var stepId = stepIds[i];
                Assert.Equal(expectedIndex + i, steps.First(s => s.Id == stepId).Location);
            }
        }

        [Theory]
        [InlineData(1, 3, 1, 2, 4)]
        [InlineData(2, 4, -1, 1, 3)]
        [InlineData(5, 10, 2, 7, 12)]
        public void MoveNonContiguousSteps_KeepLocationsUnique(int firstIndex, int secondIndex, int moveOffset, int expectedFirst, int expectedSecond)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var firstId = initialSteps.First(s => s.Location == firstIndex).Id;
            var secondId = initialSteps.First(s => s.Location == secondIndex).Id;

            // Act
            context.MoveSteps(StepCategoryName, [firstId, secondId], moveOffset);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            Assert.Equal(expectedFirst, steps.First(s => s.Id == firstId).Location);
            Assert.Equal(expectedSecond, steps.First(s => s.Id == secondId).Location);
        }

        [Fact]
        public void MoveNonContiguousStepsDown_ReloadedOrderMatchesMove()
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var first = initialSteps.First(s => s.Location == 1).Id;
            var second = initialSteps.First(s => s.Location == 3).Id;
            var expectedOrder = initialSteps.OrderBy(s => s.Location).Select(s => s.Id).ToList();
            (expectedOrder[0], expectedOrder[1]) = (expectedOrder[1], expectedOrder[0]);
            (expectedOrder[2], expectedOrder[3]) = (expectedOrder[3], expectedOrder[2]);

            // Act
            context.MoveSteps(StepCategoryName, [first, second], 1);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(expectedOrder, steps.Select(s => s.Id).ToList());
        }
    }
}