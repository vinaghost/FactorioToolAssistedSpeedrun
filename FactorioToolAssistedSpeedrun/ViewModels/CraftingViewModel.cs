using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class CraftingViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly IStartupService _startupService;
        private readonly ICommandStack _commandStack;
        public PanelService PanelService => _panelService;
        public IStartupService startupService => _startupService;

        public CraftingViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
            _startupService = App.Current.Services.GetRequiredService<IStartupService>();
            _commandStack = App.Current.Services.GetRequiredService<ICommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public CraftingViewModel(PanelService panelService, IStartupService startupService, ICommandStack commandStack)
        {
            _panelService = panelService;
            _startupService = startupService;
            _commandStack = commandStack;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await Task.Run(_panelService.LoadCraft);
        }

        [RelayCommand]
        public async Task Remove(System.Collections.IList selectedItems)
        {
            var result = MessageBox.Show("Remember refresh before deleting!!! Are you sure want to delete these steps?", "Warning", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var items = selectedItems.OfType<CraftingModel>().ToList();

            for (int i = 1; i < items.Count; i++)
            {
                if (items[i].Location != items[i - 1].Location + 1)
                {
                    MessageBox.Show("Selected steps do not have consecutive locations.", "Error", MessageBoxButton.OK);
                    return;
                }
            }

            var command = _commandStack.Push<DeleteStepCommand>();
            if (command is not null)
            {
                command.Setup(new("", [.. items.Select(x => x.ToEntity())]));
                command.Commit();
                await Refresh();
            }
        }
    }
}