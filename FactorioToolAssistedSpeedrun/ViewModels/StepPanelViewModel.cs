using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepPanelViewModel : ObservableObject
    {
        private readonly CommandStack _commandStack;
        private readonly StartupService _startupService;
        private readonly StepService _stepService;
        private readonly PanelService _panelService;

        public PanelService PanelService => _panelService;
        public StartupService StartupService => _startupService;

        public StepPanelViewModel()
        {
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _stepService = App.Current.Services.GetRequiredService<StepService>();
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public StepPanelViewModel(CommandStack commandStack, StartupService startupService, StepService stepService, PanelService panelService)
        {
            _commandStack = commandStack;
            _startupService = startupService;
            _stepService = stepService;
            _panelService = panelService;
        }

        [RelayCommand]
        public async Task MouseRightButtonUp(DataGridRow row)
        {
            var index = row.GetIndex();
            var step = _panelService.StepCollection[index];
            _stepService.FromStep(step);
        }

        [RelayCommand]
        public async Task Add(bool rightClick)
        {
            var step = _stepService.ToStep();
            var index = rightClick ? PanelService.SelectedStepIndex + 1 : PanelService.SelectedStepIndex;
            step.Location = index + 1;

            var command = _commandStack.Push<AddStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [step]));
                command.Commit();
            }

            PanelService.SelectedStepIndex = index;
        }

        [RelayCommand]
        public async Task Remove(System.Collections.IList selectedItems)
        {
            var result = MessageBox.Show("Are you sure want to delete these steps?", "Warning", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var items = selectedItems.OfType<StepModel>().ToList();

            var command = _commandStack.Push<DeleteStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.ToEntity())]));
                command.Commit();
            }
        }

        [RelayCommand]
        public async Task MoveUpOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();

            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.Id)], -1));
                command.Commit();
            }
        }

        [RelayCommand]
        public async Task MoveUpFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.Id)], -5));
                command.Commit();
            }
        }

        [RelayCommand]
        public async Task MoveDownOne(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.Id)], 1));
                command.Commit();
            }
        }

        [RelayCommand]
        public async Task MoveDownFive(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.Id)], 5));
                command.Commit();
            }
        }

        [RelayCommand]
        public async Task Skip(System.Collections.IList selectedItems)
        {
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = _commandStack.Push<ApplySkipCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.Id)]));
                command.Commit();
            }
        }
    }
}