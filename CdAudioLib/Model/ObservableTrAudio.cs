/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using System.ComponentModel;
using System.Runtime.Serialization;

namespace CdAudioLib.Model
{
    [Serializable]
    public class ObservableTrAudio : ITrAudio, INotifyPropertyChanged, ICloneable, IEquatable<ITrAudio>, ISerializable
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

        private int _duration;
        public int duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged(nameof(duration));
                    OnPropertyChanged(nameof(DurationString));
                }
            }
        }

        public string DurationString
        {
            get => $"{_duration / 60}:{_duration % 60:D2}";
        }

        private string _filePath = String.Empty;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                    OnPropertyChanged(nameof(FileType));
                    OnPropertyChanged(nameof(FileStatus));
                }
            }
        }

        public string FileType  
            => Path.GetExtension(FilePath).ToLower() switch
            {
                ".wad" => "MS ADPCM",
                ".wav" => "PCM",
                ".mp3" => "MP3",
                ".ogg" => "OGG",
                _ => "other"
            };

        public string FileStatus 
            => Path.GetExtension(FilePath) switch
            {
                ".wad" => "Included",
                _ => "External"
            };

        private int _offset;
        public int offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    OnPropertyChanged(nameof(offset));
                }
            }
        }

        private int _fileSize;
        public int fileSize
        {
            get => _fileSize;
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    OnPropertyChanged(nameof(fileSize));
                    OnPropertyChanged(nameof(FileSizeString));
                }
            }
        }

        public string FileSizeString
            => (fileSize / 1024.0).ToString("N0") + " KB";

        private int _channels;
        public int channels
        {
            get => _channels;
            set
            {
                if (_channels != value)
                {
                    _channels = value;
                    OnPropertyChanged(nameof(channels));
                    OnPropertyChanged(nameof(ChannelsString));
                }
            }
        }

        public string? ChannelsString 
            => channels switch { 1 => "Mono", _ => "Stereo" };

        private int _sampleRate;
        public int sampleRate
        {
            get => _sampleRate;
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    OnPropertyChanged(nameof(sampleRate));
                    OnPropertyChanged(nameof(SampleRateString));
                }
            }
        }

        public string SampleRateString 
            => $"{sampleRate / 1000}.{sampleRate % 1000:D3} Hz";

        private bool _isOriginalTrAudio;

        public bool IsOriginalTrAudio 
        { 
            get => _isOriginalTrAudio; 
            set
            {
                if (_isOriginalTrAudio != value)
                {
                    _isOriginalTrAudio = value;
                    OnPropertyChanged(nameof(IsOriginalTrAudio));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableTrAudio(ITrAudio trAudio)
        {
            Name = trAudio.Name;
            duration = trAudio.duration;
            FilePath = trAudio.FilePath;
            offset = trAudio.offset;
            fileSize = trAudio.fileSize;
            sampleRate = trAudio.sampleRate;
            channels = trAudio.channels;
            IsOriginalTrAudio = trAudio.IsOriginalTrAudio;
        }

        protected ObservableTrAudio(SerializationInfo info, StreamingContext context)
        {
            FilePath = info.GetString(nameof(FilePath)) ?? String.Empty;
            duration = info.GetInt32(nameof(duration));
            fileSize = info.GetInt32(nameof(fileSize));
            Name = info.GetString(nameof(Name));
            offset = info.GetInt32(nameof(offset));
            sampleRate = info.GetInt32(nameof(sampleRate));
            channels = info.GetInt32(nameof(channels));
            IsOriginalTrAudio = info.GetBoolean(nameof(IsOriginalTrAudio));
        }

        public object Clone()
        {
            return new ObservableTrAudio() 
            { 
                FilePath = this.FilePath, 
                duration =   this.duration, 
                fileSize = this.fileSize,
                Name =     this.Name,
                offset = this.offset,
                sampleRate = this.sampleRate,
                channels = this.channels,
                IsOriginalTrAudio = this.IsOriginalTrAudio
            };
        }

        public bool Equals(ITrAudio? other)
        {
            if (other == null) return false;
            return 
                (
                    other.FilePath == this.FilePath &&
                    other.duration   == this.duration   &&
                    other.fileSize == this.fileSize &&
                    other.Name     == this.Name     &&
                    other.offset == this.offset &&
                    other.sampleRate == this.sampleRate &&
                    other.channels == this.channels &&
                    other.IsOriginalTrAudio == this.IsOriginalTrAudio
                );
        }

        public void UpdateFromOther(ITrAudio? other)
        {
            if (other == null) return;
            this.Name =     other.Name;
            this.duration =   other.duration;
            this.FilePath = other.FilePath;
            this.offset =   other.offset;
            this.fileSize = other.fileSize;
            this.sampleRate = other.sampleRate;
            this.channels = other.channels;
            this.IsOriginalTrAudio = other.IsOriginalTrAudio;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name), this.Name);
            info.AddValue(nameof(duration), this.duration);
            info.AddValue(nameof(FilePath), this.FilePath);
            info.AddValue(nameof(offset), this.offset);
            info.AddValue(nameof(fileSize), this.fileSize);
            info.AddValue(nameof(sampleRate), this.sampleRate);
            info.AddValue(nameof(channels), this.channels);
            info.AddValue(nameof(IsOriginalTrAudio),this.IsOriginalTrAudio);
        }
        public ObservableTrAudio()
        {
            // Default constructor implementation
        }
    }
}
