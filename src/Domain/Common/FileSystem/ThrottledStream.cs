using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/32724000/8205497
    /// </summary>
    public class ThrottledStream : Stream
    {
        Stream _InputStream;

        int _Throttle;

        Stopwatch _watch = Stopwatch.StartNew();

        long _TotalBytesRead;

        public ThrottledStream(Stream @in, int throttleKb)
        {
            _Throttle = throttleKb * 1024;
            _InputStream = @in;
        }

        public override bool CanRead => _InputStream.CanRead;

        public override bool CanSeek => _InputStream.CanSeek;

        public override bool CanWrite => false;

        public override void Flush() { }

        public override long Length => _InputStream.Length;

        public override long Position
        {
            get => _InputStream.Position;
            set => _InputStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var newCount = GetBytesToReturn(count);
            int read = _InputStream.Read(buffer, offset, newCount);
            Interlocked.Add(ref _TotalBytesRead, read);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _InputStream.Seek(offset, origin);
        }

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count) { }

        int GetBytesToReturn(int count)
        {
            return GetBytesToReturnAsync(count).Result;
        }

        public void SetThrottleSpeed(int throttleKb)
        {
            _Throttle = throttleKb * 1024;
        }

        async Task<int> GetBytesToReturnAsync(int count)
        {
            if (_Throttle <= 0)
                return count;

            long canSend = (long)(_watch.ElapsedMilliseconds * (_Throttle / 1000.0));

            int diff = (int)(canSend - _TotalBytesRead);

            if (diff <= 0)
            {
                var waitInSec = diff * -1.0 / _Throttle;

                await Task.Delay((int)(waitInSec * 1000)).ConfigureAwait(false);
            }

            if (diff >= count) return count;

            return diff > 0 ? diff : Math.Min(1024 * 8, count);
        }
    }
}