using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class TemplatesTabViewModel : ObservableObject
    {
        private readonly StepTypePanelViewModel _stepTypePanelViewModel;
        private readonly StepDetailPanelViewModel _stepDetailPanelViewModel;
        private readonly CommandStack _commandStack;
        private readonly StartupService _startupService;
        private readonly StepService _stepService;

        public TemplatesTabViewModel()
        {
            _stepDetailPanelViewModel = App.Current.Services.GetRequiredService<StepDetailPanelViewModel>();
            _stepTypePanelViewModel = App.Current.Services.GetRequiredService<StepTypePanelViewModel>();
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _stepService = App.Current.Services.GetRequiredService<StepService>();
        }

        [ActivatorUtilitiesConstructor]
        public TemplatesTabViewModel(StepTypePanelViewModel stepTypePanelViewModel, StepDetailPanelViewModel stepDetailPanelViewModel, CommandStack commandStack, StartupService startupService, StepService stepService)
        {
            _stepTypePanelViewModel = stepTypePanelViewModel;
            _stepDetailPanelViewModel = stepDetailPanelViewModel;
            _commandStack = commandStack;
            _startupService = startupService;
            _stepService = stepService;

            _startupService.OnProjectDataLoaded += OnProjectDataLoaded;
            _startupService.OnGameDataLoaded += OnGameDataLoaded;
        }

        private void OnGameDataLoaded()
        {
            ItemCollection.Clear();

            ItemCollection.AddRange(_startupService.GameData!.Items.Select(x => x.Key));
            ItemCollection.AddRange(_startupService.GameData!.Recipes.Select(x => x.Key));
            ItemCollection.AddRange(_startupService.GameData!.Technologies.Select(x => x.Key));
        }

        private void OnProjectDataLoaded()
        {
        }

        [ObservableProperty]
        private StepModel? _selectedItem;

        [ObservableProperty]
        private string? _selectedTemplate;

        [ObservableProperty]
        private string? _inputTemplate;

        [ObservableProperty]
        private int _selectedIndex;

        partial void OnSelectedTemplateChanged(string? value)
        {
            StepCollection.Clear();

            if (!_startupService.IsProjectDataLoaded)
                return;

            if (string.IsNullOrEmpty(value))
                return;

            using var context = new ProjectDbContext(_startupService.ProjectDataFile!);
            foreach (var step in context.Steps.Where(x => x.Name == value).OrderBy(x => x.Location))
            {
                StepModel model = new() { Collection = StepCollection };
                model.FromEntity(step);
                StepCollection.Add(model);
            }
        }

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];
        public ObservableCollection<string> TemplateCollection { get; set; } = [];
        public List<string> ItemCollection { get; set; } = [];

        [RelayCommand]
        public async Task MouseRightButtonUp(DataGridRow row)
        {
            var index = row.GetIndex();
            var step = StepCollection[index];
            _stepService.FromStep(step);
        }

        [RelayCommand]
        public async Task Load()
        {
            if (!_startupService.IsProjectDataLoaded)
                return;

            using var context = new ProjectDbContext(_startupService.ProjectDataFile!);
            TemplateCollection.Clear();
            foreach (var template in context.Steps.Select(x => x.Name).Distinct().OrderBy(x => x))
            {
                TemplateCollection.Add(template);
            }
            if (TemplateCollection.Count > 0)
                SelectedTemplate = TemplateCollection[0];
        }

        [RelayCommand]
        public async Task New()
        {
            if (string.IsNullOrWhiteSpace(InputTemplate))
            {
                MessageBox.Show("Template name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TemplateCollection.Contains(InputTemplate))
            {
                MessageBox.Show("Template with the same name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TemplateCollection.Add(InputTemplate);
            SelectedTemplate = InputTemplate;
            StepCollection.Clear();
        }

        [RelayCommand]
        public async Task Add(bool rightClick)
        {
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var step = _stepService.ToStep();
            var index = rightClick ? SelectedIndex + 1 : SelectedIndex;
            step.Location = index + 1;
            var command = new AddStepCommand
            {
                Collection = StepCollection,
                Steps = [step],
            };
            command.Commit();
            SelectedIndex = index;
            _commandStack.Push(command);
        }

        [RelayCommand]
        public async Task Delete(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            if (string.IsNullOrEmpty(SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
    }
}