using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.UI
{
    [Collection("Step model")]
    public class ApplySkipTests : UIBaseTests
    {
        public ApplySkipTests(UIFixture fixture) : base(fixture)
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
        [InlineData(2, 3)]
        [InlineData(Amount, 1)]
        public void ApplySkip_TogglesIsSkipForSelectedSteps(int startIndex, int count)
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(Amount);
            var selectedIds = collection.Where(s => s.Location >= startIndex && s.Location < startIndex + count).Select(s => s.Id).ToList();
            var command = CreateCommand();
            command.Setup(new ApplySkipCommandParameters(StepCategoryName, selectedIds));

            // Act
            command.UICommit(collection);

            // Assert
            Assert.Equal(Amount, collection.Count);
            Assert.Equal(Enumerable.Range(1, Amount), collection.Select(x => x.Location));
            Assert.All(collection, step => Assert.Equal(selectedIds.Contains(step.Id), step.IsSkip));
        }

        [Fact]
        public void ApplySkipRollback_RestoresOriginalIsSkip()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(Amount);
            var selectedIds = collection.Where(s => s.Location >= 2 && s.Location <= 4).Select(s => s.Id).ToList();
            var command = CreateCommand();
            command.Setup(new ApplySkipCommandParameters(StepCategoryName, selectedIds));
            command.UICommit(collection);

            // Act
            command.UIRollback(collection);

            // Assert
            Assert.Equal(Amount, collection.Count);
            Assert.All(collection, step => Assert.False(step.IsSkip));
        }
    }
}
