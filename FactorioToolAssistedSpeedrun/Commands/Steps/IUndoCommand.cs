namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IUndoCommand
    {
        void Commit();

        void Rollback();
    }
}