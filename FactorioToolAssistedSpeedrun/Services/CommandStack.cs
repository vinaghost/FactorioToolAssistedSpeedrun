using FactorioToolAssistedSpeedrun.Commands.Steps;

namespace FactorioToolAssistedSpeedrun.Services
{
    public class CommandStack
    {
        private readonly Stack<IUndoCommand> _undoStack = new();
        private readonly Stack<IUndoCommand> _redoStack = new();
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

        public IUndoCommand UndoPop()
        {
            var command = _undoStack.Pop();
            _redoStack.Push(command);
            return command;
        }

        public IUndoCommand RedoPop()
        {
            var command = _redoStack.Pop();
            _undoStack.Push(command);
            return command;
        }

        public void Push(IUndoCommand command)
        {
            if (_lock)
                return;
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}