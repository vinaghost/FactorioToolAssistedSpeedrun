using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.UI
{
    [Collection("Step model")]
    public class UpdateStepPropertyTests : UIBaseTests
    {
        public UpdateStepPropertyTests(UIFixture fixture) : base(fixture)
        {
        }

        private static UpdateStepPropertyCommand<string, double> CreateCommand()
        {
            var dataService = Substitute.For<IDataService>();
            return new UpdateStepPropertyCommand<string, double>(dataService, new PanelService(dataService), Substitute.For<ICommandStack>());
        }

        private static UpdateStepPropertyCommandParameters<string, double> CreatePositionParameters(Guid stepId, string oldValue, string newValue)
        {
            return new(StepCategoryName, stepId, oldValue, newValue, str => double.Parse(str), step => step.X, model => model.X);
        }

        [Fact]
        public void UpdateStepPosition_UpdatesOnlyTargetStep()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(Amount);
            SetPositions(collection, location => location == 3 ? (5, 1) : (100 + location, 100 + location));
            var targetId = collection.First(s => s.Location == 3).Id;
            var before = collection.ToDictionary(s => s.Id, s => (s.X, s.Y));
            var command = CreateCommand();
            command.Setup(CreatePositionParameters(targetId, "5.00", "12.50"));

            // Act
            command.UICommit(collection);

            // Assert
            Assert.Equal(Amount, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount), collection.Select(x => x.Location));
            foreach (var step in collection)
            {
                var expected = step.Id == targetId ? ("12.50", before[step.Id].Y) : before[step.Id];
                Assert.Equal(expected, (step.X, step.Y));
            }
        }

        [Fact]
        public void UpdateStepPositionRollback_RestoresOldValue()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(Amount);
            SetPositions(collection, location => location == 3 ? (5, 1) : (100 + location, 100 + location));
            var targetId = collection.First(s => s.Location == 3).Id;
            var command = CreateCommand();
            command.Setup(CreatePositionParameters(targetId, "5.00", "12.50"));
            command.UICommit(collection);

            // Act
            command.UIRollback(collection);

            // Assert
            var target = collection.First(s => s.Id == targetId);
            Assert.Equal("5.00", target.X);
            Assert.Equal("1.00", target.Y);
        }
    }
}
