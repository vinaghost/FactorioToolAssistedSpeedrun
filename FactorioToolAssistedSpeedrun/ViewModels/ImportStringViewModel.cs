using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Exceptions;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class ImportStringViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly StartupService _startupService;

        public ImportStringViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
        }

        [ActivatorUtilitiesConstructor]
        public ImportStringViewModel(PanelService panelService, StartupService startupService)
        {
            _panelService = panelService;
            _startupService = startupService;
        }

        [ObservableProperty]
        private int _lineIndex;

        [ObservableProperty]
        private string _importString = "";

        [RelayCommand]
        public void CurrentStepIndex(bool right)
        {
            var index = _panelService.SelectedStepIndex;
            if (right)
            {
                var totalRow = _panelService.StepCollection.Count;
                LineIndex = index - totalRow - 1;
            }
            else
            {
                LineIndex = index - 1;
            }
        }

        public IEnumerable<Step> ExtractStep()
        {
            var lines = ImportString.Split('\n');

            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var segments = line.Split(';');

                var step = ReadStep(segments);
                step.Name = "";
                yield return step;
            }
        }

        private Step ReadStep(string[] segments)
        {
            if (segments.Length < 9)
            {
                throw new TasFileParserException($"Invalid step format: {string.Join(',', segments)}");
            }

            static double GetX(string[] segments)
            {
                var x = double.TryParse(segments[1], out double xVal) ? xVal : 0;
                return x;
            }
            static double GetY(string[] segments)
            {
                var y = double.TryParse(segments[2], out double yVal) ? yVal : 0;
                return y;
            }
            static int GetAmount(string[] segments)
            {
                var amount = int.TryParse(segments[3], out int amountVal) ? amountVal : 0;
                return amount;
            }

            static string GetItemName(string[] segments, GameData gameData)
            {
                if (!gameData.ReverseItemsLocale.TryGetValue(segments[4], out string? value))
                {
                    throw new TasFileParserException($"Unknown recipe: {segments[4]}");
                }
                return value;
            }

            static string GetRecipeName(string[] segments, GameData gameData)
            {
                if (!gameData.ReverseRecipesLocale.TryGetValue(segments[4], out string? value))
                {
                    throw new TasFileParserException($"Unknown recipe: {segments[4]}");
                }
                return value;
            }
            static string GetTechName(string[] segments, GameData gameData)
            {
                if (!gameData.ReverseTechnologiesLocale.TryGetValue(segments[4], out string? value))
                {
                    throw new TasFileParserException($"Unknown technology: {segments[4]}");
                }
                return value;
            }

            static ModifierType? GetModifierString(string[] segments)
            {
                var modifierSegments = segments[8].Split(',');
                foreach (var modifierStr in modifierSegments.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()))
                {
                    if (ModifierTypeExtensions.TryGetValue(modifierStr, out ModifierType modifierValue))
                    {
                        return modifierValue;
                    }
                }
                return null;
            }

            var type = StepTypeExtensions.FromString(segments[0]);
            var comment = segments[6];
            var color = segments[7];
            var isSkip = segments[8].Contains("skip");

            var step = new Step()
            {
                Type = type,
                IsSkip = isSkip,
                Comment = comment,
                Color = color,
            };

            if (type.ContainFlag(ParameterFlag.Point))
            {
                step.X = GetX(segments);
                step.Y = GetY(segments);
            }

            if (type.ContainFlag(ParameterFlag.Item))
            {
                step.Item = type switch
                {
                    StepType.Tech => GetTechName(segments, _startupService.GameData!),
                    StepType.Recipe => GetRecipeName(segments, _startupService.GameData!),
                    _ => GetItemName(segments, _startupService.GameData!),
                };
            }

            if (type.ContainFlag(ParameterFlag.Amount))
            {
                step.Amount = GetAmount(segments);
            }

            if (type.ContainFlag(ParameterFlag.Modifier))
            {
                step.Modifier = GetModifierString(segments);
            }

            if (type.ContainFlag(ParameterFlag.Priority))
            {
                step.Priority = Priority.FromString(segments[5]);
            }
            if (type.ContainFlag(ParameterFlag.Orientation))
            {
                step.Orientation = OrientationTypeExtensions.FromString(segments[5]);
            }
            if (type.ContainFlag(ParameterFlag.Inventory))
            {
                step.Inventory = InventoryTypeExtensions.FromString(segments[5]);
            }

            return step;
        }
    }
}