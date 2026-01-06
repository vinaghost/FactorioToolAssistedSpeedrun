using FactorioToolAssistedSpeedrun.Commands.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Services
{
    public class CommandStack
    {
        private readonly Stack<IStepCommand> _undoStack = new();

        public void Clear() => _undoStack.Clear();

        public IStepCommand Pop() => _undoStack.Pop();

        public void Push(IStepCommand command) => _undoStack.Push(command);

        public bool CanUndo => _undoStack.Count > 0;
    }
}