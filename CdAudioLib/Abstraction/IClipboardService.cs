namespace CdAudioLib.Abstraction
{
    public interface IClipboardService
    {
        void SetText(string text);
        string GetText();
        void SetData(string format, object data);
        object GetData(string format);
        bool ContainsText();
        bool ContainsData(string format);
    }
}
