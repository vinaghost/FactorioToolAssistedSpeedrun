using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Exceptions;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Models.Game;
using System.IO;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class TasFileResult
    {
        public List<Step> StepCollection { get; } = [];
        public List<Template> TemplateCollection { get; } = [];

        public string Goal { get; set; } = "";
        public string ScriptFolder { get; set; } = "";

        public int SelectedRow { get; set; }
        public int ImportIntoRow { get; set; }
        public bool PrintComments { get; set; } = false;
        public bool PrintSavegame { get; set; } = false;
        public bool PrintTech { get; set; } = false;

        public int Environment { get; set; } = 1;
    }

    public class ParseTasFileCommand : ICommand, ICommandResult<TasFileResult>
    {
        public required string FileName { get; init; }
        public required GameData GameData { get; init; }

        public TasFileResult Result { get; } = new();

        public void Execute()
        {
            using var sr = File.OpenText(FileName);
            var line = sr.ReadLine() ?? throw new TasFileParserException("Empty file");
            var totalStep = 0;
            if (line.Equals(TasFileConstants.TOTAL_STEPS_INDICATOR))
            {
                var totalStepsLine = sr.ReadLine() ?? throw new TasFileParserException("Expected total steps line");
                if (!int.TryParse(totalStepsLine, out totalStep))
                {
                    throw new TasFileParserException($"Invalid total steps value {totalStepsLine}");
                }
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected goal indicator line");
            if (line.Equals(TasFileConstants.GOAL_INDICATOR))
            {
                var goalLine = sr.ReadLine() ?? throw new TasFileParserException("Expected goal line");
                Result.Goal = goalLine;
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected steps indicator line");

            if (line.Equals(TasFileConstants.STEPS_INDICATOR))
            {
                line = sr.ReadLine();
                while (line is not null)
                {
                    if (line.Equals(TasFileConstants.TEMPLATES_INDICATOR))
                    {
                        break;
                    }

                    var segments = line.Split(';');

                    var step = ReadStep(segments);
                    step.Location = Result.StepCollection.Count + 1;
                    Result.StepCollection.Add(step);
                    line = sr.ReadLine();
                }
            }

            if (line is null) throw new TasFileParserException("Expected templates indicator line");

            if (line.Equals(TasFileConstants.TEMPLATES_INDICATOR))
            {
                line = sr.ReadLine();

                while (line is not null)
                {
                    if (line.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
                    {
                        break;
                    }
                    var segments = line.Split(';');
                    if (segments.Length < 10)
                    {
                        throw new TasFileParserException($"Invalid template format: {line}");
                    }
                    var name = segments[0];
                    var step = ReadStep(segments[1..10]);
                    var template = Template.FromStep(name, step);

                    step.Location = Result.TemplateCollection.Count(x => x.Name == name);
                    Result.TemplateCollection.Add(template);
                    line = sr.ReadLine();
                }
            }

            if (line is null) throw new TasFileParserException("Expected save file indicator line");
            if (line.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
            {
                var saveFileLine = sr.ReadLine() ?? throw new TasFileParserException("Expected save file line");
                _ = saveFileLine;
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected step folder indicator line");
            if (line.Equals(TasFileConstants.CODE_FILE_INDICATOR))
            {
                var codeFileLine = sr.ReadLine() ?? throw new TasFileParserException("Expected step folder line");
                Result.ScriptFolder = codeFileLine[..^1];
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected selected row indicator line");

            if (line.Contains(TasFileConstants.SELECTED_ROW_INDICATOR))
            {
                var segments = line.Split(";");
                if (segments.Length != 4) throw new TasFileParserException($"Invalid selected row format: {line}");
                if (int.TryParse(segments[1], out int startRow) && int.TryParse(segments[2], out int endRow))
                {
                    Result.SelectedRow = startRow;
                }
                else
                {
                    throw new TasFileParserException($"Invalid selected row values: {segments[1]}, {segments[2]}");
                }
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected import into row indicator line");
            if (line.Contains(TasFileConstants.IMPORT_INTO_ROW_INDICATOR))
            {
                var segments = line.Split(";");
                if (segments.Length != 2) throw new TasFileParserException($"Invalid import into row format: {line}");
                if (int.TryParse(segments[1], out int importIntoRow))
                {
                    Result.ImportIntoRow = importIntoRow;
                }
                else
                {
                    throw new TasFileParserException($"Invalid import into row value: {segments[1]}");
                }
            }

            line = sr.ReadLine() ?? throw new TasFileParserException("Expected logging indicator line");
            if (line.Contains(TasFileConstants.LOGGING_INDICATOR))
            {
                var segments = line.Split(";");
                if (segments.Length != 6) throw new TasFileParserException($"Invalid logging format: {line}");
                Result.PrintSavegame = segments[1].Equals("1");
                Result.PrintTech = segments[2].Equals("1");
                Result.PrintComments = segments[3].Equals("1");
                if (int.TryParse(segments[4], out int environment))
                {
                    Result.Environment = environment;
                }
                else
                {
                    throw new TasFileParserException($"Invalid environment value: {segments[4]}");
                }
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
                    if (ModifierTypeExtensions.Lookup.TryGetValue(modifierStr, out ModifierType modifierValue))
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
                    StepType.Tech => GetTechName(segments, GameData),
                    StepType.Recipe => GetRecipeName(segments, GameData),
                    _ => GetItemName(segments, GameData),
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