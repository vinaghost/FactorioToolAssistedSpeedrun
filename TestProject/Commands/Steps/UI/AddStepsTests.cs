using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;

namespace TestProject.Commands.Steps.UI
{
    public class AddStepsTests : IClassFixture<UIFixture>
    {
        private readonly UIFixture _fixture;
        private const int Amount = 5;

        public AddStepsTests(UIFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void AddSingleStep_InsertsCorrectly(int insertAt)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);
            var newStep = new Step { Id = Guid.NewGuid(), Location = insertAt };

            var expectedSteps = new List<Guid>();
            expectedSteps.AddRange(initialSteps.Where(s => s.Location < insertAt).OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.Add(newStep.Id);
            expectedSteps.AddRange(initialSteps.Where(s => s.Location >= insertAt).OrderBy(s => s.Location).Select(s => s.Id));

            // Act
            collection.AddSteps([newStep]);

            // Assert
            Assert.Equal(Amount + 1, collection.Count);
            Assert.Equal(expectedSteps, collection.Select(x => x.Id));
            Assert.Equal(Enumerable.Range(1, Amount + 1), collection.Select(x => x.Location));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(3, 3)]
        [InlineData(5, 3)]
        public void AddMultipleStep_InsertsCorrectly(int insertAt, int count)
        {
            // Arrange
            var (collection, initialSteps) = _fixture.SeedSteps(Amount);

            var newSteps = new List<Step>();
            for (var i = 0; i < count; i++)
            {
                newSteps.Add(new Step { Id = Guid.NewGuid(), Location = insertAt + i });
            }

            var expectedSteps = new List<Guid>();
            expectedSteps.AddRange(initialSteps.Where(s => s.Location < insertAt).OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.AddRange(newSteps.OrderBy(s => s.Location).Select(s => s.Id));
            expectedSteps.AddRange(initialSteps.Where(s => s.Location >= insertAt).OrderBy(s => s.Location).Select(s => s.Id));

            // Act
            collection.AddSteps(newSteps);

            // Assert
            Assert.Equal(Amount + count, collection.Count);
            Assert.Equal(expectedSteps, collection.Select(x => x.Id));
            Assert.Equal(Enumerable.Range(1, Amount + count), collection.Select(x => x.Location));
        }
    }
}