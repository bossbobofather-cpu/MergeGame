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
        /// SerializePooled 함수를 처리합니다.
        /// </summary>
        public static PooledSegment SerializePooled(IByteSerializable obj)
        {
            // 핵심 로직을 처리합니다.
            int size = obj.GetSerializedSize();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);

            int written = obj.WriteTo(buffer.AsSpan(0, size));
            if (written != size)
            {
                // size 怨꾩빟??媛뺤젣?섎젮硫??대젃寃?
                ArrayPool<byte>.Shared.Return(buffer);
                throw new InvalidOperationException($"Size mismatch. expected={size}, written={written}");
            }

            return new PooledSegment(buffer, written);
        }
    }
}
