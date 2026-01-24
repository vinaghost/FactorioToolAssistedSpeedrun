using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;

namespace FactorioToolAssistedSpeedrun.Models.UI
{
    public partial class CraftingModel : ObservableObject
    {
        public Guid Id { get; set; }

        [ObservableProperty]
        private StepType _type;

        [ObservableProperty]
        private int _location;

        [ObservableProperty]
        private string _item = "";

        [ObservableProperty]
        private string _amount = "";

        [ObservableProperty]
        private string _comment = "";

        [ObservableProperty]
        private bool _isSkip;

        public void FromEntity(Step step)
        {
            if (step.Type != StepType.Craft) return;

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
        }
    }
}