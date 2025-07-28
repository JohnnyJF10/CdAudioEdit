

using System.ComponentModel;

namespace CdAudioLib.Model
{
    public class ListableTrAudio : ITrAudio, INotifyPropertyChanged
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public virtual int duration { get; set; }
        public virtual string FilePath { get; set; } = string.Empty;
        public virtual int offset { get; set; }
        public virtual int fileSize { get; set; }
        public virtual int sampleRate { get; set; }
        public virtual int channels { get; set; }
        public virtual bool IsOriginalTrAudio { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}