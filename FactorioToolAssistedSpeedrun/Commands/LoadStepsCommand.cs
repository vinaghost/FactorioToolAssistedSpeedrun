using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class LoadStepsCommand : ICommand, ICommandResult<List<Step>>
    {
        public required string ProjectDataFile { get; init; }
        public List<Step> Result { get; private set; } = [];

        public void Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            Result = [.. context.Steps.AsNoTracking().OrderBy(x => x.Location)];
        }
    }
}