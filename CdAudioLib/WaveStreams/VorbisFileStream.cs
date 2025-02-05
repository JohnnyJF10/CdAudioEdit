using NAudio.Wave;
using NVorbis;

namespace CdAudioLib.WaveStreams
{
    public class VorbisFileStream : WaveStream
    {
        private readonly VorbisReader vorbisReader;
        private readonly WaveFormat waveFormat;
        private readonly float[] sharedBuffer;

        public VorbisFileStream(string filePath, float[] externalBuffer)
        {
            vorbisReader = new VorbisReader(filePath);
            waveFormat = WaveFormat.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                vorbisReader.SampleRate,
                vorbisReader.Channels,
                vorbisReader.SampleRate * vorbisReader.Channels * 2,
                vorbisReader.Channels * 2,                          
                16                                                  
            );

            if (externalBuffer == null || externalBuffer.Length < 4096)
            {
                throw new ArgumentException("Buffer must be non-null and have a sufficient size.", nameof(externalBuffer));
            }

            sharedBuffer = externalBuffer; 
        }

        public override WaveFormat WaveFormat => waveFormat;

        public override long Length => vorbisReader.TotalSamples * waveFormat.BlockAlign;

        public override long Position
        {
            get => vorbisReader.SamplePosition * waveFormat.BlockAlign;
            set => vorbisReader.SamplePosition = value / waveFormat.BlockAlign;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int samplesToRead = count / 2; 

            int samplesRead = vorbisReader.ReadSamples(sharedBuffer, 0, Math.Min(sharedBuffer.Length, samplesToRead));

            if (samplesRead == 0) return 0;

            int bytesWritten = 0;
            for (int i = 0; i < samplesRead; i++)
            {
                float sample = sharedBuffer[i];
                sample = Math.Max(-1.0f, Math.Min(1.0f, sample));
                short shortSample = (short)(sample * 32767.0f); 
                BitConverter.GetBytes(shortSample).CopyTo(buffer, offset + bytesWritten);
                bytesWritten += 2; 
            }

            return bytesWritten;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vorbisReader.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
