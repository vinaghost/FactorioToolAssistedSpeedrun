using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Services;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class StepModel : ObservableObject
    {
        private readonly ICommandStack _commandStack = Ioc.Default.GetRequiredService<ICommandStack>();
        private readonly IDataService _dataService = Ioc.Default.GetRequiredService<IDataService>();

        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = "";
        private bool _loaded = false;
        private bool _lock = false;

        [ObservableProperty]
        public partial int Location { get; set; }

        [ObservableProperty]
        public partial StepType Type { get; set; }

        [ObservableProperty]
        public partial string X { get; set; } = "";

        partial void OnXChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;
            if (!Type.ContainFlag(ParameterFlag.Point))
            {
                X = "";
                return;
            }

            if (!double.TryParse(newValue, out _))
            {
                X = oldValue;
                return;
            }

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, double>>();
            command?.Setup(new(Name, Id, oldValue, newValue,
                str => double.Parse(str),
                model => model.X,
                step => step.X));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial string Y { get; set; } = "";

        partial void OnYChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;
            if (!Type.ContainFlag(ParameterFlag.Point))
            {
                Y = "";
                return;
            }

            if (!double.TryParse(newValue, out _))
            {
                Y = oldValue;
                return;
            }

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, double>>();
            command?.Setup(new(Name, Id, oldValue, newValue,
                str => double.Parse(str),
                model => model.Y,
                step => step.Y));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        private string _amount = "";

        partial void OnAmountChanged(string? oldValue, string newValue)
        {
            if (!_loaded || _lock) return;
            if (newValue == "All") newValue = "0";
            if (!Type.ContainFlag(ParameterFlag.Amount))
            {
                Amount = "";
                return;
            }

            if (!double.TryParse(newValue, out var value))
            {
                Amount = oldValue ?? "";
                return;
            }

            if (value == 0)
            {
                Amount = "All";
                return;
            }

            if (value < 0)
            {
                Amount = oldValue ?? "";
                return;
            }

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, int>>();
            command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                str => int.Parse(str),
                model => model.Amount,
                step => step.Amount));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial string Item { get; set; } = "";

        partial void OnItemChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;
            if (!Type.ContainFlag(ParameterFlag.Item))
            {
                Item = "";
                return;
            }

            var isValidItem = Type == StepType.Tech
                ? _dataService.GameData.Technologies.ContainsKey(newValue)
                : Type == StepType.Recipe
                    ? _dataService.GameData.Recipes.ContainsKey(newValue)
                    : _dataService.GameData.Items.ContainsKey(newValue);
            if (!isValidItem)
            {
                Item = oldValue ?? "";
                return;
            }

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
            command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                str => str,
                model => model.Item,
                step => step.Item));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial string Orientation { get; set; } = "";

        partial void OnOrientationChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;

            if (Type.ContainFlag(ParameterFlag.Orientation))
            {
                if (!OrientationTypeExtensions.TryGetValue(newValue, out _))
                {
                    Orientation = oldValue ?? "";
                    return;
                }

                _lock = true;

                var command = _commandStack.Push<UpdateStepPropertyCommand<string, OrientationType?>>();
                command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                    str => OrientationTypeExtensions.FromString(str),
                    model => model.Orientation,
                    step => step.Orientation));
                command?.Commit(true);

                _lock = false;
                return;
            }

            if (Type.ContainFlag(ParameterFlag.Inventory))
            {
                if (!InventoryTypeExtensions.TryGetValue(newValue, out _))
                {
                    Orientation = oldValue ?? "";
                    return;
                }

                _lock = true;

                var command = _commandStack.Push<UpdateStepPropertyCommand<string, InventoryType?>>();
                command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                    str => InventoryTypeExtensions.FromString(str),
                    model => model.Inventory,
                    step => step.Orientation));
                command?.Commit(true);

                _lock = false;
                return;
            }

            if (Type.ContainFlag(ParameterFlag.Priority))
            {
                if (Priority.FromString(newValue) is null)
                {
                    Orientation = oldValue ?? "";
                    return;
                }

                _lock = true;

                var command = _commandStack.Push<UpdateStepPropertyCommand<string, Priority?>>();
                command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                    str => Priority.FromString(str),
                    model => model.Priority,
                    step => step.Orientation));
                command?.Commit(true);

                _lock = false;
                return;
            }

            Orientation = "";
        }

        [ObservableProperty]
        public partial string Modifier { get; set; } = "";

        partial void OnModifierChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;
            if (string.IsNullOrEmpty(newValue)) return;
            if (!Type.ContainFlag(ParameterFlag.Modifier))
            {
                Modifier = "";
                return;
            }

            if (!ModifierTypeExtensions.TryGetValue(newValue, out var value))
            {
                Modifier = oldValue ?? "";
                return;
            }

            if (Type == StepType.Mine && value != ModifierType.Split)
            {
                Modifier = oldValue ?? "";
                return;
            }

            if (Type == StepType.Take && value != ModifierType.All)
            {
                Modifier = oldValue ?? "";
                return;
            }

            if (Type == StepType.Wait && value != ModifierType.WalkTowards)
            {
                Modifier = oldValue ?? "";
                return;
            }

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, ModifierType?>>();
            command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                str => ModifierTypeExtensions.FromString(str),
                model => model.Modifier,
                step => step.Modifier));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial string Color { get; set; } = "";

        partial void OnColorChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
            command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                str => str,
                model => model.Color,
                step => step.Color));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial string Comment { get; set; } = "";

        partial void OnCommentChanged(string oldValue, string newValue)
        {
            if (!_loaded || _lock) return;

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
            command?.Setup(new(Name, Id, oldValue ?? "", newValue,
                str => str,
                model => model.Comment,
                step => step.Comment));
            command?.Commit(true);

            _lock = false;
        }

        [ObservableProperty]
        public partial bool IsSkip { get; set; }

        partial void OnIsSkipChanged(bool oldValue, bool newValue)
        {
            if (!_loaded || _lock) return;

            _lock = true;

            var command = _commandStack.Push<UpdateStepPropertyCommand<bool, bool>>();
            command?.Setup(new(Name, Id, oldValue, newValue,
                str => str,
                model => model.IsSkip,
                step => step.IsSkip));
            command?.Commit(true);

            _lock = false;
        }

        public Step ToEntity()
        {
            var step = new Step
            {
                Id = Id,
                Name = Name,
                Location = Location,
                Type = Type,
                Color = Color,
                Comment = Comment,
                IsSkip = IsSkip
            };
            if (Type.ContainFlag(ParameterFlag.Point))
            {
                step.X = double.Parse(X);
                step.Y = double.Parse(Y);
            }
            if (Type.ContainFlag(ParameterFlag.Amount))
            {
                if (Amount == "All")
                {
                    step.Amount = 0;
                }
                else
                {
                    step.Amount = int.Parse(Amount);
                }
            }
            if (Type.ContainFlag(ParameterFlag.Item))
            {
                step.Item = Item;
            }
            if (Type.ContainFlag(ParameterFlag.Orientation))
            {
                step.Orientation = OrientationTypeExtensions.FromString(Orientation)!;
            }
            else if (Type.ContainFlag(ParameterFlag.Inventory))
            {
                step.Inventory = InventoryTypeExtensions.FromString(Orientation)!;
            }
            else if (Type.ContainFlag(ParameterFlag.Priority))
            {
                step.Priority = Priority.FromString(Orientation)!;
            }
            if (Type.ContainFlag(ParameterFlag.Modifier))
            {
                step.Modifier = ModifierTypeExtensions.FromString(Modifier)!;
            }
            return step;
        }

        public void FromEntity(Step step)
        {
            _loaded = false;

            Id = step.Id;
            Name = step.Name;
            Location = step.Location;
            Type = step.Type;

            if (step.Type.ContainFlag(ParameterFlag.Point))
            {
                X = $"{step.X:F2}";
                Y = $"{step.Y:F2}";
            }
            else
            {
                X = "";
                Y = "";
            }

            if (step.Type.ContainFlag(ParameterFlag.Amount))
            {
                if (step.Amount < 1)
                {
                    Amount = "All";
                }
                else
                {
                    Amount = $"{step.Amount}";
                }
            }
            else
            {
                Amount = "";
            }

            Item = step.Item;

            if (step.Type.ContainFlag(ParameterFlag.Orientation))
            {
                Orientation = OrientationTypeExtensions.ToString(step.Orientation);
            }
            else if (step.Type.ContainFlag(ParameterFlag.Inventory))
            {
                Orientation = InventoryTypeExtensions.ToString(step.Inventory);
            }
            else if (step.Type.ContainFlag(ParameterFlag.Priority))
            {
                Orientation = Priority.ToString(step.Priority);
            }
            else
            {
                Orientation = "";
            }

            if (step.Type.ContainFlag(ParameterFlag.Modifier))
            {
                Modifier = ModifierTypeExtensions.ToString(step.Modifier);
            }
            else
            {
                Modifier = "";
            }

            Color = step.Color;
            Comment = step.Comment;
            IsSkip = step.IsSkip;

            _loaded = true;
        }
    }
}