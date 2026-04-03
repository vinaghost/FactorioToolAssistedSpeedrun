using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly LoadingService _loadingService;
        private readonly PanelService _panelService;
        public LoadingService LoadingService => _loadingService;

        private readonly ICommandStack _commandStack;

        public MainViewModel()
        {
            _dataService = Ioc.Default.GetRequiredService<IDataService>();
            _commandStack = Ioc.Default.GetRequiredService<ICommandStack>();
            _loadingService = Ioc.Default.GetRequiredService<LoadingService>();
            _panelService = Ioc.Default.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(ICommandStack commandStack, IDataService dataService, LoadingService loadingService, PanelService panelService)
        {
            _commandStack = commandStack;
            _dataService = dataService;
            _loadingService = loadingService;
            _panelService = panelService;

            _dataService.OnProjectDataLoaded += OnProjectDataLoaded;
        }

        [ObservableProperty]
        private string _projectName = "Not loaded";

        private void OnProjectDataLoaded()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ProjectName = _dataService.ProjectName;
            });
        }

        [RelayCommand]
        private async Task LoadSettings()
        {
            LoadingService.Show();

            try
            {
                await Task.Run(_dataService.LoadGameDataFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                await Task.Run(_dataService.LoadProjectDataFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadingService.Hide();
        }

        [RelayCommand]
        private static void OpenImportStringWindow()
        {
            var existingWindow = Application.Current.Windows.OfType<ImportStringWindow>().FirstOrDefault();
            if (existingWindow is not null)
            {
                existingWindow.Activate();
                if (existingWindow.WindowState == WindowState.Minimized)
                {
                    existingWindow.WindowState = WindowState.Normal;
                }
                return;
            }

            var importStringWindow = new ImportStringWindow
            {
                Owner = Application.Current.MainWindow
            };
            importStringWindow.Show();
        }

        [RelayCommand]
        private static void OpenCraftingWindow()
        {
            var existingWindow = Application.Current.Windows.OfType<CraftingWindow>().FirstOrDefault();
            if (existingWindow is not null)
            {
                existingWindow.Activate();
                if (existingWindow.WindowState == WindowState.Minimized)
                {
                    existingWindow.WindowState = WindowState.Normal;
                }
                return;
            }

            var craftingWindow = new CraftingWindow
            {
                Owner = Application.Current.MainWindow
            };
            craftingWindow.Show();
        }

        [RelayCommand]
        private void ToTemplatePanel(System.Collections.IList selectedItems)
        {
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one step.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var items = selectedItems.OfType<StepModel>().Select(x => x.ToEntity()).OrderBy(x => x.Location).ToList();
            var index = _panelService.SelectedTemplateStepIndex + 1;
            if (index == 0) index = 1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.Id = Guid.NewGuid();
                item.Location = index + i;
                item.Name = _panelService.SelectedTemplate;
            }

            var command = _commandStack.Push<AddStepCommand>();
            if (command is not null)
            {
                command.Setup(new(_panelService.SelectedTemplate, items));
                command.Commit();
            }
        }

        [RelayCommand]
        private void ToStepPanel(System.Collections.IList selectedItems)
        {
            if (string.IsNullOrEmpty(_panelService.SelectedTemplate))
            {
                MessageBox.Show("Please select a template first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<Step> items;
            if (selectedItems.Count == 0)
            {
                items = [.. _panelService.TemplateStepCollection.Select(x => x.ToEntity()).OrderBy(x => x.Location)];
            }
            else
            {
                items = [.. selectedItems.OfType<StepModel>().Select(x => x.ToEntity()).OrderBy(x => x.Location)];
            }
            _panelService.ApplyTemplateModifier(items);
            var index = _panelService.SelectedStepIndex + 1;
            if (index == 0) index = 1;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.Id = Guid.NewGuid();
                item.Location = index + i;
                item.Name = "";
            }

            var command = _commandStack.Push<AddStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", items));
                command.Commit();
            }
        }

        [RelayCommand]
        private void SortTopRight(System.Collections.IList selectedItems)
        {
            Sort(selectedItems, SortDirectionType.TopRight);
        }

        [RelayCommand]
        private void SortTopLeft(System.Collections.IList selectedItems)
        {
            Sort(selectedItems, SortDirectionType.TopLeft);
        }

        [RelayCommand]
        private void SortBottomRight(System.Collections.IList selectedItems)
        {
            Sort(selectedItems, SortDirectionType.BottomRight);
        }

        [RelayCommand]
        private void SortBottomLeft(System.Collections.IList selectedItems)
        {
            Sort(selectedItems, SortDirectionType.BottomLeft);
        }

        public void Sort(System.Collections.IList selectedItems, SortDirectionType direction)
        {
            if (selectedItems.Count < 3)
            {
                MessageBox.Show("Please select at least three steps to sort.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var answer = MessageBox.Show("Sort by X coordinate first?", "Sort Steps", MessageBoxButton.YesNo, MessageBoxImage.Question);
            var firstType = answer == MessageBoxResult.Yes ? SortFirstType.X : SortFirstType.Y;

            var items = selectedItems.OfType<StepModel>().Select(x => x.ToEntity()).OrderBy(x => x.Location).ToList();
            var command = _commandStack.Push<SortStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", items, direction, firstType));
                command.Commit();
            }
        }
    }
}