using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Misc;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace TestProject.Commands.Steps.UI
{
    public class UIFixture
    {
        private readonly ObservableCollectionEx<StepModel> _collection = [];
        private readonly IServiceProvider _services;

        public UIFixture()
        {
            _services = ConfigureServices();
            Ioc.Default.ConfigureServices(_services);
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(x => Substitute.For<IDataService>());
            services.AddSingleton(x => Substitute.For<ICommandStack>());
            return services.BuildServiceProvider();
        }

        public (ObservableCollectionEx<StepModel>, List<StepModel>) SeedSteps(int count)
        {
            var steps = GenerateSteps(count);
            AddSteps(steps);
            return (_collection, steps);
        }

        private void AddSteps(List<StepModel> steps)
        {
            _collection.Clear();
            foreach (var step in steps)
            {
                _collection.Add(step);
            }
        }

        private static List<StepModel> GenerateSteps(int count)
        {
            var collection = new List<StepModel>();
            for (int i = 0; i < count; i++)
            {
                collection.Add(new StepModel()
                {
                    Location = i + 1,
                });
            }
            return collection;
        }
    }
}