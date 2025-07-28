

namespace CdAudioLib.Model
{
    public interface ITrAudio
    {
        public string? Name { get; set; }
        public int duration { get; set; }
        public string FilePath { get; set; }
        public int offset { get; set; }
        public int fileSize { get; set; }
        public int sampleRate { get; set; }
        public int channels { get; set; }
        public bool IsOriginalTrAudio { get; set; }
    }
}
