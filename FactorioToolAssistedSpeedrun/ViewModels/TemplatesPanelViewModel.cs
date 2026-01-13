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
    public partial class TemplatePanelViewModel : ObservableObject
    {
        private readonly CommandStack _commandStack;
        private readonly StartupService _startupService;
        private readonly StepService _stepService;
        private readonly PanelService _panelService;
        public PanelService PanelService => _panelService;
        public StartupService StartupService => _startupService;

        public TemplatePanelViewModel()
        {
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _stepService = App.Current.Services.GetRequiredService<StepService>();
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public TemplatePanelViewModel(CommandStack commandStack, StartupService startupService, StepService stepService, PanelService panelService)
        {
            _commandStack = commandStack;
            _startupService = startupService;
            _stepService = stepService;
            _panelService = panelService;
        }

        [ObservableProperty]
        private string? _inputTemplate;

        [RelayCommand]
        public async Task MouseRightButtonUp(DataGridRow row)
        {
            var index = row.GetIndex();
            var step = _panelService.StepCollection[index];
            _stepService.FromStep(step);
        }

        [RelayCommand]
        public async Task Load()
        {
            _panelService.LoadTemplateSteps();
        }

        [RelayCommand]
        public async Task New()
        {
            if (string.IsNullOrWhiteSpace(InputTemplate))
            {
                MessageBox.Show("Template name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_panelService.TemplateCollection.Contains(InputTemplate))
            {
                MessageBox.Show("Template with the same name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _panelService.TemplateCollection.Add(InputTemplate);
            _panelService.SelectedTemplate = InputTemplate;
        }

        [RelayCommand]
        public async Task Add(bool rightClick)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var step = _stepService.ToStep();
            var index = rightClick ? _panelService.SelectedTemplateStepIndex + 1 : _panelService.SelectedTemplateStepIndex;

            step.Location = index + 1;
            step.Name = _panelService.SelectedTemplate;

            var command = new AddStepCommand
            {
                Name = _panelService.SelectedTemplate,
                Steps = [step],
            };
            command.Commit();
            _panelService.SelectedTemplateStepIndex = index;
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task Remove(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure want to delete these steps?", "Warning", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new DeleteStepCommand
            {
                Name = _panelService.SelectedTemplate,
                Steps = [.. items.Select(x => x.ToEntity())],
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpOne(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var items = selectedItems.OfType<StepModel>().ToList();

            var command = new MoveStepCommand
            {
                Name = _panelService.SelectedTemplate,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -1,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveUpFive(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Name = _panelService.SelectedTemplate,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = -5,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownOne(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Name = _panelService.SelectedTemplate,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 1,
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task MoveDownFive(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var items = selectedItems.OfType<StepModel>().ToList();
            var command = new MoveStepCommand
            {
                Name = _panelService.SelectedTemplate,
                StepIds = [.. items.Select(x => x.Id)],
                MoveOffset = 5,
            };
            command.Commit();
            _commandStack.Push(command);
        }
    }
}