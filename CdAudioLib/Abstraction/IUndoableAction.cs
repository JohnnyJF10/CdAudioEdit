namespace CdAudioLib.Abstraction
{
    public interface IUndoableAction
    {
        void Undo();
        void Redo();
    }
}
