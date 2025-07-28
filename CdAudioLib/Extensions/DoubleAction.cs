using CdAudioLib.Abstraction;

namespace CdAudioLib.Extensions
{
    internal class DoubleAction : IUndoableAction
    {
        private readonly Action _undo1;
        private readonly Action _redo1;
        private readonly Action _undo2;
        private readonly Action _redo2;

        public DoubleAction(Action undo1, Action redo1, Action undo2, Action redo2)
        {
            _undo1 = undo1;
            _redo1 = redo1;
            _undo2 = undo2;
            _redo2 = redo2;
        }

        public void Undo() { _undo1(); _undo2(); }
        public void Redo() { _redo1(); _redo2(); }
    }
}
