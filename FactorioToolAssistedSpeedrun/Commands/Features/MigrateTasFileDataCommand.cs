using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Commands.Features
{
    public static class MigrateTasFileDataCommand
    {
        public async static Task Execute(string projectDataFile, TasFileObject tasFile)
        {
            using var context = new ProjectDbContext(projectDataFile);

            await Task.Run(context.EnsureDeleted);
            await Task.Run(context.EnsureCreated);
            await Task.Run(context.SetupTriggers);

            await Task.Run(() => context.RunInTransaction(() =>
            {
                context.InsertSteps(tasFile.StepCollection);
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.ScriptFolder,
                    Value = tasFile.ScriptFolder
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.SelectedRow,
                    Value = tasFile.SelectedRow.ToString()
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.ImportIntoRow,
                    Value = tasFile.ImportIntoRow.ToString()
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintMessage,
                    Value = tasFile.PrintComments ? "1" : "0"
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintSavegame,
                    Value = tasFile.PrintSavegame ? "1" : "0"
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.PrintTech,
                    Value = tasFile.PrintTech ? "1" : "0"
                });

                context.AddSetting(new Setting
                {
                    Key = SettingConstants.Environment,
                    Value = tasFile.Environment.ToString()
                });
            }));
        }
    }
}