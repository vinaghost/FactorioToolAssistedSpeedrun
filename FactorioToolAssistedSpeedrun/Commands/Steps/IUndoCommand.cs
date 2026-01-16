namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface IUndoCommand
    {
        void Commit(bool ignoreUI = false);

        void Rollback();
    }
}