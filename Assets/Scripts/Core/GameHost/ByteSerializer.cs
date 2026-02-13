using System;
using System.Buffers;

namespace Noname.GameHost
{
    public interface IByteSerializable
    {
        int GetSerializedSize();
        int WriteTo(Span<byte> dst);
    }

    public static class ByteSerializer
    {
        /// <summary>
        /// SerializePooled 메서드입니다.
        /// </summary>
        public static PooledSegment SerializePooled(IByteSerializable obj)
        {
            int size = obj.GetSerializedSize();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);

            int written = obj.WriteTo(buffer.AsSpan(0, size));
            if (written != size)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw new InvalidOperationException($"Size mismatch. expected={size}, written={written}");
            }

            return new PooledSegment(buffer, written);
        }
    }
}
