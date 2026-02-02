using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Queries;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class ReplaceViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly ICommandStack _commandStack;

        public ReplaceViewModel()
        {
            _dataService = App.Current.Services.GetRequiredService<IDataService>();
            _commandStack = App.Current.Services.GetRequiredService<ICommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public ReplaceViewModel(IDataService dataService, ICommandStack commandStack)
        {
            _dataService = dataService;
            _commandStack = commandStack;
        }

        [ObservableProperty]
        private double _findX;

        [ObservableProperty]
        private double _findY;

        [ObservableProperty]
        private double _replaceX;

        [ObservableProperty]
        private double _replaceY;

        [ObservableProperty]
        private int _instancesFound;

        [RelayCommand]
        private void Find()
        {
            if (!_dataService.IsProjectDataLoaded)
            {
                MessageBox.Show("Project data is not loaded. Please load the project data file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var countPointQuery = new CountPointQuery
            {
                ProjectDataFile = _dataService.ProjectDataFile,
                X = FindX,
                Y = FindY
            };

            InstancesFound = countPointQuery.Execute();
        }

        [RelayCommand]
        private void Replace()
        {
            if (!_dataService.IsProjectDataLoaded)
            {
                MessageBox.Show("Project data is not loaded. Please load the project data file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var command = _commandStack.Push<ReplacePointCommand>();
            if (command is not null)
            {
                command.Setup(new("", FindX, FindY, ReplaceX, ReplaceY));
                command.Commit();
            }

            MessageBox.Show($"Replaced {InstancesFound} instances of point ({FindX}, {FindY}) with ({ReplaceX}, {ReplaceY}).", "Replace Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}