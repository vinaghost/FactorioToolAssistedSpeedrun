using FactorioToolAssistedSpeedrun.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class UpdateSettingCommand : ICommand
    {
        public required string ProjectDataFile { get; init; }
        public required string Setting { get; init; }
        public required string Value { get; init; }

        public void Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            context.Settings
                .Where(s => s.Key == Setting)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, Value));
        }
    }
}