using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Commands.UI;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepPanelViewModel : ObservableObject
    {
        private readonly CommandStack _commandStack;
        private readonly StartupService _startupService;
        private readonly StepService _stepService;

        public StepPanelViewModel()
        {
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _stepService = App.Current.Services.GetRequiredService<StepService>();
        }

        [ActivatorUtilitiesConstructor]
        public StepPanelViewModel(CommandStack commandStack, StartupService startupService, StepService stepService)
        {
            _commandStack = commandStack;
            _startupService = startupService;
            _stepService = stepService;

            _startupService.OnProjectDataLoaded += OnProjectDataLoaded;
            _startupService.OnGameDataLoaded += OnGameDataLoaded;
        }

        private void OnGameDataLoaded()
        {
            LoadItems(_startupService.GameData!);
        }

        private void OnProjectDataLoaded()
        {
            var loadStepsCommand = new LoadStepsCommand
            {
                ProjectDataFile = _startupService.ProjectDataFile,
            };
            loadStepsCommand.Execute();
            LoadSteps(loadStepsCommand.Result);
        }

        [ObservableProperty]
        private StepModel? _selectedItem;

        [ObservableProperty]
        private int _selectedIndex;

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];
        public List<string> ItemCollection { get; set; } = [];

        public Action? StepsChangeStarted;

        public Action? StepsChangeCompleted;
        public Action? ScrollToSelected;

        [RelayCommand]
        public async Task MouseRightButtonUp(DataGridRow row)
        {
            var index = row.GetIndex();
            var step = StepCollection[index];
            _stepService.FromStep(step);
        }

        [RelayCommand]
        public async Task Add(bool rightClick)
        {
            var step = _stepService.ToStep();
            var index = rightClick ? SelectedIndex + 1 : SelectedIndex;
            step.Location = index + 1;
            var command = new AddStepCommand
            {
                Collection = StepCollection,
                Steps = [step],
            };
            command.Commit();
            _commandStack.Push(command);

            SelectedIndex = index;
        }

        [RelayCommand]
        public async Task Delete(System.Collections.IList selectedItems)
        {
            var result = MessageBox.Show("Are you sure want to delete these steps?", "Warning", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new DeleteStepCommand
            {
                Collection = StepCollection,
                Steps = [.. items.Select(x => x.ToEntity())],
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();

            var command = new MoveStepCommand
            {
                Collection = StepCollection,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -1,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Collection = StepCollection,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -5,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Collection = StepCollection,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 1,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Collection = StepCollection,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 5,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        private void LoadItems(GameData gameData)
        {
            ItemCollection.Clear();

            ItemCollection.AddRange(gameData.Items.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Recipes.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Technologies.Select(x => x.Key));
        }

        private void LoadSteps(List<Step> steps)
        {
            StepsChangeStarted?.Invoke();

            if (steps.Count < StepCollection.Count)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                while (StepCollection.Count > steps.Count)
                {
                    StepCollection.RemoveAt(StepCollection.Count - 1);
                }
            }
            else if (steps.Count > StepCollection.Count)
            {
                for (int i = 0; i < StepCollection.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                for (int i = StepCollection.Count; i < steps.Count; i++)
                {
                    StepModel model = new() { Collection = StepCollection };
                    model.FromEntity(steps[i]);
                    StepCollection.Add(model);
                }
            }
            else
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
            }
            StepsChangeCompleted?.Invoke();
        }
    }
}