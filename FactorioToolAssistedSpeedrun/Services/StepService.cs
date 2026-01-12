using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class StepService : ObservableObject
    {
        private readonly StartupService _startupService;

        public StepService()
        {
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
        }

        [ActivatorUtilitiesConstructor]
        public StepService(StartupService startupService)
        {
            _startupService = startupService;
            _startupService.OnGameDataLoaded += OnGameDataLoaded;
        }

        public event Action<StepType>? TypeChanged;

        private void OnGameDataLoaded()
        {
            App.Current.Dispatcher.Invoke(() => Type = StepType.Walk);
        }

        [ObservableProperty]
        private StepType _type;

        partial void OnTypeChanged(StepType value)
        {
            TypeChanged?.Invoke(value);
            Modifier = null;
        }

        [ObservableProperty]
        private double _x;

        [ObservableProperty]
        private double _y;

        [ObservableProperty]
        private int _amount;

        [ObservableProperty]
        private string _comment = "";

        [ObservableProperty]
        private string _selectedItem = "";

        [ObservableProperty]
        private InventoryType? _inventory;

        [ObservableProperty]
        private PriorityType _inputPriority;

        [ObservableProperty]
        private PriorityType _outputPriority;

        [ObservableProperty]
        private OrientationType _orientation = OrientationType.North;

        [ObservableProperty]
        private ModifierType? _modifier;

        public void FromStep(StepModel step)
        {
            Type = step.Type;
            Comment = step.Comment;

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
        }

        public Step ToStep()
        {
            var step = new Step
            {
                Type = Type,
                Comment = Comment,
            };
            if (Type.ContainFlag(ParameterFlag.Point))
            {
                step.X = X;
                step.Y = Y;
            }
            if (Type.ContainFlag(ParameterFlag.Amount))
            {
                step.Amount = Amount;
            }
            if (Type.ContainFlag(ParameterFlag.Item))
            {
                step.Item = SelectedItem;
            }
            if (Type.ContainFlag(ParameterFlag.Inventory))
            {
                step.Inventory = Inventory;
            }
            if (Type.ContainFlag(ParameterFlag.Priority))
            {
                var priority = new Priority()
                {
                    In = InputPriority,
                    Out = OutputPriority
                };
                step.Priority = priority;
            }
            if (Type.ContainFlag(ParameterFlag.Orientation))
            {
                step.Orientation = Orientation;
            }
            if (Type.ContainFlag(ParameterFlag.Modifier))
            {
                step.Modifier = Modifier;
            }

            return step;
        }
    }
}