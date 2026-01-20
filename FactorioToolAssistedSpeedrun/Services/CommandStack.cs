using FactorioToolAssistedSpeedrun.Commands.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioToolAssistedSpeedrun.Services
{
    public class CommandStack
    {
        private readonly IServiceProvider _services;

        public CommandStack(IServiceProvider services)
        {
            _services = services;
        }

        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private bool _lock = false;

        public void Lock()
        {
            _lock = true;
        }

        public void Unlock()
        {
            _lock = false;
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public ICommand UndoPop()
        {
            var command = _undoStack.Pop();
            _redoStack.Push(command);
            return command;
        }

        public ICommand RedoPop()
        {
            var command = _redoStack.Pop();
            _undoStack.Push(command);
            return command;
        }

        public T? Push<T>() where T : ICommand
        {
            if (_lock)
                return default;
            var command = _services.GetRequiredService<T>();
            _undoStack.Push(command);
            _redoStack.Clear();
            return command;
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public bool IsLocked => _lock;
    }
}