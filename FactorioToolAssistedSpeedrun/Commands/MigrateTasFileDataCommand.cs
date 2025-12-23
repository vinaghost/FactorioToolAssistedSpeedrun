using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class MigrateTasFileDataCommand : ICommand
    {
        public required string ProjectDataFile { get; init; }
        public required TasFileResult TasFileResult { get; init; }

        public void Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);

            context.Database.EnsureDeletedAsync();
            context.Database.EnsureCreatedAsync();
            context.SetupTriggers();

            context.Steps.AddRange(TasFileResult.StepCollection);
            context.Templates.AddRange(TasFileResult.TemplateCollection);
            context.Settings.Add(new Setting
            {
                Key = SettingConstants.ScriptFolder,
                Value = TasFileResult.ScriptFolder
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.SelectedRow,
                Value = TasFileResult.SelectedRow.ToString()
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.ImportIntoRow,
                Value = TasFileResult.ImportIntoRow.ToString()
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.PrintMessage,
                Value = TasFileResult.PrintComments ? "1" : "0"
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.PrintSavegame,
                Value = TasFileResult.PrintSavegame ? "1" : "0"
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.PrintTech,
                Value = TasFileResult.PrintTech ? "1" : "0"
            });

            context.Settings.Add(new Setting
            {
                Key = SettingConstants.Environment,
                Value = TasFileResult.Environment.ToString()
            });

            context.SaveChanges();
        }
    }
}