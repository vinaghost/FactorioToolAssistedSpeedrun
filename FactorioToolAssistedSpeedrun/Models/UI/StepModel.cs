using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class StepModel : ObservableObject
    {
        public Guid Id { get; private set; }

        [ObservableProperty]
        private int _location;

        [ObservableProperty]
        private StepType _type;

        [ObservableProperty]
        private string _x = "";

        [ObservableProperty]
        private string _y = "";

        [ObservableProperty]
        private string _amount = "";

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
        }
    }
}