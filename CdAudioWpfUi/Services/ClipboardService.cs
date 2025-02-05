using CdAudioLib.Abstraction;

using Clipboard = System.Windows.Clipboard;

namespace CdAudioWpfUi.Services
{
    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }

        public string GetText()
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }

        public void SetData(string format, object data)
        {
            Clipboard.SetData(format, data);
        }

        public object GetData(string format)
        {
            return Clipboard.ContainsData(format) ? Clipboard.GetData(format) : null;
        }

        public bool ContainsText()
        {
            return Clipboard.ContainsText();
        }

        public bool ContainsData(string format)
        {
            return Clipboard.ContainsData(format);
        }
    }
}
