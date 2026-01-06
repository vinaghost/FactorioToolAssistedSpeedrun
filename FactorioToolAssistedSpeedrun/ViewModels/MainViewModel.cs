using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.UI;
using FactorioToolAssistedSpeedrun.Models.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public LoadingViewModel LoadingViewModel { get; }

        public MenuBarViewModel MenuBarViewModel { get; }
        public StepDetailPanelViewModel StepDetailPanelViewModel { get; }

        public StepTypePanelViewModel StepTypePanelViewModel { get; }
        public StepPanelViewModel StepPanelViewModel { get; }

        public ImportTabViewModel ImportTabViewModel { get; }
        public TemplatesTabViewModel TemplatesTabViewModel { get; }

        public MainViewModel()
        {
            LoadingViewModel = new LoadingViewModel();
            MenuBarViewModel = new MenuBarViewModel();
            StepTypePanelViewModel = new StepTypePanelViewModel();
            StepDetailPanelViewModel = new StepDetailPanelViewModel();
            StepPanelViewModel = new StepPanelViewModel();
            ImportTabViewModel = new ImportTabViewModel();
            TemplatesTabViewModel = new TemplatesTabViewModel();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(LoadingViewModel loadingViewModel, MenuBarViewModel menuBarViewModel, StepTypePanelViewModel stepTypePanelViewModel, StepDetailPanelViewModel stepDetailPanelViewModel, StepPanelViewModel stepPanelViewModel, ImportTabViewModel importTabViewModel, TemplatesTabViewModel templatesTabViewModel)
        {
            LoadingViewModel = loadingViewModel;
            MenuBarViewModel = menuBarViewModel;
            StepTypePanelViewModel = stepTypePanelViewModel;
            StepDetailPanelViewModel = stepDetailPanelViewModel;
            StepPanelViewModel = stepPanelViewModel;
            ImportTabViewModel = importTabViewModel;
            TemplatesTabViewModel = templatesTabViewModel;
        }

        [RelayCommand]
        private async Task GoToLine()
        {
            var dialogViewModel = App.Current.Services.GetRequiredService<DialogViewModel>();
            dialogViewModel.MinLine = 1;
            dialogViewModel.MaxLine = StepPanelViewModel.StepCollection.Count;
            var dialog = new Views.Dialog
            {
                DataContext = dialogViewModel,
                Owner = Application.Current.MainWindow
            };
            if (dialog.ShowDialog() == true)
            {
                var line = dialogViewModel.Line;
                if (line > 0 && line < StepPanelViewModel.StepCollection.Count - 1)
                {
                    var center = Math.Min(StepPanelViewModel.StepCollection.Count - 1, line + 20);

                    StepPanelViewModel.SelectedItem = StepPanelViewModel.StepCollection[center];
                    StepPanelViewModel.ScrollToSelected?.Invoke();
                    if (center != line - 1)
                        StepPanelViewModel.SelectedItem = StepPanelViewModel.StepCollection[line - 1];
                }
            }
        }

        [RelayCommand]
        private async Task LoadSettings()
        {
            LoadingViewModel.IsShown = true;

            try
            {
                LoadGameDataFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                await LoadProjectDataFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadingViewModel.IsShown = false;
        }

        private void LoadGameDataFile()
        {
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!File.Exists(gameDataFile))
                return;

            MenuBarViewModel.Version = Path.GetFileNameWithoutExtension(gameDataFile);

            var fileContent = File.ReadAllText(gameDataFile);
            App.Current.GameData = JsonSerializer.Deserialize<GameData>(fileContent);

            StepPanelViewModel.LoadItems(App.Current.GameData!);
        }

        private async Task LoadProjectDataFile()
        {
            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!File.Exists(projectDataFile))
                return;

            App.Current.ProjectDataFile = projectDataFile;
            MenuBarViewModel.ProjectName = Path.GetFileNameWithoutExtension(projectDataFile);

            var loadSettingsCommand = new LoadSettingsCommand
            {
                ProjectDataFile = projectDataFile
            };

            await Task.Run(loadSettingsCommand.Execute);

            MenuBarViewModel.UpdateSetting(loadSettingsCommand.Result);

            var loadStepsCommand = new LoadStepsCommand
            {
                ProjectDataFile = projectDataFile
            };
            await Task.Run(loadStepsCommand.Execute);

            StepPanelViewModel.LoadSteps(loadStepsCommand.Result);
        }
    }
}