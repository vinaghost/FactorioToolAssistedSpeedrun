using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Commands.UI
{
    public static class UpdateSettingCommand
    {
        public static async Task Execute(string projectDataFile, string setting, string value)
        {
            await using var context = new ProjectDbContext(projectDataFile);
            await context.Settings
                .Where(s => s.Key == setting)
                .ExecuteUpdateAsync(s => s.SetProperty(s => s.Value, value));
        }
    }
}