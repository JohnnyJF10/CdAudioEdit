using NAudio.Wave;

namespace CdAudioLib.WaveStreams
{
    public class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider waveProvider;
        private readonly WaveFormat waveFormat;
        private long position;

        public WaveProviderToWaveStream(IWaveProvider waveProvider)
        {
            this.waveProvider = waveProvider ?? throw new ArgumentNullException(nameof(waveProvider));
            this.waveFormat = waveProvider.WaveFormat;
        }

        public override WaveFormat WaveFormat => waveFormat;

        public override long Length => 0; 

        public override long Position
        {
            get => position;
            set => throw new NotSupportedException("Setting position is not supported");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = waveProvider.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }
    }
}
