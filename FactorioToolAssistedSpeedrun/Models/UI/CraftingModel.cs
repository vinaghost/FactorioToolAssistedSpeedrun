using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class CraftingModel : ObservableObject
    {
        private readonly CommandStack _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
        private readonly StartupService _startupService = App.Current.Services.GetRequiredService<StartupService>();
        public Guid Id { get; set; }

        private bool _loaded = false;
        private bool _lock = false;

        [ObservableProperty]
        private StepType _type;

        [ObservableProperty]
        private int _location;

        [ObservableProperty]
        private string _item = "";

        partial void OnItemChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;

            _lock = true;

            if (!_startupService.GameData!.Items.ContainsKey(newValue))
            {
                Item = oldValue ?? "";
            }
            else
            {
                var command = _commandStack.Push<UpdateStepPropertyCommand<string, string>>();
                if (command is not null)
                {
                    command.Setup(new("", Id, oldValue ?? "", newValue,
                        str => str,
                        model => model.Item,
                        step => step.Item));

                    command.Commit();
                }
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
                        command.Setup(new("", Id, oldValue ?? "", newValue,
                            str => int.Parse(str),
                            model => model.Amount,
                            step => step.Amount));

                        command.Commit();
                    }
                }
            }

            _lock = false;
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
                command.Setup(new("", Id, oldValue ?? "", newValue,
                    str => str,
                    model => model.Comment,
                    step => step.Comment));

                command.Commit();
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
                command.Setup(new("", Id, oldValue, newValue,
                    str => str,
                    model => model.IsSkip,
                    step => step.IsSkip));

                command.Commit();
            }
        }

        public void FromEntity(Step step)
        {
            if (step.Type != StepType.Craft) return;
            _loaded = false;

            Id = step.Id;
            Type = step.Type;
            Location = step.Location;

            Item = step.Item;
            Comment = step.Comment;
            IsSkip = step.IsSkip;
            if (step.Amount < 1)
            {
                Amount = "All";
            }
            else
            {
                Amount = $"{step.Amount}";
            }
            _loaded = true;
        }
    }
}