using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.UI;
using System.Collections.ObjectModel;

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

        public ObservableCollection<string> Items { get; } = [];

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

        public void Load(StepModel step)
        {
            if (step.Type.ContainFlag(ParameterFlag.Point))
            {
                X = double.Parse(step.X);
                Y = double.Parse(step.Y);
            }
            if (step.Type.ContainFlag(ParameterFlag.Amount))
            {
                if (step.Amount == "All")
                {
                    Amount = 0;
                }
                else
                {
                    Amount = int.Parse(step.Amount);
                }
            }
            if (step.Type.ContainFlag(ParameterFlag.Item))
            {
                SelectedItem = step.Item;
            }

            if (step.Type.ContainFlag(ParameterFlag.Inventory))
            {
                Inventory = InventoryTypeExtensions.FromString(step.Orientation);
            }
            if (step.Type.ContainFlag(ParameterFlag.Priority))
            {
                var priority = Priority.FromString(step.Orientation);
                InputPriority = priority!.In;
                OutputPriority = priority!.Out;
            }
            if (step.Type.ContainFlag(ParameterFlag.Orientation))
            {
                Orientation = OrientationTypeExtensions.FromString(step.Orientation) ?? OrientationType.North;
            }
            if (step.Type.ContainFlag(ParameterFlag.Modifier) && !string.IsNullOrEmpty(step.Modifier))
            {
                Modifier = ModifierTypeExtensions.FromString(step.Modifier);
            }

            Comment = step.Comment;
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
            var items = new List<string>();
            switch (stepType)
            {
                case StepType.Build:
                    items.AddRange(App.Current.GameData!.Items
                                        .Where(x => x.Value.IsBuilable)
                                        .Select(x => x.Key));
                    break;

                case StepType.Craft:
                case StepType.Filter:
                case StepType.Put:
                case StepType.Take:
                case StepType.Drop:
                case StepType.CancelCrafting:
                    items.AddRange(App.Current.GameData!.Items
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
                    break;

                case StepType.Equip:
                    items.AddRange(App.Current.GameData!.Items
                                        .Where(x => !string.IsNullOrEmpty(x.Value.Type) &&
                                                    (x.Value.Type.StartsWith("armor") ||
                                                    x.Value.Type.StartsWith("gun") ||
                                                    x.Value.Type.StartsWith("ammo")))
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
                    break;

                case StepType.Throw:
                    items.AddRange(App.Current.GameData!.Items
                                        .Where(x => !string.IsNullOrEmpty(x.Value.Type) &&
                                                    x.Value.Type.StartsWith("capsule"))
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
                    break;

                case StepType.Recipe:
                    items.AddRange(App.Current.GameData!.Recipes
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
                    break;

                case StepType.Tech:
                    items.AddRange(App.Current.GameData!.Technologies
                                        .OrderBy(x => x.Key)
                                        .Select(x => x.Key));
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