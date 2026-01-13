using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private readonly DialogViewModel _dialogViewModel;

        public MainViewModel()
        {
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            _dialogViewModel = App.Current.Services.GetRequiredService<DialogViewModel>();
            _loadingService = App.Current.Services.GetRequiredService<LoadingService>();
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(CommandStack commandStack, DialogViewModel dialogViewModel, StartupService startupService, LoadingService loadingService, PanelService panelService)
        {
            _commandStack = commandStack;
            _dialogViewModel = dialogViewModel;
            _startupService = startupService;
            _loadingService = loadingService;
            _panelService = panelService;
        }

        [RelayCommand]
        private async Task GoToLine(object dataContext)
        {
            if (dataContext is not StepPanelViewModel StepPanelViewModel) return;

            _dialogViewModel.MinLine = 1;
            _dialogViewModel.MaxLine = _panelService.StepCollection.Count;

            var dialog = new Views.Dialog
            {
                DataContext = _dialogViewModel,
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true)
                return;
            var line = _dialogViewModel.Line;
            _panelService.ScrollTo(line);
        }

        [RelayCommand]
        private async Task Undo()
        {
            if (!_commandStack.CanUndo) return;
            var command = _commandStack.UndoPop();
            command.Rollback();
        }

        [RelayCommand]
        private async Task Redo()
        {
            if (!_commandStack.CanRedo) return;
            var command = _commandStack.RedoPop();
            command.Commit();
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
    }
}