using CdAudioLib.Abstraction;

namespace CdAudioLib.Extensions
{
    public class UndoRedoManager
    {
        private bool IsUndoRedo = false;

        private readonly Stack<IUndoableAction> _undoStack = new();
        private readonly Stack<IUndoableAction> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event Action? StateChanged;

        public void ExecuteChange(Action doAction, Action undoAction)
        {
            if (IsUndoRedo) return;
            SingleAction singleAction = new(undoAction, doAction);
            doAction.Invoke();
            _undoStack.Push(singleAction);
            _redoStack.Clear(); 
            OnStateChanged();
        }

        public void ExecuteChange(Action doAction1, Action undoAction1, Action doAction2, Action undoAction2)
        {
            if (IsUndoRedo) return;
            DoubleAction doubleAction = new(undoAction1, doAction1, undoAction2, doAction2);
            doAction1.Invoke();
            doAction2.Invoke();
            _undoStack.Push(doubleAction);
            _redoStack.Clear(); 
            OnStateChanged();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            IsUndoRedo = true;

            var undoAction = _undoStack.Pop();
            undoAction.Undo();
            _redoStack.Push(undoAction);
            OnStateChanged();

            IsUndoRedo = false;
        }

        public void Redo()
        {
            if (!CanRedo) return;

            IsUndoRedo = true;

            var redoAction = _redoStack.Pop();
            redoAction.Redo();
            _undoStack.Push(redoAction);
            OnStateChanged();

            IsUndoRedo = false;
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }
    }
}
