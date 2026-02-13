using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Noname.GameHost
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameCommandBase : IByteSerializable
    {
        private const int GuidSize = 16;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public Guid CommandId { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long SenderUid { get; }
        /// <summary>
        /// GameCommandBase 메서드입니다.
        /// </summary>

        protected GameCommandBase(long senderUid = 0)
        {
            CommandId = Guid.NewGuid();
            SenderUid = senderUid;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        protected GameCommandBase(Guid commandId, long senderUid)
        {
            CommandId = commandId;
            SenderUid = senderUid;
        }
        /// <summary>
        /// GetSerializedSize 메서드입니다.
        /// </summary>

        public int GetSerializedSize()
        {
            return GuidSize + sizeof(long) + GetPayloadSize();
        }
        /// <summary>
        /// WriteTo 메서드입니다.
        /// </summary>

        public int WriteTo(Span<byte> dst)
        {
            var offset = 0;

            CommandId.TryWriteBytes(dst.Slice(offset, GuidSize));
            offset += GuidSize;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), SenderUid);
            offset += sizeof(long);

            var payloadWritten = WritePayload(dst.Slice(offset));
            offset += payloadWritten;
            return offset;
        }
        /// <summary>
        /// ReadHeader 메서드입니다.
        /// </summary>

        public static (Guid commandId, long senderUid, int bytesRead) ReadHeader(ReadOnlySpan<byte> src)
        {
            var offset = 0;

            var commandId = new Guid(src.Slice(offset, GuidSize));
            offset += GuidSize;

            long senderUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            return (commandId, senderUid, offset);
        }

        protected abstract int GetPayloadSize();
        protected abstract int WritePayload(Span<byte> dst);
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameCommandResultBase : IByteSerializable
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long SenderUid { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string ErrorMessage { get; }

        protected GameCommandResultBase(long tick, long senderUid, bool success, string errorMessage = null)
        {
            Tick = tick;
            SenderUid = senderUid;
            Success = success;
            ErrorMessage = errorMessage ?? string.Empty;
        }
        /// <summary>
        /// GetSerializedSize 메서드입니다.
        /// </summary>

        public int GetSerializedSize()
        {
            return sizeof(long)     // Tick
                + sizeof(long)      // SenderUid
                + sizeof(byte)      // Success
                + sizeof(ushort)    // ErrorMessage length prefix
                + Encoding.UTF8.GetByteCount(ErrorMessage) // ErrorMessage bytes
                + GetPayloadSize();
        }
        /// <summary>
        /// WriteTo 메서드입니다.
        /// </summary>

        public int WriteTo(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), Tick);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), SenderUid);
            offset += sizeof(long);

            dst[offset] = Success ? (byte)1 : (byte)0;
            offset += sizeof(byte);

            var errorMsgCount = Encoding.UTF8.GetByteCount(ErrorMessage);
            BinaryPrimitives.WriteUInt16LittleEndian(dst.Slice(offset, sizeof(ushort)), (ushort)errorMsgCount);
            offset += sizeof(ushort);

            Encoding.UTF8.GetBytes(ErrorMessage, dst.Slice(offset, errorMsgCount));
            offset += errorMsgCount;

            var payloadWritten = WritePayload(dst.Slice(offset));
            offset += payloadWritten;
            return offset;
        }
        /// <summary>
        /// ReadHeader 메서드입니다.
        /// </summary>

        public static (long tick, long senderUid, bool success, string errorMessage, int bytesRead) ReadHeader(ReadOnlySpan<byte> src)
        {
            var offset = 0;

            long tick = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long senderUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            bool success = src[offset] != 0;
            offset += sizeof(byte);

            ushort errorMsgLen = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            string errorMessage = errorMsgLen > 0
                ? Encoding.UTF8.GetString(src.Slice(offset, errorMsgLen))
                : string.Empty;
            offset += errorMsgLen;

            return (tick, senderUid, success, errorMessage, offset);
        }

        protected abstract int GetPayloadSize();
        protected abstract int WritePayload(Span<byte> dst);
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameEventBase : IByteSerializable
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long Tick { get; }

        protected GameEventBase(long tick)
        {
            Tick = tick;
        }
        /// <summary>
        /// GetSerializedSize 메서드입니다.
        /// </summary>

        public int GetSerializedSize()
        {
            return sizeof(long)
                + GetPayloadSize();
        }
        /// <summary>
        /// WriteTo 메서드입니다.
        /// </summary>

        public int WriteTo(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), Tick);
            offset += sizeof(long);

            var payloadWritten = WritePayload(dst.Slice(offset));
            offset += payloadWritten;
            return offset;
        }
        /// <summary>
        /// ReadHeader 메서드입니다.
        /// </summary>

        public static (long tick, int bytesRead) ReadHeader(ReadOnlySpan<byte> src)
        {
            var offset = 0;

            long tick = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            return (tick, offset);
        }

        protected abstract int GetPayloadSize();
        protected abstract int WritePayload(Span<byte> dst);
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameSnapshotBase : IByteSerializable
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long Tick { get; }

        protected GameSnapshotBase(long tick)
        {
            Tick = tick;
        }
        /// <summary>
        /// GetSerializedSize 메서드입니다.
        /// </summary>

        public int GetSerializedSize()
        {
            return sizeof(long)
                + GetPayloadSize();
        }
        /// <summary>
        /// WriteTo 메서드입니다.
        /// </summary>

        public int WriteTo(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), Tick);
            offset += sizeof(long);

            var payloadWritten = WritePayload(dst.Slice(offset));
            offset += payloadWritten;
            return offset;
        }
        /// <summary>
        /// ReadHeader 메서드입니다.
        /// </summary>

        public static (long tick, int bytesRead) ReadHeader(ReadOnlySpan<byte> src)
        {
            var offset = 0;

            long tick = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            return (tick, offset);
        }

        protected abstract int GetPayloadSize();
        protected abstract int WritePayload(Span<byte> dst);
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public interface IGameHost<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        event Action<TResult> ResultProduced;
        event Action<TEvent> EventRaised;

        void StartSimulation();
        void StopSimulation();
        void SendCommand(TCommand command);
        void FlushEvents();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        TSnapshot GetLatestSnapshot();
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    internal interface IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        long Tick { get; }

        void Submit(TCommand command);
        void Advance(float deltaSeconds);
        TSnapshot BuildSnapshot();
    }
}
