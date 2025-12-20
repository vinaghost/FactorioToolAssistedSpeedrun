namespace FactorioToolAssistedSpeedrun.Commands
{
    public interface ICommandResult<T> where T : class
    {
        T Result { get; }
    }
}