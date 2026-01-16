using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Commands.Steps;
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
        private readonly StartupService _startupService = App.Current.Services.GetRequiredService<StartupService>();
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = "";
        private bool _loaded = false;
        private bool _lock = false;

        [ObservableProperty]
        private int _location;

        [ObservableProperty]
        private StepType _type;

        [ObservableProperty]
        private string _x = "";

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
                    var command = new UpdateStepPropertyCommand<string, double>()
                    {
                        StepId = Id,
                        Name = Name,
                        OldValue = oldValue ?? "",
                        NewValue = newValue,
                        StepPropertySelector = step => step.X,
                        StepModelPropertySelector = model => model.X,
                        StepPropertyTransformer = str => double.Parse(str)
                    };
                    command.Commit();
                    _commandStack.Push(command);
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
                    var command = new UpdateStepPropertyCommand<string, double>()
                    {
                        StepId = Id,
                        OldValue = oldValue ?? "",
                        NewValue = newValue,
                        Name = Name,
                        StepPropertySelector = step => step.Y,
                        StepModelPropertySelector = model => model.Y,
                        StepPropertyTransformer = str => double.Parse(str)
                    };
                    command.Commit();
                    _commandStack.Push(command);
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
                        var command = new UpdateStepPropertyCommand<string, int>()
                        {
                            StepId = Id,
                            OldValue = oldValue ?? "",
                            NewValue = newValue,
                            Name = Name,
                            StepPropertySelector = step => step.Amount,
                            StepModelPropertySelector = model => model.Amount,
                            StepPropertyTransformer = str => int.Parse(str)
                        };
                        command.Commit();
                        _commandStack.Push(command);
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

        partial void OnItemChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            _lock = true;

            if (Type.ContainFlag(ParameterFlag.Item))
            {
                if (Type == StepType.Tech)
                {
                    if (!_startupService.GameData!.Technologies.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = new UpdateStepPropertyCommand<string, string>()
                        {
                            StepId = Id,
                            OldValue = oldValue ?? "",
                            NewValue = newValue,
                            Name = Name,
                            StepPropertySelector = step => step.Item,
                            StepModelPropertySelector = model => model.Item,
                            StepPropertyTransformer = str => str
                        };
                        command.Commit();
                        _commandStack.Push(command);
                    }
                }
                else if (Type == StepType.Recipe)
                {
                    if (!_startupService.GameData!.Recipes.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = new UpdateStepPropertyCommand<string, string>()
                        {
                            StepId = Id,
                            OldValue = oldValue ?? "",
                            NewValue = newValue,
                            Name = Name,
                            StepPropertySelector = step => step.Item,
                            StepModelPropertySelector = model => model.Item,
                            StepPropertyTransformer = str => str
                        };
                        command.Commit();
                        _commandStack.Push(command);
                    }
                }
                else
                {
                    if (!_startupService.GameData!.Items.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = new UpdateStepPropertyCommand<string, string>()
                        {
                            StepId = Id,
                            OldValue = oldValue ?? "",
                            NewValue = newValue,
                            Name = Name,
                            StepPropertySelector = step => step.Item,
                            StepModelPropertySelector = model => model.Item,
                            StepPropertyTransformer = str => str
                        };
                        command.Commit();
                        _commandStack.Push(command);
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
                    var command = new UpdateStepPropertyCommand<string, OrientationType?>()
                    {
                        StepId = Id,
                        OldValue = oldValue ?? "",
                        NewValue = newValue,
                        Name = Name,
                        StepPropertySelector = step => step.Orientation,
                        StepModelPropertySelector = model => model.Orientation,
                        StepPropertyTransformer = str => OrientationTypeExtensions.FromString(str)
                    };
                    command.Commit();
                    _commandStack.Push(command);
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
                    var command = new UpdateStepPropertyCommand<string, InventoryType?>()
                    {
                        StepId = Id,
                        OldValue = oldValue ?? "",
                        NewValue = newValue,
                        Name = Name,
                        StepPropertySelector = step => step.Inventory,
                        StepModelPropertySelector = model => model.Orientation,
                        StepPropertyTransformer = str => InventoryTypeExtensions.FromString(str)
                    };
                    command.Commit();
                    _commandStack.Push(command);
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
                    var command = new UpdateStepPropertyCommand<string, Priority?>()
                    {
                        StepId = Id,
                        OldValue = oldValue ?? "",
                        NewValue = newValue,
                        Name = Name,
                        StepPropertySelector = step => step.Priority,
                        StepModelPropertySelector = model => model.Orientation,
                        StepPropertyTransformer = str => Priority.FromString(str)
                    };
                    command.Commit();
                    _commandStack.Push(command);
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
                        var command = new UpdateStepPropertyCommand<string, ModifierType?>()
                        {
                            StepId = Id,
                            OldValue = oldValue ?? "",
                            NewValue = newValue,
                            Name = Name,
                            StepPropertySelector = step => step.Modifier,
                            StepModelPropertySelector = model => model.Modifier,
                            StepPropertyTransformer = str => ModifierTypeExtensions.FromString(str)
                        };
                        command.Commit();
                        _commandStack.Push(command);
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

        partial void OnColorChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            var command = new UpdateStepPropertyCommand<string, string>()
            {
                StepId = Id,
                OldValue = oldValue ?? "",
                NewValue = newValue,
                Name = Name,
                StepPropertySelector = step => step.Color,
                StepModelPropertySelector = model => model.Color,
                StepPropertyTransformer = str => str
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [ObservableProperty]
        private string _comment = "";

        partial void OnCommentChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            var command = new UpdateStepPropertyCommand<string, string>()
            {
                StepId = Id,
                OldValue = oldValue ?? "",
                NewValue = newValue,
                Name = Name,
                StepPropertySelector = step => step.Comment,
                StepModelPropertySelector = model => model.Comment,
                StepPropertyTransformer = str => str
            };
            command.Commit();
            _commandStack.Push(command);
        }

        [ObservableProperty]
        private bool _isSkip;

        partial void OnIsSkipChanged(bool oldValue, bool newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            var command = new UpdateStepPropertyCommand<bool, bool>()
            {
                StepId = Id,
                OldValue = oldValue,
                NewValue = newValue,
                Name = Name,
                StepPropertySelector = step => step.IsSkip,
                StepModelPropertySelector = model => model.IsSkip,
                StepPropertyTransformer = str => str
            };
            command.Commit(true);
            _commandStack.Push(command);
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