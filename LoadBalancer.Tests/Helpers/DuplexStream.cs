namespace LoadBalancer.Tests.Helpers
{
    public class DuplexStream : Stream
    {
        private readonly Stream _readStream;
        private readonly Stream _writeStream;

        public DuplexStream(Stream readStream, Stream writeStream)
        {
            _readStream = readStream;
            _writeStream = writeStream;
        }

        public override bool CanRead => _readStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _writeStream.CanWrite;
        public override long Length => _readStream.Length;
        public override long Position { get => _readStream.Position; set => _readStream.Position = value; }

        public override void Flush() => _writeStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _readStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _readStream.Seek(offset, origin);
        public override void SetLength(long value) => _writeStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _writeStream.Write(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _readStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _writeStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

}
