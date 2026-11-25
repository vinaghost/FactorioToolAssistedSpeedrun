using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Services;

using NSubstitute;

namespace TestProject.Commands.Steps.UI
{
    [Collection("Step model")]
    public class SortStepTests : UIBaseTests
    {
        public SortStepTests(UIFixture fixture) : base(fixture)
        {
        }

        private static SortStepCommand CreateCommand()
        {
            var dataService = Substitute.For<IDataService>();
            return new SortStepCommand(dataService, new PanelService(dataService));
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
            var (collection, _) = _fixture.SeedSteps(6);
            SetPositions(collection, SortPosition);
            var selected = collection.Where(s => s.Location >= 3 && s.Location <= 6).Select(x => x.ToEntity()).ToList();
            var idsByOriginalLocation = selected.ToDictionary(s => s.Location, s => s.Id);
            var command = CreateCommand();
            command.Setup(new SortStepCommandParameters(StepCategoryName, selected, direction, sortFirst));

            // Act
            command.UICommit(collection);

            // Assert
            Assert.Equal(6, collection.Count);
            Assert.Equal(Enumerable.Range(1, 6), collection.Select(x => x.Location));
            var expectedIds = expectedOrder.Select(location => idsByOriginalLocation[location]).ToList();
            var sortedIds = collection.Where(s => s.Location >= 3 && s.Location <= 6).Select(s => s.Id).ToList();
            Assert.Equal(expectedIds, sortedIds);
        }

        [Fact]
        public void SortRollback_RestoresOriginalOrder()
        {
            // Arrange
            var (collection, _) = _fixture.SeedSteps(6);
            SetPositions(collection, SortPosition);
            var originalIds = collection.Select(s => s.Id).ToList();
            var selected = collection.Where(s => s.Location >= 3 && s.Location <= 6).Select(x => x.ToEntity()).ToList();
            var command = CreateCommand();
            command.Setup(new SortStepCommandParameters(StepCategoryName, selected, SortDirectionType.TopLeft, SortFirstType.X));
            command.UICommit(collection);

            // Act
            command.UIRollback(collection);

            // Assert
            Assert.Equal(6, collection.Count);
            Assert.Equal(originalIds, collection.Select(s => s.Id));
            Assert.Equal(Enumerable.Range(1, 6), collection.Select(x => x.Location));
        }
    }
}
