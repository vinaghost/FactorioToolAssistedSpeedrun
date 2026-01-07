using FactorioToolAssistedSpeedrun.Commands.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Services
{
    public class CommandStack
    {
        private readonly Stack<IStepCommand> _undoStack = new();
        private readonly Stack<IStepCommand> _redoStack = new();

        public IStepCommand UndoPop()
        {
            var command = _undoStack.Pop();
            _redoStack.Push(command);
            return command;
        }

        public IStepCommand RedoPop()
        {
            var command = _redoStack.Pop();
            _undoStack.Push(command);
            return command;
        }

        public void Push(IStepCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}