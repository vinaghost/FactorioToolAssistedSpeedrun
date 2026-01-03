using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepDetailPanelViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _x;

        [ObservableProperty]
        private bool _xEnabled;

        [ObservableProperty]
        private double _y;

        [ObservableProperty]
        private bool _yEnabled;

        [ObservableProperty]
        private int _amount;

        [ObservableProperty]
        private bool _amountEnabled;

        [ObservableProperty]
        private string _comment = "";

        [ObservableProperty]
        private string _selectedItem = "";

        [ObservableProperty]
        private bool _itemEnabled;

        public ObservableCollection<KeyValuePair<string, string>> Items { get; } = [];

        [ObservableProperty]
        private InventoryType? _inventory;

        public ObservableCollection<InventoryType> Inventories { get; } = [.. Enum.GetValues<InventoryType>()];

        [ObservableProperty]
        private bool _inventoryEnabled;

        [ObservableProperty]
        private PriorityType _inputPriority;

        [ObservableProperty]
        private bool _inputPriorityEnabled;

        [ObservableProperty]
        private PriorityType _outputPriority;

        [ObservableProperty]
        private bool _outputPriorityEnabled;

        [ObservableProperty]
        private OrientationType _orientation = OrientationType.North;

        public ObservableCollection<OrientationType> Orientations { get; } = [.. Enum.GetValues<OrientationType>()];

        [ObservableProperty]
        private bool _orientationEnabled;

        [ObservableProperty]
        private ModifierType? _modifier;

        [ObservableProperty]
        private bool _takeAllEnabled;

        [ObservableProperty]
        private bool _mineSplitEnabled;

        [ObservableProperty]
        private bool _walkTowardsEnabled;

        public void Load(StepType stepType)
        {
            Enable(stepType);
            LoadItem(stepType);
            Modifier = null;
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
            var items = new List<KeyValuePair<string, string>>();
            switch (stepType)
            {
                case StepType.Build:
                    {
                        var buildings = App.Current.GameData!.Items.Where(x => x.Value.IsBuilable).Select(x => x.Key).ToList();
                        items.AddRange(App.Current.GameData!.ItemsLocale.Where(x => buildings.Contains(x.Key)).OrderBy(x => x.Key).Select(x => KeyValuePair.Create(x.Key, x.Value)));
                        break;
                    }

                case StepType.Craft:
                case StepType.Filter:
                case StepType.Put:
                case StepType.Take:
                case StepType.Drop:
                case StepType.CancelCrafting:
                case StepType.Equip:
                case StepType.Throw:
                    items.AddRange(App.Current.GameData!.ItemsLocale.OrderBy(x => x.Key).Select(x => KeyValuePair.Create(x.Key, x.Value)));
                    break;

                case StepType.Recipe:
                    items.AddRange(App.Current.GameData!.RecipesLocale.OrderBy(x => x.Key).Select(x => KeyValuePair.Create(x.Key, x.Value)));
                    break;

                case StepType.Tech:
                    items.AddRange(App.Current.GameData!.TechnologiesLocale.OrderBy(x => x.Key).Select(x => KeyValuePair.Create(x.Key, x.Value)));
                    break;

                default:
                    break;
            }

            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }
    }
}