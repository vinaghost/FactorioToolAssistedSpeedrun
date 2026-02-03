using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class CraftingViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly IDataService _dataService;
        private readonly ICommandStack _commandStack;
        public PanelService PanelService => _panelService;

        public ObservableCollection<string> ItemsCollection { get; }

        public CraftingViewModel()
        {
            _panelService = Ioc.Default.GetRequiredService<PanelService>();
            _dataService = Ioc.Default.GetRequiredService<IDataService>();
            _commandStack = Ioc.Default.GetRequiredService<ICommandStack>();

            ItemsCollection = _dataService.ItemsCollection;
        }

        [ActivatorUtilitiesConstructor]
        public CraftingViewModel(PanelService panelService, IDataService dataService, ICommandStack commandStack)
        {
            _panelService = panelService;
            _dataService = dataService;
            _commandStack = commandStack;

            ItemsCollection = _dataService.ItemsCollection;
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