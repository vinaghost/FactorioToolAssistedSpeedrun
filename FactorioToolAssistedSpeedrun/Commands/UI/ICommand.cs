namespace FactorioToolAssistedSpeedrun.Commands.UI
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