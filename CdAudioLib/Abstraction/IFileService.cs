namespace CdAudioLib.Abstraction
{
    public interface IFileService
    {
        string SelectedPath { get; set; }
        bool OpenFileDialog(FileTypes types, string? InitDir = null, string? Title = null);
        bool SaveFileDialog(FileTypes types, string? InitDir = null, string? Title = null);
        bool SelectFolderDialog(string? InitDir = null, string? Title = null);
    }

    [Flags]
    public enum FileTypes
    {
        None = 0,
        CdAudioWad = 1,
        Wav = 2,
        Mp3 = 4,
        Ogg = 8,
    }
}
