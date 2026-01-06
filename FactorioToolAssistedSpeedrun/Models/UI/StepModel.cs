using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class StepModel : ObservableObject
    {
        private readonly CommandStack _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
        public Guid Id { get; private set; }
        private bool _loaded = false;
        private bool _lock = false;
        private Step? _cached = null;

        [ObservableProperty]
        private int _location;

        [ObservableProperty]
        private StepType _type;

        [ObservableProperty]
        private string _x = "";

        partial void OnXChanging(string value) => UpdateCache();

        partial void OnXChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            _lock = true;
            if (Type.ContainFlag(ParameterFlag.Point))
            {
                if (!double.TryParse(newValue, out _))
                {
                    X = oldValue ?? "";
                }
                else
                {
                    UpdateProperty();
                }
            }
            else
            {
                X = "";
            }

            _lock = false;
        }

        [ObservableProperty]
        private string _y = "";

        partial void OnYChanging(string value) => UpdateCache();

        partial void OnYChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            _lock = true;
            if (Type.ContainFlag(ParameterFlag.Point))
            {
                if (!double.TryParse(newValue, out _))
                {
                    Y = oldValue ?? "";
                }
                else
                {
                    UpdateProperty();
                }
            }
            else
            {
                Y = "";
            }
            _lock = false;
        }

        [ObservableProperty]
        private string _amount = "";

        partial void OnAmountChanging(string value) => UpdateCache();

        partial void OnAmountChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            if (newValue == "All") newValue = "0";

            _lock = true;
            if (Type.ContainFlag(ParameterFlag.Amount))
            {
                if (!double.TryParse(newValue, out var value))
                {
                    Amount = oldValue ?? "";
                }
                else
                {
                    if (value == 0)
                    {
                        Amount = "All";
                    }
                    else if (value < 0)
                    {
                        Amount = oldValue ?? "";
                    }
                    else
                    {
                        UpdateProperty();
                    }
                }
            }
            else
            {
                Amount = "";
            }

            _lock = false;
        }

        [ObservableProperty]
        private string _item = "";

        partial void OnItemChanging(string value) => UpdateCache();

        partial void OnItemChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            _lock = true;

            if (Type.ContainFlag(ParameterFlag.Item))
            {
                if (Type == StepType.Tech)
                {
                    if (!App.Current.GameData!.Technologies.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        UpdateProperty();
                    }
                }
                else if (Type == StepType.Recipe)
                {
                    if (!App.Current.GameData!.Recipes.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        UpdateProperty();
                    }
                }
                else
                {
                    if (!App.Current.GameData!.Items.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        UpdateProperty();
                    }
                }
            }
            else
            {
                Item = "";
            }

            _lock = false;
        }

        [ObservableProperty]
        private string _orientation = "";

        partial void OnOrientationChanging(string value) => UpdateCache();

        partial void OnOrientationChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            _lock = true;
            if (Type.ContainFlag(ParameterFlag.Orientation))
            {
                if (!OrientationTypeExtensions.TryGetValue(newValue, out _))
                {
                    Orientation = oldValue ?? "";
                }
                else
                {
                    UpdateProperty();
                }
            }
            else if (Type.ContainFlag(ParameterFlag.Inventory))
            {
                if (!InventoryTypeExtensions.TryGetValue(newValue, out _))
                {
                    Orientation = oldValue ?? "";
                }
                else
                {
                    UpdateProperty();
                }
            }
            else if (Type.ContainFlag(ParameterFlag.Priority))
            {
                if (Priority.FromString(newValue) is null)
                {
                    Orientation = oldValue ?? "";
                }
                else
                {
                    UpdateProperty();
                }
            }
            else
            {
                Orientation = "";
            }
            _lock = false;
        }

        [ObservableProperty]
        private string _modifier = "";

        partial void OnModifierChanging(string value) => UpdateCache();

        partial void OnModifierChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            if (string.IsNullOrEmpty(newValue)) return;
            _lock = true;
            if (Type.ContainFlag(ParameterFlag.Modifier))
            {
                if (!ModifierTypeExtensions.TryGetValue(newValue, out var value))
                {
                    Modifier = oldValue ?? "";
                }
                else
                {
                    if (Type == StepType.Mine && value != ModifierType.Split)
                    {
                        Modifier = oldValue ?? "";
                    }
                    else if (Type == StepType.Take && value != ModifierType.All)
                    {
                        Modifier = oldValue ?? "";
                    }
                    else if (Type == StepType.Wait && value != ModifierType.WalkTowards)
                    {
                        Modifier = oldValue ?? "";
                    }
                    else
                    {
                        UpdateProperty();
                    }
                }
            }
            else
            {
                Modifier = "";
            }
            _lock = false;
        }

        [ObservableProperty]
        private string _color = "";

        partial void OnColorChanging(string value) => UpdateCache();

        partial void OnColorChanged(string? oldValue, string newValue) => UpdateProperty();

        [ObservableProperty]
        private string _comment = "";

        partial void OnCommentChanging(string value) => UpdateCache();

        partial void OnCommentChanged(string? oldValue, string newValue) => UpdateProperty();

        [ObservableProperty]
        private bool _isSkip;

        partial void OnIsSkipChanging(bool value) => UpdateCache();

        partial void OnIsSkipChanged(bool oldValue, bool newValue) => UpdateProperty();

        public Step ToEntity()
        {
            var step = new Step
            {
                Id = Id,
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
            step.Item = Item;
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

        private void UpdateCache()
        {
            if (!_loaded) return;
            if (_lock) return;
            _cached = ToEntity();
        }

        private void UpdateProperty()
        {
            if (_cached is null) return;

            var newStep = ToEntity();
            var command = new UpdateStepPropertyCommand
            {
                OldSteps = _cached,
                NewSteps = newStep
            };
            command.Commit();
            _commandStack.Push(command);
            _cached = null;
        }
    }
}