/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Model;
using System.Text;
using CdAudioLib.Utils;

namespace CdAudioLib.CdAudio
{
    public partial class CdAudioFileManager
    {

        public List<T> Read<T>(string fileName) where T : ITrAudio, new()
        {
            var OutputList = new List<T>();
            long oldPos;
            int factValue = 0;
            T trAudio;
            using (var reader = new BinaryReader(File.OpenRead(fileName)))
            {

                for (int i = 0; i < CdAudioConstants.CD_AUDIO_CAPACITY; i++)
                {
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(CdAudioConstants.CD_AUDIO_NAME_LEN));
                    name = name.Replace("\0", "");

                    trAudio = new T
                    {
                        Name = name,
                        fileSize = reader.ReadInt32(),
                        offset = reader.ReadInt32(),
                        FilePath = fileName,
                    };

                    //Check if MS-ADPCM WAVE
                    if (String.IsNullOrEmpty(name) && trAudio.fileSize == 0 && trAudio.offset == 0)
                    {
                        trAudio.Name = null;
                    }
                    if (name != "")
                    {
                        oldPos = reader.BaseStream.Position;

                        reader.BaseStream.Position = trAudio.offset;

                        try
                        {
                            (int format, trAudio.channels, trAudio.sampleRate, factValue) = GetMsAdpcmParameters(reader);
                        }
                        catch (Exception ex)
                        {
                            throw new FileLoadException($"Error reading file {fileName}.", ex);
                        }

                        trAudio.duration = (int)(factValue / trAudio.sampleRate);

                        reader.BaseStream.Position = oldPos;
                    }

                    OutputList.Add(trAudio);
                }
                reader.Close();
            }
            return OutputList;
        }

        private (int format, int channels, int sampleRate, int factValue) GetMsAdpcmParameters(BinaryReader reader)
        {
            int format = 0;
            int channels = 0;
            int sampleRate = 0;
            int factValue = -1;

            int dataSize = 0;

            // 'RIFF'
            string RiffMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (RiffMagic != CdAudioConstants.RIFF_WORD)
                throw new Exception("Error, no RIFF word found. Wrong file format?");

            reader.ReadUInt32();

            // 'WAVE'
            string WaveMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (WaveMagic != CdAudioConstants.WAVE_WORD)
                throw new Exception("Error, no RIFF word found. Wrong file format?");

            for (int k = 0; k < 20; k++)
            {
                string name = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint blockSize = reader.ReadUInt32();

                // 'fmt '
                if (name == CdAudioConstants.FMT_WORD)
                {
                    format = reader.ReadUInt16();
                    if (format != CdAudioConstants.TR_AUDIO_FORMAT)
                        throw new Exception($"WAVE format {format} is not original TrAudio Wave Format.");
                    channels = reader.ReadUInt16();
                    if (channels != CdAudioConstants.TR_AUDIO_NUM_CHANNELS)
                        throw new Exception($"WAVE channels num {channels} is not original TrAudio Wave channels num (2).");
                    sampleRate = (int)reader.ReadUInt32();
                    if (sampleRate != CdAudioConstants.TR_AUDIO_SAMPLE_RATE)
                        throw new Exception($"WAVE sample rate {sampleRate} is not original TrAudio Wave sample rate (44100Hz).");
                    reader.ReadUInt32();
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    reader.BaseStream.Position += blockSize - CdAudioConstants.PCM_USUAL_FMT_SIZE;

                }
                else if (name == CdAudioConstants.FACT_WORD)
                {
                    factValue = (int)reader.ReadUInt32();
                }
                else if (name == CdAudioConstants.DATA_WORD)
                {
                    dataSize = (int)blockSize;
                    break;
                }
                else
                {
                    reader.BaseStream.Position += blockSize;
                }

            }
            if (factValue < 0)
            {
                factValue = dataSize / CdAudioConstants.TR_AUDIO_BLOCK_ALIGNMENT * CdAudioConstants.TR_AUDIO_SAMPLES_PER_BLOCK;
            }
            return (format, channels, sampleRate, factValue);

        }
    }
}
