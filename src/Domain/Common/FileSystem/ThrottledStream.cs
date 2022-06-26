using System.Diagnostics;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/32724000/8205497
    /// </summary>
    public class ThrottledStream : Stream
    {
        #region Fields

        private readonly Stream _inputStream;

        private readonly Stopwatch _watch = Stopwatch.StartNew();

        private int _throttle;

        private long _totalBytesRead;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottledStream"/> class.
        /// </summary>
        /// <param name="in">The input <see cref="Stream"/>.</param>
        /// <param name="throttleKb">The kb/s to throttle by.</param>
        public ThrottledStream(Stream @in, int throttleKb)
        {
            _throttle = throttleKb * 1024;
            _inputStream = @in;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override bool CanRead => _inputStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _inputStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _inputStream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _inputStream.Position;
            set => _inputStream.Position = value;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var newCount = GetBytesToReturn(count);
            int read = _inputStream.Read(buffer, offset, newCount);
            Interlocked.Add(ref _totalBytesRead, read);
            return read;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inputStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value) { }

        public void SetThrottleSpeed(int throttleKb)
        {
            _throttle = throttleKb * 1024;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) { }

        #endregion

        #region Private Methods

        private int GetBytesToReturn(int count)
        {
            return GetBytesToReturnAsync(count).Result;
        }

        async Task<int> GetBytesToReturnAsync(int count)
        {
            if (_throttle <= 0)
                return count;

            long canSend = (long)(_watch.ElapsedMilliseconds * (_throttle / 1000.0));

            int diff = (int)(canSend - _totalBytesRead);

            if (diff <= 0)
            {
                var waitInSec = diff * -1.0 / _throttle;

                await Task.Delay((int)(waitInSec * 1000)).ConfigureAwait(false);
            }

            if (diff >= count) return count;

            return diff > 0 ? diff : Math.Min(1024 * 8, count);
        }

        #endregion
    }
}