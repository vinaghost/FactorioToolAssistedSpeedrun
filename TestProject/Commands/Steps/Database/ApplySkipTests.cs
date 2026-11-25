using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.Database
{
    public class ApplySkipTests : DatabaseBaseTests
    {
        public ApplySkipTests(DatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        private static ApplySkipCommand CreateCommand()
        {
            var dataService = Substitute.For<IDataService>();
            return new ApplySkipCommand(dataService, new PanelService(dataService), Substitute.For<ICommandStack>());
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, Amount)]
        [InlineData(3, 4)]
        [InlineData(Amount, 1)]
        public void ApplySkip_TogglesIsSkipForSelectedSteps(int startIndex, int count)
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var selectedIds = initialSteps.Where(s => s.Location >= startIndex && s.Location < startIndex + count).Select(s => s.Id).ToList();
            var command = CreateCommand();
            command.Setup(new ApplySkipCommandParameters(StepCategoryName, selectedIds));

            // Act
            command.DatabaseCommit(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            Assert.All(steps, step => Assert.Equal(selectedIds.Contains(step.Id), step.IsSkip));
        }

        [Fact]
        public void ApplySkipRollback_RestoresOriginalIsSkip()
        {
            // Arrange
            var (context, initialSteps) = _fixture.SeedSteps(StepCategoryName, Amount);
            var selectedIds = initialSteps.Where(s => s.Location >= 2 && s.Location <= 4).Select(s => s.Id).ToList();
            var command = CreateCommand();
            command.Setup(new ApplySkipCommandParameters(StepCategoryName, selectedIds));
            command.DatabaseCommit(context);

            // Act
            command.DatabaseRollback(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.All(steps, step => Assert.False(step.IsSkip));
        }
    }
}
