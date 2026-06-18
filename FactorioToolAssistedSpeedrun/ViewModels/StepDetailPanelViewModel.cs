using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepDetailPanelViewModel : ObservableObject
    {
        private readonly StepService _stepService;
        private readonly IDataService _dataService;

        public StepService StepService => _stepService;

        public StepDetailPanelViewModel()
        {
            _stepService = Ioc.Default.GetRequiredService<StepService>();
            _dataService = Ioc.Default.GetRequiredService<IDataService>();
        }

        [ActivatorUtilitiesConstructor]
        public StepDetailPanelViewModel(StepService stepService, IDataService dataService)
        {
            _stepService = stepService;
            _dataService = dataService;
            _dataService.OnGameDataLoaded += LoadItemData;
            _stepService.TypeChanged += LoadDetail;
        }

        private readonly List<string> _buildableItems = [];
        private readonly List<string> _craftableItems = [];
        private readonly List<string> _equipableItems = [];
        private readonly List<string> _throwableItems = [];
        private readonly List<string> _recipes = [];
        private readonly List<string> _technologies = [];

        private void LoadItemData()
        {
            _buildableItems.Clear();
            _buildableItems.AddRange(_dataService.GameData.Items
                                        .Where(x => x.Value.IsBuilable)
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
            _craftableItems.Clear();
            _craftableItems.AddRange(_dataService.GameData.Items
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));

            _equipableItems.Clear();
            _equipableItems.AddRange(_dataService.GameData.Items
                                        .Where(x => !string.IsNullOrEmpty(x.Value.Type) &&
                                                    (x.Value.Type.StartsWith("armor") ||
                                                    x.Value.Type.StartsWith("gun") ||
                                                    x.Value.Type.StartsWith("ammo")))
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));

            _throwableItems.Clear();
            _throwableItems.AddRange(_dataService.GameData.Items
                                        .Where(x => !string.IsNullOrEmpty(x.Value.Type) &&
                                                    x.Value.Type.StartsWith("capsule"))
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));

            _recipes.Clear();
            _recipes.AddRange(_dataService.GameData.Recipes
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));

            _technologies.Clear();
            _technologies.AddRange(_dataService.GameData.Technologies
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
        }

        private void LoadDetail(StepType stepType)
        {
            Enable(stepType);
            LoadItem(stepType);
        }

        private void Enable(StepType stepType)
        {
            XEnabled = stepType.ContainFlag(ParameterFlag.Point);
            YEnabled = stepType.ContainFlag(ParameterFlag.Point);
            AmountEnabled = stepType.ContainFlag(ParameterFlag.Amount);
            ItemEnabled = stepType.ContainFlag(ParameterFlag.Item);
            InventoryEnabled = stepType.ContainFlag(ParameterFlag.Inventory);
            InputPriorityEnabled = stepType.ContainFlag(ParameterFlag.Priority);
            OutputPriorityEnabled = stepType.ContainFlag(ParameterFlag.Priority);
            OrientationEnabled = stepType.ContainFlag(ParameterFlag.Orientation);

            TakeAllEnabled = stepType == StepType.Take;
            MineSplitEnabled = stepType == StepType.Mine;
            WalkTowardsEnabled = stepType == StepType.Walk;
        }

        private void LoadItem(StepType stepType)
        {
            Items.Clear();
            switch (stepType)
            {
                case StepType.Build:
                    foreach (var item in _buildableItems)
                    {
                        Items.Add(item);
                    }
                    break;

                case StepType.Craft:
                case StepType.Filter:
                case StepType.Put:
                case StepType.Take:
                case StepType.Drop:
                case StepType.CancelCrafting:
                    foreach (var item in _craftableItems)
                    {
                        Items.Add(item);
                    }
                    if (string.IsNullOrEmpty(StepService.SelectedItem))
                    {
                        StepService.SelectedItem = Items.FirstOrDefault()!;
                    }
                    break;

                case StepType.Equip:
                    foreach (var item in _equipableItems)
                    {
                        Items.Add(item);
                    }
                    if (string.IsNullOrEmpty(StepService.SelectedItem))
                    {
                        StepService.SelectedItem = Items.FirstOrDefault()!;
                    }
                    break;

                case StepType.Throw:
                    foreach (var item in _throwableItems)
                    {
                        Items.Add(item);
                    }
                    if (string.IsNullOrEmpty(StepService.SelectedItem))
                    {
                        StepService.SelectedItem = Items.FirstOrDefault()!;
                    }
                    break;

                case StepType.Recipe:
                    foreach (var item in _recipes)
                    {
                        Items.Add(item);
                    }
                    if (string.IsNullOrEmpty(StepService.SelectedItem))
                    {
                        StepService.SelectedItem = Items.FirstOrDefault()!;
                    }
                    break;

                case StepType.Tech:
                    foreach (var item in _technologies)
                    {
                        Items.Add(item);
                    }
                    if (string.IsNullOrEmpty(StepService.SelectedItem))
                    {
                        StepService.SelectedItem = Items.FirstOrDefault()!;
                    }
                    break;

                default:
                    break;
            }
        }

        [ObservableProperty]
        private bool _xEnabled;

        [ObservableProperty]
        private bool _yEnabled;

        [ObservableProperty]
        private bool _amountEnabled;

        [ObservableProperty]
        private bool _itemEnabled;

        [ObservableProperty]
        private bool _inventoryEnabled;

        [ObservableProperty]
        private bool _inputPriorityEnabled;

        [ObservableProperty]
        private bool _outputPriorityEnabled;

        [ObservableProperty]
        private bool _orientationEnabled;

        [ObservableProperty]
        private bool _takeAllEnabled;

        [ObservableProperty]
        private bool _mineSplitEnabled;

        [ObservableProperty]
        private bool _walkTowardsEnabled;

        public ObservableCollection<string> Items { get; } = [];
        public ObservableCollection<InventoryType> Inventories { get; } = [.. Enum.GetValues<InventoryType>()];

        public ObservableCollection<OrientationType> Orientations { get; } = [.. Enum.GetValues<OrientationType>()];
    }
}