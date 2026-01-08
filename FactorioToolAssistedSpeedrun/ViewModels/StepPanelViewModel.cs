using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
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
        private readonly StepTypePanelViewModel _stepTypePanelViewModel;
        private readonly StepDetailPanelViewModel _stepDetailPanelViewModel;
        private readonly CommandStack _commandStack;

        public StepPanelViewModel()
        {
            _stepDetailPanelViewModel = App.Current.Services.GetRequiredService<StepDetailPanelViewModel>();
            _stepTypePanelViewModel = App.Current.Services.GetRequiredService<StepTypePanelViewModel>();
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public StepPanelViewModel(StepTypePanelViewModel stepTypePanelViewModel, StepDetailPanelViewModel stepDetailPanelViewModel, CommandStack commandStack)
        {
            _stepTypePanelViewModel = stepTypePanelViewModel;
            _stepDetailPanelViewModel = stepDetailPanelViewModel;
            _commandStack = commandStack;
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

            _stepTypePanelViewModel.SelectedStepType = step.Type;
            _stepDetailPanelViewModel.Load(step);
        }

        [RelayCommand]
        public async Task Add(bool rightClick)
        {
            var step = _stepDetailPanelViewModel.ToStep(_stepTypePanelViewModel.SelectedStepType);
            var index = rightClick ? SelectedIndex + 1 : SelectedIndex;
            step.Location = index + 1;
            var command = new AddStepCommand
            {
                Steps = [step.ToEntity()],
            };
            command.Commit(StepCollection);
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
                Steps = [.. items.Select(x => x.ToEntity())],
            };
            command.Commit(StepCollection);
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();

            var command = new MoveStepCommand
            {
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -1,
            };
            command.Commit(StepCollection);
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -5,
            };
            command.Commit(StepCollection);
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 1,
            };
            command.Commit(StepCollection);
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 5,
            };
            command.Commit(StepCollection);
            _commandStack.Push(command);
        }

        public void LoadItems(GameData gameData)
        {
            ItemCollection.Clear();

            ItemCollection.AddRange(gameData.Items.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Recipes.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Technologies.Select(x => x.Key));
        }

        public void LoadSteps(List<Step> steps)
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
                    StepModel model = new();
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