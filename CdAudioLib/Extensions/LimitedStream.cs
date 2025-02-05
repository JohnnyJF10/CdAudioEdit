namespace CdAudioLib.Extensions
{
    public class LimitedStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _length;
        private long _position;

        public LimitedStream(Stream baseStream, long length)
        {
            _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _length = length;
            _position = 0;
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false; 
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= _length)
                return 0;

            count = (int)Math.Min(count, _length - _position);
            int bytesRead = _baseStream.Read(buffer, offset, count);
            _position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long targetPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin.")
            };

            if (targetPosition < 0 || targetPosition > _length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Seek position is out of range.");

            _baseStream.Seek(targetPosition + (_baseStream.Position - _position), SeekOrigin.Begin);
            _position = targetPosition;
            return _position;
        }

        public override void SetLength(long value) => throw new NotSupportedException("LimitedStream does not support SetLength.");
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("LimitedStream is read-only.");
    }
}
