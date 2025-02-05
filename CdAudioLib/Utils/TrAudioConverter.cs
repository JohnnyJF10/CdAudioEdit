/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using CdAudioLib.Abstraction;
using CdAudioLib.Extensions;
using CdAudioLib.Model;
using CdAudioLib.WaveStreams;

using NAudio.Wave;
using NAudio.Lame;

using System.Globalization;


namespace CdAudioLib.Utils
{
    public class TrAudioConverter
    {


        private int bytesRead = 0;

        //Buffers
        private byte[] tempBuffer = new byte[2 * CdAudioConstants.TR_AUDIO_NUM_OF_REQ_PCM_SHORTS_PER_BLOCK];
        private float[] oggBuffer = new float[0x1000];
        private short[] pcmSampleBuffer = new short[CdAudioConstants.TR_AUDIO_NUM_OF_REQ_PCM_SHORTS_PER_BLOCK];
        private byte[] adpcmSampleBuffer = new byte[CdAudioConstants.TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK];

        //Streams
        private WaveStream? waveStream;

        //Encoders
        private MsAdpcm.Encoder encoder = new MsAdpcm.Encoder();

        //Properties
        private string _exportDir;
        public string ExportDir 
        {
            get => _exportDir;
            set
            {
                _exportDir = value;
                EnsureFolderExists(_exportDir);
            }
        }

        public int numOfAdpcmTestSamples
        {
            get => encoder.numOfAdpcmTestSamples;
            set => encoder.numOfAdpcmTestSamples = value;
        }
        public int resamplingQuality { get; set; } = 60;

        public TrAudioConverter(string exportDir)
        {
            _exportDir = exportDir;
            EnsureFolderExists(_exportDir);
        }

        public void ConvertForCdAudio(string fileName, BinaryWriter writer, out int factValue)
        {
            factValue = 0;
            string extension = Path.GetExtension(fileName).ToLower(CultureInfo.InvariantCulture);

            waveStream = extension switch
            {
                ".mp3" => new Mp3FileReader(fileName),
                ".ogg" => new VorbisFileStream(fileName, oggBuffer),
                ".wav" => new WaveFileReader(fileName),
                _ => throw new NotSupportedException("Unsupported file format")
            };

            using (waveStream)
            {
                ConvertAndWrite(waveStream, writer, out factValue);
            }
        }

        public void QuickConvert(string InputFileName)
        {
            string OutputFilePath = _exportDir + Path.GetFileNameWithoutExtension(InputFileName) + ".wav";

            int factValue = 0;
            string extension = Path.GetExtension(InputFileName).ToLower(CultureInfo.InvariantCulture);
            waveStream = extension switch
            {
                ".mp3" => new Mp3FileReader(InputFileName),
                ".ogg" => new VorbisFileStream(InputFileName, oggBuffer),
                ".wav" => new WaveFileReader(InputFileName),
                _ => throw new NotSupportedException("Unsupported file format")
            };

            using (waveStream)
            using (var writer = new BinaryWriter(File.OpenWrite(OutputFilePath)))
            {
                ConvertAndWrite(waveStream, writer, out factValue);
                writer.Close();
            }
        }

        public void Export(ITrAudio trAudio, ExportAudioFormat exportAudioFormat = ExportAudioFormat.WAV, string? OutputFilePath = null)
        {
            if (OutputFilePath == null)
            {
                OutputFilePath = _exportDir + trAudio.Name;
            }
            else
            {
                string extension = Path.GetExtension(OutputFilePath).ToLower(CultureInfo.InvariantCulture);
                exportAudioFormat = extension switch
                {
                    ".wav" => ExportAudioFormat.WAV,
                    ".mp3" => ExportAudioFormat.MP3,
                    ".ogg" => ExportAudioFormat.OGG,
                    _ => throw new NotSupportedException("Unsupported file format")
                };
            }           
            
            switch (exportAudioFormat)
            {
                case ExportAudioFormat.WAV:
                    if (Path.GetExtension(OutputFilePath) != ".wav")
                        OutputFilePath += ".wav";

                    using (BinaryWriter writer = new BinaryWriter(File.Create(OutputFilePath)))
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(trAudio.FilePath)))
                    {
                        reader.BaseStream.Position += trAudio.offset;
                        for (int i = 0; i < trAudio.fileSize; i++)
                            writer.Write(reader.ReadByte());
                        reader.Close();
                        writer.Close();
                    }
                    return;
                case ExportAudioFormat.MP3:
                    if (Path.GetExtension(OutputFilePath) != ".mp3")
                        OutputFilePath = Path.ChangeExtension(OutputFilePath, ".mp3");

                    using (var containerStream = new FileStream(trAudio.FilePath, FileMode.Open, FileAccess.Read))
                    {
                        containerStream.Seek(trAudio.offset, SeekOrigin.Begin);
                        using (var limitedStream = new LimitedStream(containerStream, trAudio.fileSize))
                        {
                            using (var waveReader = new WaveFileReader(limitedStream))
                            {
                                WaveStream pcmStream = waveReader;
                                if (waveReader.WaveFormat.Encoding == WaveFormatEncoding.Adpcm)
                                {
                                    pcmStream = WaveFormatConversionStream.CreatePcmStream(waveReader);
                                }

                                using (pcmStream)
                                using (var mp3Stream = new FileStream(OutputFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    using (var mp3Writer = new LameMP3FileWriter(mp3Stream, pcmStream.WaveFormat, LAMEPreset.VBR_90))
                                    {
                                        Array.Clear(tempBuffer, 0, tempBuffer.Length);
                                        int bytesRead;
                                        while ((bytesRead = pcmStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                                        {
                                            mp3Writer.Write(tempBuffer, 0, bytesRead);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return;
                case ExportAudioFormat.OGG:
                    if (Path.GetExtension(OutputFilePath) != ".ogg")
                        OutputFilePath = Path.ChangeExtension(OutputFilePath, ".ogg");
                    using (var containerStream = new FileStream(trAudio.FilePath, FileMode.Open, FileAccess.Read))
                    {
                        containerStream.Seek(trAudio.offset, SeekOrigin.Begin);
                        using (var limitedStream = new LimitedStream(containerStream, trAudio.fileSize))
                        {
                            using (var waveReader = new WaveFileReader(limitedStream))
                            {
                                WaveStream pcmStream = waveReader;
                                if (waveReader.WaveFormat.Encoding == WaveFormatEncoding.Adpcm)
                                {
                                    pcmStream = WaveFormatConversionStream.CreatePcmStream(waveReader);
                                }
                                using (pcmStream)
                                using (var oggStream = new FileStream(OutputFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    throw new NotImplementedException("OGG export is not implemented yet");
                                    //// Create the OGG encoder
                                    //var oggEncoder = new OggEncoder(oggStream, pcmStream.WaveFormat.SampleRate, pcmStream.WaveFormat.Channels, 2);
                                    //
                                    //// Write the PCM data to the OGG encoder
                                    //byte[] buffer = new byte[4096]; // Adjust buffer size as needed
                                    //int bytesRead;
                                    //while ((bytesRead = pcmStream.Read(buffer, 0, buffer.Length)) > 0)
                                    //{
                                    //    oggEncoder.Encode(buffer, bytesRead);
                                    //}
                                    //
                                    //// Finalize the OGG encoding
                                    //oggEncoder.Finish();
                                }
                            }
                        }
                    }
                    return;

                default:
                    throw new NotSupportedException("Unsupported audio format");
            }
        }

        private void ConvertAndWrite(WaveStream waveStream, BinaryWriter writer,out int factValue)
        {
            var inputFormat = waveStream.WaveFormat;
            IWaveProvider waveProvider = waveStream;
            long startPos = writer.BaseStream.Position;
            long endPos;
            factValue = 0;
            int tempIndex = 0;

            if (inputFormat.SampleRate != CdAudioConstants.TR_AUDIO_SAMPLE_RATE || inputFormat.BitsPerSample != 16)
            {
                waveProvider = new MediaFoundationResampler(waveStream,
                    new WaveFormat(
                        rate: (int)CdAudioConstants.TR_AUDIO_SAMPLE_RATE,
                        bits: 16, 
                        channels: inputFormat.Channels))
                {
                    ResamplerQuality = this.resamplingQuality
                };
            }

            if (inputFormat.Channels != 2)
            {
                if (inputFormat.Channels == 1)
                {
                    waveProvider = new MonoToStereoProvider16(waveProvider);
                }
                else if (inputFormat.Channels > 2)
                {
                    throw new NotSupportedException("Unsupported number of channels");
                }
            }

            using (var pcmStream = new WaveProviderToWaveStream(waveProvider))
            {

                writer.BaseStream.Position += CdAudioConstants.TR_AUDIO_DATA_START_POS;
                while ((bytesRead = pcmStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                {
                    factValue += bytesRead;

                    tempIndex = 0;
                    for (int i = 0; i < CdAudioConstants.TR_AUDIO_NUM_OF_REQ_PCM_SHORTS_PER_BLOCK; i++)
                    {
                        pcmSampleBuffer[i] = tempBuffer[tempIndex++];
                        pcmSampleBuffer[i] += (short)(tempBuffer[tempIndex++] << 8);
                    }

                    encoder.EncodeBlock(pcmSampleBuffer, ref adpcmSampleBuffer);

                    for (int i = 0; i < CdAudioConstants.TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK; i++)
                        writer.Write(adpcmSampleBuffer[i]);
                }
                endPos = writer.BaseStream.Position;
                factValue /= 4;

                writer.BaseStream.Position = startPos;
                WriteTrWavHeader(writer, factValue);
                writer.BaseStream.Position = endPos;
            }
        }

        private void WriteTrWavHeader(BinaryWriter writer, int factValue)
        {
            int nBlocks = factValue / CdAudioConstants.TR_AUDIO_SAMPLES_PER_BLOCK;
            int nSamplesInLastBlock = factValue % CdAudioConstants.TR_AUDIO_SAMPLES_PER_BLOCK;
            if (nSamplesInLastBlock != 0)
                nBlocks++;

            uint dataSize = (uint)(nBlocks * CdAudioConstants.TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK);
            uint fileSize = CdAudioConstants.TR_AUDIO_HEADER_SIZE + dataSize;

            writer.Write(CdAudioConstants.RIFF_CHARS);
            writer.Write(fileSize);

            writer.Write(CdAudioConstants.WAVE_CHARS);
            writer.Write(CdAudioConstants.FMT_CHARS);
            writer.Write(CdAudioConstants.TR_AUDIO_FMT_SIZE);

            writer.Write(CdAudioConstants.TR_AUDIO_FORMAT);
            writer.Write(CdAudioConstants.TR_AUDIO_NUM_CHANNELS);
            writer.Write(CdAudioConstants.TR_AUDIO_SAMPLE_RATE);
            writer.Write(CdAudioConstants.TR_AUDIO_BYTE_RATE);
            writer.Write(CdAudioConstants.TR_AUDIO_BLOCK_ALIGNMENT);
            writer.Write(CdAudioConstants.TR_AUDIO_BITS_PER_SAMPLE);

            writer.Write(CdAudioConstants.TR_AUDIO_FMT_EXTRA_LENGTH);
            writer.Write(CdAudioConstants.TR_AUDIO_SAMPLES_PER_BLOCK);
            writer.Write(CdAudioConstants.TR_AUDIO_COEFFICIENT_COUNT);
            for (int i = 0; i < CdAudioConstants.TR_AUDIO_COEFFICIENT_COUNT; i++)
            {
                writer.Write(CdAudioConstants.TR_AUDIO_COEFFICIENT_1[i]);
                writer.Write(CdAudioConstants.TR_AUDIO_COEFFICIENT_2[i]);
            }
            writer.Write(CdAudioConstants.FACT_CHARS);
            writer.Write(CdAudioConstants.TR_AUDIO_FACT_SIZE);
            writer.Write((uint)factValue);

            writer.Write(CdAudioConstants.DATA_CHARS);
            writer.Write(dataSize);

        }

        private void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
