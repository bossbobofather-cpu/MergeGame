using System;
using System.Buffers;

namespace Noname.GameHost
{
    public readonly struct PooledSegment : IDisposable
    {
        public readonly byte[] Buffer;
        public readonly int Length;

        public PooledSegment(byte[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }

        public ArraySegment<byte> Segment => new(Buffer, 0, Length);
        /// <summary>
        /// Dispose 함수를 처리합니다.
        /// </summary>

        public void Dispose()
        {
            // 핵심 로직을 처리합니다.
            if (Buffer != null)
                ArrayPool<byte>.Shared.Return(Buffer);
        }
    }
}
