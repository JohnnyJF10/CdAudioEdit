using CdAudioLib.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdAudioLib.Extensions
{
    public class SingleAction : IUndoableAction
    {
        private readonly Action _undo;
        private readonly Action _redo;

        public SingleAction(Action undo, Action redo)
        {
            _undo = undo;
            _redo = redo;
        }

        public void Undo() => _undo();
        public void Redo() => _redo();
    }
}
