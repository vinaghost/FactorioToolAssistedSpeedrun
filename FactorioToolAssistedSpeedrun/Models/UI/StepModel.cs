using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using System.Net.Http.Headers;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class StepModel : ObservableObject
    {
        public Guid Id { get; private set; }
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
            if (double.TryParse(newValue, out _)) return;

            _lock = true;
            X = oldValue ?? "";
            _lock = false;
        }

        [ObservableProperty]
        private string _y = "";

        partial void OnYChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            if (double.TryParse(newValue, out _)) return;

            _lock = true;
            Y = oldValue ?? "";
            _lock = false;
        }

        [ObservableProperty]
        private string _amount = "";

        partial void OnAmountChanged(string? oldValue, string newValue)
        {
            if (!_loaded) return;
            if (_lock) return;
            if (newValue == "All") newValue = "0";
            if (int.TryParse(newValue, out _)) return;
            _lock = true;
            Amount = oldValue ?? "";
            _lock = false;
        }

        [ObservableProperty]
        private string _item = "";

        [ObservableProperty]
        private string _orientation = "";

        [ObservableProperty]
        private string _modifier = "";

        [ObservableProperty]
        private string _color = "";

        [ObservableProperty]
        private string _comment = "";

        [ObservableProperty]
        private bool _isSkip;

        public void FromEntity(Step step)
        {
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
    }
}