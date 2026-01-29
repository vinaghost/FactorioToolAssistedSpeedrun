using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace FactorioToolAssistedSpeedrun.Services
{
    public interface ICommandStack
    {
        bool CanRedo { get; }
        bool CanUndo { get; }
        bool IsLocked { get; }

        void Clear();
        void Lock();
        T? Push<T>() where T : ICommand;
        ICommand RedoPop();
        ICommand UndoPop();
        void Unlock();
    }
}