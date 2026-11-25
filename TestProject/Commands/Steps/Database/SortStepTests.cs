using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.Database
{
    public class SortStepTests : DatabaseBaseTests
    {
        public SortStepTests(DatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        private static SortStepCommand CreateCommand()
        {
            var dataService = Substitute.For<IDataService>();
            return new SortStepCommand(dataService, new PanelService(dataService));
        }

        // Locations 3-6 are the contiguous selection. Locations 3 and 6 share a Y value, and locations 3 and 5 share an X value.
        private static (double X, double Y) SortPosition(int location)
        {
            return location switch
            {
                3 => (5, 1),
                4 => (2, 7),
                5 => (5, -3),
                6 => (9, 1),
                _ => (100 + location, 100 + location),
            };
        }

        [Theory]
        [InlineData(SortDirectionType.TopLeft, SortFirstType.X, new[] { 4, 5, 3, 6 })]
        [InlineData(SortDirectionType.TopRight, SortFirstType.X, new[] { 6, 5, 3, 4 })]
        [InlineData(SortDirectionType.BottomLeft, SortFirstType.X, new[] { 4, 3, 5, 6 })]
        [InlineData(SortDirectionType.BottomRight, SortFirstType.X, new[] { 6, 3, 5, 4 })]
        [InlineData(SortDirectionType.TopLeft, SortFirstType.Y, new[] { 5, 3, 6, 4 })]
        [InlineData(SortDirectionType.TopRight, SortFirstType.Y, new[] { 5, 6, 3, 4 })]
        [InlineData(SortDirectionType.BottomLeft, SortFirstType.Y, new[] { 4, 3, 6, 5 })]
        [InlineData(SortDirectionType.BottomRight, SortFirstType.Y, new[] { 4, 6, 3, 5 })]
        public void SortSelectedSteps_UpdatesLocationsToSortedOrder(SortDirectionType direction, SortFirstType sortFirst, int[] expectedOrder)
        {
            // Arrange
            var (context, _) = _fixture.SeedSteps(StepCategoryName, Amount, SortPosition);
            var selected = context.GetSteps(StepCategoryName).Where(s => s.Location >= 3 && s.Location <= 6).ToList();
            var idsByOriginalLocation = selected.ToDictionary(s => s.Location, s => s.Id);
            var command = CreateCommand();
            command.Setup(new SortStepCommandParameters(StepCategoryName, selected, direction, sortFirst));

            // Act
            command.DatabaseCommit(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.Equal(Enumerable.Range(1, Amount), steps.Select(x => x.Location));
            var expectedIds = expectedOrder.Select(location => idsByOriginalLocation[location]).ToList();
            var sortedIds = steps.Where(s => s.Location >= 3 && s.Location <= 6).Select(s => s.Id).ToList();
            Assert.Equal(expectedIds, sortedIds);
        }

        [Fact]
        public void SortRollback_RestoresOriginalLocations()
        {
            // Arrange
            var (context, _) = _fixture.SeedSteps(StepCategoryName, Amount, SortPosition);
            var selected = context.GetSteps(StepCategoryName).Where(s => s.Location >= 3 && s.Location <= 6).ToList();
            var originalLocations = context.GetSteps(StepCategoryName).ToDictionary(s => s.Id, s => s.Location);
            var command = CreateCommand();
            command.Setup(new SortStepCommandParameters(StepCategoryName, selected, SortDirectionType.TopLeft, SortFirstType.X));
            command.DatabaseCommit(context);

            // Act
            command.DatabaseRollback(context);

            // Assert
            var steps = context.GetSteps(StepCategoryName);
            Assert.Equal(Amount, steps.Count);
            Assert.All(steps, step => Assert.Equal(originalLocations[step.Id], step.Location));
        }
    }
}
