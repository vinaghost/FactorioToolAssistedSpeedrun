namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IDatabaseCommand
    {
        void DatabaseCommit(ProjectDbContext context);

        void DatabaseRollback(ProjectDbContext context);
    }
}