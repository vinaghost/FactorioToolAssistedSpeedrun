namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public interface ICommand
    {
        void Commit(bool ignoreUI = false);

        void Rollback();
    }
}