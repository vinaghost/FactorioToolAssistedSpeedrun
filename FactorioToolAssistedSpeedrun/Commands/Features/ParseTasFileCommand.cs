using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.Game;
using System.IO;

namespace FactorioToolAssistedSpeedrun.Commands.Features
{
    public class TasFileObject
    {
        public required List<Step> StepCollection { get; init; }
        public required string Goal { get; init; }
        public required string ScriptFolder { get; init; }
        public required int SelectedRow { get; init; }
        public required int ImportIntoRow { get; init; }
        public required bool PrintComments { get; init; }
        public required bool PrintSavegame { get; init; }
        public required bool PrintTech { get; init; }
        public int Environment { get; init; }
    }

    public static class ParseTasFileCommand
    {
        public static async Task<TasFileObject> Execute(string fileName, GameData gameData)
        {
            using var sr = File.OpenText(fileName);
            var totalStepsIndicatorLine = await sr.ReadLineAsync() ?? throw new Exception("Empty file");

            if (!totalStepsIndicatorLine.Equals(TasFileConstants.TOTAL_STEPS_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.TOTAL_STEPS_INDICATOR} but got: {totalStepsIndicatorLine}");
            }

            var totalStepsLine = await sr.ReadLineAsync() ?? throw new Exception("Expected total steps line but file ended");

            if (!int.TryParse(totalStepsLine, out var totalStep))
            {
                throw new Exception($"Invalid total steps value {totalStepsLine}");
            }

            var goalIndicatorLine = await sr.ReadLineAsync() ?? throw new Exception("Expected goal indicator line but file ended");

            if (!goalIndicatorLine.Equals(TasFileConstants.GOAL_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.GOAL_INDICATOR} but got: {goalIndicatorLine}");
            }

            var goal = await sr.ReadLineAsync() ?? throw new Exception("Expected goal line but file ended");

            var stepIndicatorLine = await sr.ReadLineAsync() ?? throw new Exception("Expected steps indicator line but file ended");
            if (!stepIndicatorLine.Equals(TasFileConstants.STEPS_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.STEPS_INDICATOR} but got: {stepIndicatorLine}");
            }

            var stepLine = await sr.ReadLineAsync();
            var steps = new List<Step>();
            string[] segments;
            while (stepLine is not null)
            {
                if (stepLine.Equals(TasFileConstants.TEMPLATES_INDICATOR))
                {
                    break;
                }

                segments = stepLine.Split(';');

                var step = ReadStep(segments, gameData);
                step.Location = steps.Count + 1;
                step.Name = "";
                steps.Add(step);
                stepLine = await sr.ReadLineAsync();
            }

            if (stepLine is null) throw new Exception("Expected templates indicator line but file ended");

            if (!stepLine.Equals(TasFileConstants.TEMPLATES_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.TEMPLATES_INDICATOR} but got: {stepLine}");
            }

            var templateLine = await sr.ReadLineAsync();

            while (templateLine is not null)
            {
                if (templateLine.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
                {
                    break;
                }
                segments = templateLine.Split(';');
                if (segments.Length < 10)
                {
                    throw new Exception($"Invalid template format: {templateLine}");
                }
                var name = segments[0];
                var step = ReadStep(segments[1..10], gameData);
                step.Location = steps.Count(x => x.Name == name) + 1;
                step.Name = name;
                steps.Add(step);
                templateLine = await sr.ReadLineAsync();
            }

            if (templateLine is null) throw new Exception("Expected save file indicator line but file ended");
            if (!templateLine.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.SAVE_FILE_INDICATOR} but got: {templateLine}");
            }

            var saveFileLine = await sr.ReadLineAsync() ?? throw new Exception("Expected save file line but file ended");
            _ = saveFileLine;

            var codeFileIndicatorLine = await sr.ReadLineAsync() ?? throw new Exception("Expected step folder indicator line but file ended");
            if (!codeFileIndicatorLine.Equals(TasFileConstants.CODE_FILE_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.CODE_FILE_INDICATOR} but got: {codeFileIndicatorLine}");
            }

            var codeFileLine = await sr.ReadLineAsync() ?? throw new Exception("Expected step folder line but file ended");
            var scriptFolder = codeFileLine[..^1];

            var selectedRowline = await sr.ReadLineAsync() ?? throw new Exception("Expected selected row indicator line but file ended");

            if (!selectedRowline.Contains(TasFileConstants.SELECTED_ROW_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.SELECTED_ROW_INDICATOR} but got: {selectedRowline}");
            }

            segments = selectedRowline.Split(";");
            if (segments.Length != 4) throw new Exception($"Invalid selected row format: {selectedRowline}");
            if (!int.TryParse(segments[1], out int selectedRow) || !int.TryParse(segments[2], out int endRow))
            {
                throw new Exception($"Invalid selected row values: {segments[1]}, {segments[2]}");
            }

            var importIntoRowline = await sr.ReadLineAsync() ?? throw new Exception("Expected import into row indicator line but file ended");
            if (!importIntoRowline.Contains(TasFileConstants.IMPORT_INTO_ROW_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.IMPORT_INTO_ROW_INDICATOR} but got: {importIntoRowline}");
            }

            segments = importIntoRowline.Split(";");
            if (segments.Length != 2) throw new Exception($"Invalid import into row format: {importIntoRowline}");
            if (!int.TryParse(segments[1], out int importIntoRow))
            {
                throw new Exception($"Invalid import into row value: {segments[1]}");
            }

            var loggingLine = await sr.ReadLineAsync() ?? throw new Exception("Expected logging indicator line but file ended");
            if (!loggingLine.Contains(TasFileConstants.LOGGING_INDICATOR))
            {
                throw new Exception($"Expected {TasFileConstants.LOGGING_INDICATOR} but got: {loggingLine}");
            }
            segments = loggingLine.Split(";");
            if (segments.Length != 6) throw new Exception($"Invalid logging format: {loggingLine}");
            var printSavegame = segments[1].Equals("1");
            var printTech = segments[2].Equals("1");
            var printComments = segments[3].Equals("1");
            if (!int.TryParse(segments[4], out int environment))
            {
                throw new Exception($"Invalid environment value: {segments[4]}");
            }
            return new TasFileObject()
            {
                StepCollection = steps,
                Goal = goal,
                ScriptFolder = scriptFolder,
                SelectedRow = selectedRow,
                ImportIntoRow = importIntoRow,
                PrintComments = printComments,
                PrintSavegame = printSavegame,
                PrintTech = printTech,
                Environment = environment,
            };
        }

        private static Step ReadStep(string[] segments, GameData gameData)
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
                    StepType.Tech => GetTechName(segments, gameData),
                    StepType.Recipe => GetRecipeName(segments, gameData),
                    _ => GetItemName(segments, gameData),
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