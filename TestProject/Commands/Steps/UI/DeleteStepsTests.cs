using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace TestProject.Commands.Steps.UI
{
    [Collection("Step model")]
    public class DeleteStepsTests : UIBaseTests
    {
        public DeleteStepsTests(UIFixture fixture) : base(fixture)
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void DeleteSingleStep_RemovesStepsAndUpdatesLocations(int deleteAt)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);
            var stepsToDelete = collection.Where(s => s.Location == deleteAt).Select(x => x.ToEntity()).ToList();

            // Act
            collection.DeleteSteps(stepsToDelete);

            // Assert
            Assert.Equal(Amount - 1, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount - 1), collection.Select(x => x.Location));

            Assert.DoesNotContain(collection, s => s.Id == stepsToDelete[0].Id);
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(2, 2)]
        [InlineData(3, 2)]
        public void DeleteMultipleSteps_RemovesStepsAndUpdatesLocations(int deleteAt, int deleteCount)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);
            var stepsToDelete = collection.Where(s => s.Location >= deleteAt && s.Location < deleteAt + deleteCount).Select(x => x.ToEntity()).ToList();
            // Act
            collection.DeleteSteps(stepsToDelete);
            // Assert
            Assert.Equal(Amount - deleteCount, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount - deleteCount), collection.Select(x => x.Location));
            foreach (var step in stepsToDelete)
            {
                Assert.DoesNotContain(collection, s => s.Id == step.Id);
            }
        }
    }
}