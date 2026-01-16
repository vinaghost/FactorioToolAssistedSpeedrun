using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class ImportStringViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly StartupService _startupService;
        private readonly CommandStack _commandStack;

        public ImportStringViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
            _startupService = App.Current.Services.GetRequiredService<StartupService>();
            _commandStack = App.Current.Services.GetRequiredService<CommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public ImportStringViewModel(PanelService panelService, StartupService startupService, CommandStack commandStack)
        {
            _panelService = panelService;
            _startupService = startupService;
            _commandStack = commandStack;
        }

        [ObservableProperty]
        private int _lineIndex = 1;

        partial void OnLineIndexChanged(int oldValue, int newValue)
        {
            if (newValue < 1)
            {
                LineIndex = 1;
            }
        }

        [ObservableProperty]
        private string _templateName = "";

        [ObservableProperty]
        private string _importString = "";

        [ObservableProperty]
        private bool _clearAfterImport = true;

        [RelayCommand]
        private void CurrentStepIndex()
        {
            var index = _panelService.SelectedStepIndex;
            LineIndex = index + 1;
        }

        [RelayCommand]
        private void IntoStep(bool right)
        {
            try
            {
                var steps = ExtractStep().ToList();
                if (steps.Count == 0)
                {
                    MessageBox.Show("No steps to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                for (var i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    if (right)
                    {
                        step.Location = LineIndex + i + 1;
                    }
                    else
                    {
                        step.Location = LineIndex + i;
                    }
                    step.Name = "";
                }

                var command = new AddStepCommand
                {
                    Name = "",
                    Steps = steps,
                };
                command.Commit();
                _commandStack.Push(command);

                if (ClearAfterImport)
                {
                    ImportString = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void IntoTemplate()
        {
            if (_panelService.TemplateCollection.Contains(TemplateName))
            {
                MessageBox.Show("Template name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var steps = ExtractStep().ToList();
                if (steps.Count == 0)
                {
                    MessageBox.Show("No steps to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                for (var i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    step.Location = i + 1;
                    step.Name = TemplateName;
                }
                var command = new AddStepCommand
                {
                    Name = TemplateName,
                    Steps = steps,
                };
                command.Commit();
                _commandStack.Push(command);

                _panelService.AddTemplate(TemplateName);

                if (ClearAfterImport)
                {
                    ImportString = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IEnumerable<Step> ExtractStep()
        {
            var lines = ImportString.Split(Environment.NewLine);

            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var segments = line.Split(';');

                var step = ReadStep(segments);
                yield return step;
            }
        }

        private Step ReadStep(string[] segments)
        {
            if (segments.Length < 9)
            {
                throw new Exception($"Invalid step format: {string.Join(',', segments)}");
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
                    throw new Exception($"Unknown recipe: {segments[4]}");
                }
                return value;
            }

            static string GetRecipeName(string[] segments, GameData gameData)
            {
                if (!gameData.ReverseRecipesLocale.TryGetValue(segments[4], out string? value))
                {
                    throw new Exception($"Unknown recipe: {segments[4]}");
                }
                return value;
            }
            static string GetTechName(string[] segments, GameData gameData)
            {
                if (!gameData.ReverseTechnologiesLocale.TryGetValue(segments[4], out string? value))
                {
                    throw new Exception($"Unknown technology: {segments[4]}");
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