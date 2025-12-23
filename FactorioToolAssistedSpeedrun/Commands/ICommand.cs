namespace FactorioToolAssistedSpeedrun.Commands
{
    public interface ICommand
    {
        void Execute();
    }

    public interface IAsyncCommand
    {
        Task Execute();
    }
}