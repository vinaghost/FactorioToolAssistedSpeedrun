using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Exceptions;
using FactorioToolAssistedSpeedrun.Models.Game;
using System.IO;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class TasFileResult
    {
        public List<Step> StepCollection { get; } = [];
        public List<Template> TemplateCollection { get; } = [];

        public string Goal { get; set; } = "";
        public string ModsFolder { get; set; } = "";

        public int SelectedRow { get; set; }
        public int ImportIntoRow { get; set; }
        public bool PrintMessage { get; set; } = false;
        public bool PrintSavegame { get; set; } = false;
        public bool PrintTech { get; set; } = false;

        public int Environment { get; set; } = 1;
    }

    public class ParseTasFileCommand : ICommand, ICommandResult<TasFileResult>
    {
        public required string FileName { get; init; }
        public required GameData GameData { get; init; }

        public TasFileResult Result { get; } = new();

        public async Task Execute()
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

            var itemNameDictionary = new Dictionary<string, string>();

            foreach (var item in GameData.Items!)
            {
                if (item.Name is null || item.HumanizeName is null) continue;
                itemNameDictionary[item.HumanizeName] = item.Name;
            }

            var techNameDictionary = new Dictionary<string, string>();
            foreach (var tech in GameData.Technologies!)
            {
                if (tech.Name is null || tech.HumanizeName is null) continue;
                techNameDictionary[tech.HumanizeName] = tech.Name;
            }

            var recipeNameDictionary = new Dictionary<string, string>();
            foreach (var recipe in GameData.Recipes!)
            {
                if (recipe.Name is null || recipe.HumanizeName is null) continue;
                recipeNameDictionary[recipe.HumanizeName] = recipe.Name;
            }

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

                    if (segments.Length != 10)
                    {
                        throw new TasFileParserException($"Invalid step format: {line}");
                    }

                    var itemName = itemNameDictionary.TryGetValue(segments[4], out string? value) ? value : segments[4];
                    itemName = techNameDictionary.TryGetValue(itemName, out string? techValue) ? techValue : itemName;
                    itemName = recipeNameDictionary.TryGetValue(itemName, out string? recipeValue) ? recipeValue : itemName;

                    var isSkip = segments[8].Contains("skip");
                    var isSplit = segments[8].Contains("split");
                    segments[8] = segments[8].Replace("skip", "").Replace("split", "");
                    var modifierSegments = segments[8].Split(',');
                    segments[8] = string.Join(',', modifierSegments.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));

                    var step = new Step()
                    {
                        Id = Result.StepCollection.Count + 1,
                        Type = segments[0],
                        X = double.TryParse(segments[1], out double x) ? x : 0,
                        Y = double.TryParse(segments[2], out double y) ? y : 0,
                        Amount = int.TryParse(segments[3], out int amount) ? amount : 0,
                        Item = itemName,
                        Orientation = segments[5],
                        Comment = segments[6],
                        Color = segments[7],
                        Modifier = segments[8],
                        IsSkip = isSkip,
                        IsSplit = isSplit
                    };

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
                    if (segments.Length != 11)
                    {
                        throw new TasFileParserException($"Invalid template format: {line}");
                    }
                    var template = new Template()
                    {
                        Id = Result.TemplateCollection.Count(x => x.Name == segments[0]) + 1,
                        Name = segments[0],
                        Type = segments[1],
                        X = double.TryParse(segments[2], out double x) ? x : 0,
                        Y = double.TryParse(segments[3], out double y) ? y : 0,
                        Amount = int.TryParse(segments[4], out int amount) ? amount : 0,
                        Item = segments[5],
                        Orientation = segments[6],
                        Comment = segments[7],
                        Color = segments[8],
                        Modifier = segments[9],
                    };

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
                Result.ModsFolder = codeFileLine[..^1];
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
                Result.PrintMessage = segments[3].Equals("1");
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
    }
}