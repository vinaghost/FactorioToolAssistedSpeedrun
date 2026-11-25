using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.Database
{
    public class UpdateStepPropertyTests : DatabaseBaseTests
    {
        public UpdateStepPropertyTests(DatabaseFixture databaseFixture) : base(databaseFixture)
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

        private static (double X, double Y) Position(int location)
        {
            return location == 3 ? (5, 1) : (100 + location, 100 + location);
        }

        [Fact]
        public void UpdateStepPosition_UpdatesOnlyTargetStep()
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount, Position);
            var targetId = initialSteps.First(s => s.Location == 3).Id;
            var before = context.GetSteps(StepCategoryName).ToDictionary(s => s.Id);
            var command = CreateCommand();
            command.Setup(CreatePositionParameters(targetId, "5.00", "12.50"));

            // Act
            command.DatabaseCommit(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            foreach (var step in steps)
            {
                var expectedX = step.Id == targetId ? 12.5 : before[step.Id].X;
                Assert.Equal(expectedX, step.X);
                Assert.Equal(before[step.Id].Y, step.Y);
            }
        }

        [Fact]
        public void UpdateStepPositionRollback_RestoresOldValue()
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount, Position);
            var targetId = initialSteps.First(s => s.Location == 3).Id;
            var command = CreateCommand();
            command.Setup(CreatePositionParameters(targetId, "5.00", "12.50"));
            command.DatabaseCommit(context);

            // Act
            command.DatabaseRollback(context);

            // Assert
            var target = context.GetSteps(StepCategoryName).First(s => s.Id == targetId);
            Assert.Equal(5, target.X);
            Assert.Equal(1, target.Y);
        }
    }
}
