using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using NSubstitute;
using System.Collections.ObjectModel;

namespace TestProject.Commands.Steps.UI
{
    public class UIFixture
    {
        public ObservableCollection<StepModel> Collection { get; } = [];

        public void SeedSteps(int count)
        {
            Collection.Clear();
            var fakeCommandStack = Substitute.For<ICommandStack>();
            var fakeStartupService = Substitute.For<IDataService>();
            for (int i = 0; i < count; i++)
            {
                Collection.Add(new StepModel(fakeCommandStack, fakeStartupService)
                {
                    // Set Id and Name for test clarity
                    // Name is not used in GoDown
                    // Location is sequential
                    // Id is deterministic for test
                    // Use Guid based on index for reproducibility
                    // e.g. Guid.Parse($"00000000-0000-0000-0000-00000000000{i}")
                    // But for simplicity, use Guid.NewGuid()
                    // We'll track the Ids in a list for selection
                    Location = i + 1,
                    // Name = $"Step{i}",
                });
            }
        }
    }
}