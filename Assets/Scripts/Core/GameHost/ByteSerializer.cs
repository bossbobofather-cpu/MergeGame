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
        public static PooledSegment SerializePooled(IByteSerializable obj)
        {
            int size = obj.GetSerializedSize();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);

            int written = obj.WriteTo(buffer.AsSpan(0, size));
            if (written != size)
            {
                // size 계약을 강제하려면 이렇게
                ArrayPool<byte>.Shared.Return(buffer);
                throw new InvalidOperationException($"Size mismatch. expected={size}, written={written}");
            }

            return new PooledSegment(buffer, written);
        }
    }
}
