using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class SettingsResult
    {
        public bool PrintComments { get; set; }
        public bool PrintSavegame { get; set; }
        public bool PrintTech { get; set; }
        public bool DebugMode { get; set; }
        public bool DevelopmentMode { get; set; }
        public bool ProductionMode { get; set; }
        public string ScriptFolder { get; set; } = "";
    }

    public class LoadSettingsCommand : ICommand, ICommandResult<SettingsResult>
    {
        public required string ProjectDataFile { get; init; }

        public SettingsResult Result { get; } = new();

        public void Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);

            var printMessageSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintMessage);
            if (printMessageSetting is not null)
            {
                Result.PrintComments = printMessageSetting.Value == "1";
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.PrintMessage,
                    Value = "0"
                });
            }

            var printSavegameSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintSavegame);
            if (printSavegameSetting is not null)
            {
                Result.PrintSavegame = printSavegameSetting.Value == "1";
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.PrintSavegame,
                    Value = "1"
                });
            }

            var printTechSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintTech);
            if (printTechSetting is not null)
            {
                Result.PrintTech = printTechSetting.Value == "1";
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.PrintTech,
                    Value = "1"
                });
            }

            var environmentSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.Environment);
            if (environmentSetting is not null)
            {
                switch (environmentSetting.Value)
                {
                    case "0":
                        Result.DebugMode = true;
                        break;

                    case "1":
                        Result.DevelopmentMode = true;
                        break;

                    case "2":
                        Result.ProductionMode = true;
                        break;

                    default:
                        Result.DevelopmentMode = true;
                        break;
                }
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.Environment,
                    Value = "1"
                });
            }

            var modsFolderSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.ScriptFolder);
            if (modsFolderSetting is not null)
            {
                Result.ScriptFolder = modsFolderSetting.Value;
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.ScriptFolder,
                    Value = ""
                });
            }
        }
    }
}