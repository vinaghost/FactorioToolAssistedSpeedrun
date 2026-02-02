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
        public StepModel(ICommandStack? commandStack = null, IDataService? dataService = null)
        {
            _commandStack = commandStack ?? App.Current.Services.GetRequiredService<ICommandStack>();
            _dataService = dataService ?? App.Current.Services.GetRequiredService<IDataService>();
        }

        private readonly ICommandStack _commandStack;
        private readonly IDataService _dataService;
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
                    var command = _commandStack.Push<UpdateStepPropertyCommand<string, double>>();
                    if (command is not null)
                    {
                        command.Setup(new(Name, Id, oldValue ?? "", newValue,
                            str => double.Parse(str),
                            model => model.X,
                            step => step.X));

                        command.Commit(true);
                    }
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
                    var command = _commandStack.Push<UpdateStepPropertyCommand<string, double>>();
                    if (command is not null)
                    {
                        command.Setup(new(Name, Id, oldValue ?? "", newValue,
                            str => double.Parse(str),
                            model => model.Y,
                            step => step.Y));

                        command.Commit(true);
                    }
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
                        var command = _commandStack.Push<UpdateStepPropertyCommand<string, int>>();
                        if (command is not null)
                        {
                            command.Setup(new(Name, Id, oldValue ?? "", newValue,
                                str => int.Parse(str),
                                model => model.Amount,
                                step => step.Amount));

                            command.Commit(true);
                        }
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
                    if (!_dataService.GameData.Technologies.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
                        if (command is not null)
                        {
                            command.Setup(new(Name, Id, oldValue ?? "", newValue,
                                str => str,
                                model => model.Item,
                                step => step.Item));

                            command.Commit(true);
                        }
                    }
                }
                else if (Type == StepType.Recipe)
                {
                    if (!_dataService.GameData.Recipes.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
                        if (command is not null)
                        {
                            command.Setup(new(Name, Id, oldValue ?? "", newValue,
                                str => str,
                                model => model.Item,
                                step => step.Item));

                            command.Commit(true);
                        }
                    }
                }
                else
                {
                    if (!_dataService.GameData.Items.ContainsKey(newValue))
                    {
                        Item = oldValue ?? "";
                    }
                    else
                    {
                        var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
                        if (command is not null)
                        {
                            command.Setup(new(Name, Id, oldValue ?? "", newValue,
                                str => str,
                                model => model.Item,
                                step => step.Item));

                            command.Commit(true);
                        }
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
                    var command = _commandStack.Push<UpdateStepPropertyCommand<string, OrientationType?>>();
                    if (command is not null)
                    {
                        command.Setup(new(Name, Id, oldValue ?? "", newValue,
                            str => OrientationTypeExtensions.FromString(str),
                            model => model.Orientation,
                            step => step.Orientation));

                        command.Commit(true);
                    }
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
                    var command = _commandStack.Push<UpdateStepPropertyCommand<string, InventoryType?>>();
                    if (command is not null)
                    {
                        command.Setup(new(Name, Id, oldValue ?? "", newValue,
                            str => InventoryTypeExtensions.FromString(str),
                            model => model.Inventory,
                            step => step.Orientation));

                        command.Commit(true);
                    }
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
                    var command = _commandStack.Push<UpdateStepPropertyCommand<string, Priority?>>();
                    if (command is not null)
                    {
                        command.Setup(new(Name, Id, oldValue ?? "", newValue,
                            str => Priority.FromString(str),
                            model => model.Priority,
                            step => step.Orientation));

                        command.Commit(true);
                    }
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
                        var command = _commandStack.Push<UpdateStepPropertyCommand<string, ModifierType?>>();
                        if (command is not null)
                        {
                            command.Setup(new(Name, Id, oldValue ?? "", newValue,
                                str => ModifierTypeExtensions.FromString(str),
                                model => model.Modifier,
                                step => step.Modifier));

                            command.Commit(true);
                        }
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

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
            if (command is not null)
            {
                command.Setup(new(Name, Id, oldValue ?? "", newValue,
                    str => str,
                    model => model.Color,
                    step => step.Color));

                command.Commit(true);
            }
        }

        [ObservableProperty]
        private string _comment = "";

        partial void OnCommentChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
            if (command is not null)
            {
                command.Setup(new(Name, Id, oldValue ?? "", newValue,
                    str => str,
                    model => model.Comment,
                    step => step.Comment));

                command.Commit(true);
            }
        }

        [ObservableProperty]
        private bool _isSkip;

        partial void OnIsSkipChanged(bool oldValue, bool newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            var command = _commandStack.Push<UpdateStepPropertyCommand<bool, bool>>();
            if (command is not null)
            {
                command.Setup(new(Name, Id, oldValue, newValue,
                    str => str,
                    model => model.IsSkip,
                    step => step.IsSkip));

                command.Commit(true);
            }
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