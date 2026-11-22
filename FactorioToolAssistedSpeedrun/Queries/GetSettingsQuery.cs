using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Queries
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

    public class GetSettingsQuery
    {
        public required string ProjectDataFile { get; init; }

        public SettingsResult Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);

            var printComments = false;
            var printMessageSetting = context.GetSetting(SettingConstants.PrintMessage);
            if (printMessageSetting is not null)
            {
                printComments = printMessageSetting.Value == "1";
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintMessage,
                    Value = "0"
                });
            }
            var printSavegame = false;
            var printSavegameSetting = context.GetSetting(SettingConstants.PrintSavegame);
            if (printSavegameSetting is not null)
            {
                printSavegame = printSavegameSetting.Value == "1";
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintSavegame,
                    Value = "1"
                });
            }

            var printTech = false;
            var printTechSetting = context.GetSetting(SettingConstants.PrintTech);
            if (printTechSetting is not null)
            {
                printTech = printTechSetting.Value == "1";
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintTech,
                    Value = "1"
                });
            }

            var debugMode = false;
            var developmentMode = false;
            var productionMode = false;
            var environmentSetting = context.GetSetting(SettingConstants.Environment);
            if (environmentSetting is not null)
            {
                switch (environmentSetting.Value)
                {
                    case "0":
                        debugMode = true;
                        break;

                    case "1":
                        developmentMode = true;
                        break;

                    case "2":
                        productionMode = true;
                        break;

                    default:
                        developmentMode = true;
                        break;
                }
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.Environment,
                    Value = "1"
                });
            }

            var scriptFolder = "";
            var modsFolderSetting = context.GetSetting(SettingConstants.ScriptFolder);
            if (modsFolderSetting is not null)
            {
                scriptFolder = modsFolderSetting.Value;
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.ScriptFolder,
                    Value = ""
                });
            }

            return new SettingsResult
            {
                PrintComments = printComments,
                PrintSavegame = printSavegame,
                PrintTech = printTech,
                DebugMode = debugMode,
                DevelopmentMode = developmentMode,
                ProductionMode = productionMode,
                ScriptFolder = scriptFolder,
            };
        }
    }
}