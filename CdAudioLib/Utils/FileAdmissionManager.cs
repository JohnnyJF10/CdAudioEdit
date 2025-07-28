

using CdAudioLib.Model;
using NAudio.Wave;
using NVorbis;
using System.Text;

namespace CdAudioLib.Utils
{
    public class FileAdmissionManager
    {
        public bool StripExtension = true;

        public TrAudio GetTrAudioFromFile(string fileName)
        {
            TrAudio AdmittedTrAudio = new();

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            int duration;
            int nChannels;
            int sampleRate;
            bool isOriginalTrAudio = false;

            switch (extension)
            {
                case ".mp3":
                    using (Mp3FileReader reader = new Mp3FileReader(fileName))
                    {
                        duration = (int)reader.TotalTime.TotalSeconds;
                        WaveFormat waveFormat = reader.WaveFormat;

                        sampleRate = waveFormat.SampleRate;
                        nChannels = waveFormat.Channels;
                    }
                    break;

                case ".ogg":
                    using (VorbisReader vorbis = new VorbisReader(fileName))
                    {
                        duration = (int)vorbis.TotalTime.TotalSeconds;
                        sampleRate = vorbis.SampleRate;
                        nChannels = vorbis.Channels;
                    }
                    break;

                case ".wav":
                    using (WaveFileReader reader = new WaveFileReader(fileName))
                    {
                        duration = (int)reader.TotalTime.TotalSeconds;
                        WaveFormat waveFormat = reader.WaveFormat;

                        sampleRate = waveFormat.SampleRate;
                        nChannels = waveFormat.Channels;
                    }
                    isOriginalTrAudio = CheckWave(fileName);
                    break;

                default:
                    throw new NotSupportedException("File format not supported");
            }
            if (StripExtension && extension != ".wav")
                AdmittedTrAudio.Name = Path.GetFileNameWithoutExtension(fileName);
            else
                AdmittedTrAudio.Name = Path.GetFileName(fileName);
            AdmittedTrAudio.offset = 0;
            AdmittedTrAudio.FilePath = fileName;
            AdmittedTrAudio.fileSize = (int)new FileInfo(fileName).Length;
            AdmittedTrAudio.sampleRate = sampleRate;
            AdmittedTrAudio.channels = nChannels;
            AdmittedTrAudio.duration = duration;
            AdmittedTrAudio.IsOriginalTrAudio = isOriginalTrAudio;
            return AdmittedTrAudio;
        }

        private bool CheckWave(string filename)
        {
            using (var reader = new BinaryReader(File.OpenRead(filename)))
            {
                uint extraSize = 0;
                uint factValue = 0;

                // 'RIFF'
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "RIFF") return false;

                /*fileSize = */
                reader.ReadUInt32();

                // 'WAVE'
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE") return false;

                for (int k = 0; k < 20; k++)
                {
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    uint blockSize = reader.ReadUInt32();

                    // 'fmt '
                    if (name == "fmt ")
                    {
                        var fmtSize = blockSize;

                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_FORMAT) return false;
                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_NUM_CHANNELS) return false;
                        if (reader.ReadUInt32() != CdAudioConstants.TR_AUDIO_SAMPLE_RATE) return false;
                        if (reader.ReadUInt32() != CdAudioConstants.TR_AUDIO_BYTE_RATE) return false;
                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_BLOCK_ALIGNMENT) return false;
                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_BITS_PER_SAMPLE) return false;

                        extraSize = reader.ReadUInt16();

                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_SAMPLES_PER_BLOCK) return false;
                        if (reader.ReadUInt16() != CdAudioConstants.TR_AUDIO_COEFFICIENT_COUNT) return false;


                        for (int i = 0; i < 14; i++)
                            reader.ReadInt16();

                    }
                    else if (name == "fact")
                    {
                        if (blockSize != CdAudioConstants.TR_AUDIO_FACT_SIZE)
                            return false;
                        else
                            factValue = reader.ReadUInt32();
                        break;
                    }
                    else
                    {
                        reader.BaseStream.Position += blockSize;
                    }
                }
                reader.Close();

                return true;

            }
        }

    }
}
