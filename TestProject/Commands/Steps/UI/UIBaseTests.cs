using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;

namespace TestProject.Commands.Steps.UI
{
    [CollectionDefinition("Step model")]
    public class StepModelCollection : ICollectionFixture<UIFixture>
    {
    }

    public class UIBaseTests
    {
        protected const string StepCategoryName = "";
        protected const int Amount = 5;

        protected readonly UIFixture _fixture;

        public UIBaseTests(UIFixture fixture)
        {
            _fixture = fixture;
        }

        // Same layout as the database tests: locations 3-6 hold distinct positions used for sorting.
        protected static (double X, double Y) SortPosition(int location)
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

        protected static void SetPositions(IEnumerable<StepModel> steps, Func<int, (double X, double Y)> position)
        {
            foreach (var step in steps)
            {
                var (x, y) = position(step.Location);
                step.Type = StepType.Walk;
                step.X = $"{x:F2}";
                step.Y = $"{y:F2}";
            }
        }
    }
}