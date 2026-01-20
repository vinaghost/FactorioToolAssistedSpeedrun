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
            _panelService.AddTemplate(InputTemplate);
            MessageBox.Show("Template created successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        public async Task Delete()
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var result = MessageBox.Show($"Are you sure want to delete this template [{_panelService.SelectedTemplate}]?", "Warning", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            _panelService.RemoveTemplate(_panelService.SelectedTemplate);
            MessageBox.Show("Template deleted successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var command = _commandStack.Push<AddStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [step]));
                command.Commit();
                _panelService.SelectedTemplateStepIndex = index;
            }
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

            var command = _commandStack.Push<DeleteStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [.. items.Select(x => x.ToEntity())]));
                command.Commit();
            }
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

            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [.. items.Select(x => x.Id)], -1));
                command.Commit();
            }
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

            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [.. items.Select(x => x.Id)], -5));
                command.Commit();
            }
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
            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [.. items.Select(x => x.Id)], 1));
                command.Commit();
            }
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
            var command = _commandStack.Push<MoveStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, [.. items.Select(x => x.Id)], 5));
                command.Commit();
            }
        }
    }
}