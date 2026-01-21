using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly StartupService _startupService;
        private readonly LoadingService _loadingService;
        private readonly PanelService _panelService;
        public StartupService StartupService => _startupService;
        public LoadingService LoadingService => _loadingService;

        private readonly CommandStack _commandStack;

        public MainViewModel()
        {
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _loadingService = App.Current.Services.GetRequiredService<LoadingService>();
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(CommandStack commandStack, StartupService startupService, LoadingService loadingService, PanelService panelService)
        {
            _commandStack = commandStack;
            _startupService = startupService;
            _loadingService = loadingService;
            _panelService = panelService;
        }

        [RelayCommand]
        private async Task LoadSettings()
        {
            LoadingService.Show();

            try
            {
                await Task.Run(_startupService.LoadGameDataFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                await Task.Run(_startupService.LoadProjectDataFile);
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
            var importStringWindow = new Views.ImportStringWindow
            {
                Owner = Application.Current.MainWindow
            };
            importStringWindow.Show();
        }

        [RelayCommand]
        private static void OpenCraftingWindow()
        {
            var craftingWindow = new Views.CraftingWindow
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
    }
}