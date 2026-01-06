namespace FactorioToolAssistedSpeedrun.Commands.UI
{
    public interface ICommandResult<T> where T : class
    {
        T Result { get; }
    }
}