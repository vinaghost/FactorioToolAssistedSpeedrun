using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.Database
{
    public class ReplacePointTests : DatabaseBaseTests
    {
        public ReplacePointTests(DatabaseFixture databaseFixture) : base(databaseFixture)
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
            var (context, _) = _fixture.SeedSteps(StepCategoryName, Amount, SingleMatchPosition);
            var before = context.GetSteps(StepCategoryName).ToDictionary(s => s.Id);
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));

            // Act
            command.DatabaseCommit(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            foreach (var step in steps)
            {
                var expected = step.Location == 3 ? (11.5, 21.25) : (before[step.Id].X, before[step.Id].Y);
                Assert.Equal(expected, (step.X, step.Y));
            }
        }

        [Fact]
        public void ReplacePoint_UpdatesAllStepsSharingThePoint()
        {
            // Arrange
            var (context, _) = _fixture.SeedSteps(StepCategoryName, Amount, SharedPointPosition);
            var before = context.GetSteps(StepCategoryName).ToDictionary(s => s.Id);
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));

            // Act
            command.DatabaseCommit(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            foreach (var step in steps)
            {
                var original = before[step.Id];
                var expected = original.X == 10 && original.Y == 20 ? (11.5, 21.25) : (original.X, original.Y);
                Assert.Equal(expected, (step.X, step.Y));
            }
        }

        [Fact]
        public void ReplacePointRollback_RestoresOriginalPositions()
        {
            // Arrange
            var (context, _) = _fixture.SeedSteps(StepCategoryName, Amount, SharedPointPosition);
            var before = context.GetSteps(StepCategoryName).ToDictionary(s => s.Id);
            var command = CreateCommand();
            command.Setup(new ReplacePointCommandParameters(StepCategoryName, 10, 20, 11.5, 21.25));
            command.DatabaseCommit(context);

            // Act
            command.DatabaseRollback(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            foreach (var step in steps)
            {
                Assert.Equal((before[step.Id].X, before[step.Id].Y), (step.X, step.Y));
            }
        }
    }
}
