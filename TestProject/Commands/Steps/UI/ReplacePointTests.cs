using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.UI
{
    [Collection("Step model")]
    public class ReplacePointTests : UIBaseTests
    {
        public ReplacePointTests(UIFixture fixture) : base(fixture)
        {
        }

        private static ReplacePointCommand CreateCommand()
        {
            var dataService = Substitute.For<IDataService>();
            return new ReplacePointCommand(dataService, new PanelService(dataService), Substitute.For<ICommandStack>());
        }

        private static (double X, double Y) SingleMatchPosition(int location)
        {
            return location == 3 ? (10, 20) : (location * 3, -location);
        }

        private static (double X, double Y) SharedPointPosition(int location)
        {
            return (location is 2 or 5 or 9) ? (10, 20) : (location * 3, -location);
        }

        [Fact]
        public void ReplacePoint_UpdatesSingleMatchingStep()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(20);
            SetPositions(collection, SingleMatchPosition);
            var before = collection.ToDictionary(s => s.Id, s => (s.X, s.Y));
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));

            // Act
            command.UICommit(collection);

            // Assert
            Assert.Equal(20, collection.Count);
            Assert.Equal(Enumerable.Range(1, 20), collection.Select(x => x.Location));
            foreach (var step in collection)
            {
                var expected = step.Location == 3 ? ("11.50", "21.25") : before[step.Id];
                Assert.Equal(expected, (step.X, step.Y));
            }
        }

        [Fact]
        public void ReplacePoint_UpdatesAllStepsSharingThePoint()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(20);
            SetPositions(collection, SharedPointPosition);
            var before = collection.ToDictionary(s => s.Id, s => (s.X, s.Y));
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));

            // Act
            command.UICommit(collection);

            // Assert
            Assert.Equal(20, collection.Count);
            foreach (var step in collection)
            {
                var expected = before[step.Id] == ("10.00", "20.00") ? ("11.50", "21.25") : before[step.Id];
                Assert.Equal(expected, (step.X, step.Y));
            }
        }

        [Fact]
        public void ReplacePointRollback_RestoresOriginalPositions()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(20);
            SetPositions(collection, SharedPointPosition);
            var before = collection.ToDictionary(s => s.Id, s => (s.X, s.Y));
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));
            command.UICommit(collection);

            // Act
            command.UIRollback(collection);

            // Assert
            Assert.Equal(20, collection.Count);
            foreach (var step in collection)
            {
                Assert.Equal(before[step.Id], (step.X, step.Y));
            }
        }
    }
}
