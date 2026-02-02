using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject
{
    using FactorioToolAssistedSpeedrun.Commands.Steps;
    using FactorioToolAssistedSpeedrun.Models.UI;
    using FactorioToolAssistedSpeedrun.Services;
    using NSubstitute;
    using System.Collections.ObjectModel;
    using Xunit;

    public class UIMoveStepCommandTests
    {
        private static ObservableCollection<StepModel> SeedSteps(int count)
        {
            var collection = new ObservableCollection<StepModel>();
            var fakeCommandStack = Substitute.For<ICommandStack>();
            var fakeStartupService = Substitute.For<IDataService>();
            for (int i = 0; i < count; i++)
            {
                collection.Add(new StepModel(fakeCommandStack, fakeStartupService)
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
            return collection;
        }

        [Fact]
        public void GoDown_MovesBlockDownBy1_CorrectOrderAndLocations()
        {
            // Arrange
            var collection = SeedSteps(10);
            // Select steps at indices 3,4,5
            var stepIds = new List<Guid> { collection[3].Id, collection[4].Id, collection[5].Id };
            int moveOffset = 1;
            // Capture expected order: move item at index 6 above index 3
            var expectedOrder = new List<Guid>();
            expectedOrder.AddRange(collection.Take(3).Select(x => x.Id));
            expectedOrder.Add(collection[6].Id); // moved up
            expectedOrder.AddRange(collection.Skip(3).Take(3).Select(x => x.Id)); // selected
            expectedOrder.AddRange(collection.Skip(7).Select(x => x.Id));

            // Act
            MoveStepCommand.GoDown(collection, stepIds, moveOffset);

            // Assert
            Assert.Equal(10, collection.Count);
            Assert.Equal(expectedOrder, collection.Select(x => x.Id));
            // Check Location property is sequential and correct
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.Equal(i + 1, collection[i].Location);
            }
        }

        [Fact]
        public void GoDown_MovesBlockDownBy5_CorrectOrderAndLocations()
        {
            // Arrange
            var collection = SeedSteps(10);
            // Select steps at indices 3,4,5
            var stepIds = new List<Guid> { collection[3].Id, collection[4].Id, collection[5].Id };
            int moveOffset = 5;
            // Capture expected order: move items at indices 6,7,8,9 (max 4, since only 4 below) above index 3
            var expectedOrder = new List<Guid>();
            expectedOrder.AddRange(collection.Take(3).Select(x => x.Id));
            expectedOrder.AddRange(collection.Skip(6).Take(4).Select(x => x.Id)); // moved up
            expectedOrder.AddRange(collection.Skip(3).Take(3).Select(x => x.Id)); // selected

            // Act
            MoveStepCommand.GoDown(collection, stepIds, moveOffset);

            // Assert
            Assert.Equal(10, collection.Count);
            Assert.Equal(expectedOrder, collection.Select(x => x.Id));
            // Check Location property is sequential and correct
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.Equal(i + 1, collection[i].Location);
            }
        }

        [Fact]
        public void GoUp_MovesBlockUpBy1_CorrectOrderAndLocations()
        {
            // Arrange
            var collection = SeedSteps(10);
            // Select steps at indices 3,4,5
            var stepIds = new List<Guid> { collection[3].Id, collection[4].Id, collection[5].Id };
            int moveOffset = -1;
            // Capture expected order: move item at index 2 below index 5
            var expectedOrder = new List<Guid>();
            expectedOrder.AddRange(collection.Take(2).Select(x => x.Id)); // 0,1
            expectedOrder.AddRange(collection.Skip(3).Take(3).Select(x => x.Id)); // 3,4,5
            expectedOrder.Add(collection[2].Id); // 2
            expectedOrder.AddRange(collection.Skip(6).Select(x => x.Id));

            // Act
            MoveStepCommand.GoUp(collection, stepIds, moveOffset);

            // Assert
            Assert.Equal(10, collection.Count);
            Assert.Equal(expectedOrder, collection.Select(x => x.Id));
            // Check Location property is sequential and correct
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.Equal(i + 1, collection[i].Location);
            }
        }

        [Fact]
        public void GoUp_MovesBlockUpBy5_CorrectOrderAndLocations()
        {
            // Arrange
            var collection = SeedSteps(10);
            // Select steps at indices 3,4,5
            var stepIds = new List<Guid> { collection[3].Id, collection[4].Id, collection[5].Id };
            int moveOffset = -5;
            // Only 3 items above index 3, so blockSize = 3
            // Capture expected order: move items at indices 0,1,2 below index 5
            var expectedOrder = new List<Guid>();
            expectedOrder.AddRange(collection.Skip(3).Take(3).Select(x => x.Id)); // 3,4,5
            expectedOrder.AddRange(collection.Take(3).Select(x => x.Id)); // 0,1,2 moved down
            expectedOrder.AddRange(collection.Skip(6).Select(x => x.Id));

            // Act
            MoveStepCommand.GoUp(collection, stepIds, moveOffset);

            // Assert
            Assert.Equal(10, collection.Count);
            Assert.Equal(expectedOrder, collection.Select(x => x.Id));
            // Check Location property is sequential and correct
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.Equal(i + 1, collection[i].Location);
            }
        }
    }
}