namespace FactorioToolAssistedSpeedrun.Commands.Features
{
    public static class UpdateSettingCommand
    {
        public static async Task Execute(string projectDataFile, string setting, string value)
        {
            using var context = new ProjectDbContext(projectDataFile);
            // ConfigureAwait(false) avoids deadlocking when callers block on this task from the UI thread.
            await Task.Run(() => context.UpdateSetting(setting, value)).ConfigureAwait(false);
        }
    }
}