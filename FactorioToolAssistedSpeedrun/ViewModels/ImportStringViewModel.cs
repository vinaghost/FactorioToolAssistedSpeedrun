using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands.Features;
using FactorioToolAssistedSpeedrun.Commands.Steps;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Queries;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class ImportStringViewModel : ObservableObject
    {
        private readonly PanelService _panelService;
        private readonly IDataService _dataService;
        private readonly ICommandStack _commandStack;

        public ImportStringViewModel()
        {
            _panelService = App.Current.Services.GetRequiredService<PanelService>();
            _dataService = App.Current.Services.GetRequiredService<IDataService>();
            _commandStack = App.Current.Services.GetRequiredService<ICommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public ImportStringViewModel(PanelService panelService, IDataService dataService, ICommandStack commandStack)
        {
            _panelService = panelService;
            _dataService = dataService;
            _commandStack = commandStack;
        }

        [RelayCommand]
        private async Task Load()
        {
            if (!_dataService.IsProjectDataLoaded) return;

            var getImportIntoRowQuery = new GetImportIntoRowQuery()
            {
                ProjectDataFile = _dataService.ProjectDataFile
            };
            var rowIndex = await Task.Run(getImportIntoRowQuery.Execute);
            LineIndex = rowIndex;
        }

        [ObservableProperty]
        private int _lineIndex = 0;

        partial void OnLineIndexChanged(int oldValue, int newValue)
        {
            if (newValue < 0)
            {
                LineIndex = 0;
            }
        }

        [ObservableProperty]
        private string _templateName = "";

        [ObservableProperty]
        private string _importString = "";

        [ObservableProperty]
        private bool _clearAfterImport = true;

        [RelayCommand]
        private async Task CurrentStepIndex()
        {
            LineIndex = _panelService.SelectedStepIndex;
            await UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.ImportIntoRow, LineIndex.ToString());
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

                var line = LineIndex + 1;
                for (var i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    if (right)
                    {
                        step.Location = line + i + 1;
                    }
                    else
                    {
                        step.Location = line + i;
                    }
                    step.Name = "";
                }

                var command = _commandStack.Push<AddStepCommand>();
                if (command is not null)
                {
                    command.Setup(new("", steps));
                    command.Commit();
                }

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
                var command = _commandStack.Push<AddStepCommand>();
                if (command is not null)
                {
                    command.Setup(new(TemplateName, steps));
                    command.Commit();
                }

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
                var amount = double.TryParse(segments[3], out double amountVal) ? amountVal : 0;
                return (int)amount;
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
                    StepType.Tech => GetTechName(segments, _dataService.GameData),
                    StepType.Recipe => GetRecipeName(segments, _dataService.GameData),
                    _ => GetItemName(segments, _dataService.GameData),
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