using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace TestProject.Commands.Steps.UI
{
    public class MoveStepTests : UIBaseTests
    {
        public MoveStepTests(UIFixture fixture) : base(fixture)
        {
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(2, 1, 3)]
        [InlineData(3, -1, 2)]
        [InlineData(3, 5, Amount)]
        [InlineData(3, -5, 1)]
        public void MoveSingleStep_UpdateLocationCorrectly(int index, int moveOffset, int expectedIndex)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);
            var id = initialSteps.Where(s => s.Location == index).Select(x => x.Id).First();

            // Act
            collection.MoveSteps([id], moveOffset);

            // Assert
            Assert.Equal(Amount, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount), collection.Select(x => x.Location));
            Assert.Equal(expectedIndex, collection.First(s => s.Id == id).Location);
        }

        [Theory]
        [InlineData(1, 2, 2, 3)]
        [InlineData(2, 3, 1, 3)]
        [InlineData(3, 2, -1, 2)]
        [InlineData(3, 2, 5, Amount - 1)]
        [InlineData(3, 2, -5, 1)]
        public void MoveMultipleSteps_UpdateLocationsCorrectly(int startIndex, int moveCount, int moveOffset, int expectedIndex)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);
            var stepIds = initialSteps.Where(s => s.Location >= startIndex && s.Location < startIndex + moveCount).Select(s => s.Id).ToList();
            // Act
            collection.MoveSteps([.. stepIds], moveOffset);
            // Assert
            Assert.Equal(Amount, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount), collection.Select(x => x.Location));
            for (int i = 0; i < moveCount; i++)
            {
                var stepId = stepIds[i];
                Assert.Equal(expectedIndex + i, collection.First(s => s.Id == stepId).Location);
            }
        }
    }
}