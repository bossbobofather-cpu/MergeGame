using System;
using System.Buffers.Binary;
using Noname.GameHost;

namespace MyProject.MergeGame.Commands
{
    /// <summary>
    /// MergeGame용 Command 기본 타입입니다.
    /// </summary>
    public abstract class MergeGameCommand : GameCommandBase
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        protected MergeGameCommand(long senderUid) : base(senderUid)
        {
        }

        /// <summary>
        /// 역직렬화용 생성자입니다.
        /// </summary>
        protected MergeGameCommand(Guid commandId, long senderUid) : base(commandId, senderUid)
        {
        }
    }

    /// <summary>
    /// MergeGame용 Command 처리 결과 기본 타입입니다.
    /// </summary>
    public abstract class MergeCommandResult : GameCommandResultBase
    {
        /// <summary>
        /// MergeCommandResult 생성자입니다.
        /// </summary>
        protected MergeCommandResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
    }

    #region 게임 준비/시작/종료

    /// <summary>
    /// 게임 준비 command 입니다.
    /// </summary>
    public sealed class ReadyMergeGameCommand : MergeGameCommand
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        public ReadyMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        private ReadyMergeGameCommand(Guid commandId, long senderUid) : base(commandId, senderUid)
        {
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return 0;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ReadyMergeGameCommand ReadFrom(ReadOnlySpan<byte> src)
        {
            var (commandId, senderUid, _) = GameCommandBase.ReadHeader(src);
            return new ReadyMergeGameCommand(commandId, senderUid);
        }
    }


    /// <summary>
    /// 게임 준비 result 입니다.
    /// </summary>
    public sealed class ReadyMergeGameResult : MergeCommandResult
    {
        private ReadyMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
        /// <summary>
        /// Ok 메서드입니다.
        /// </summary>

        public static ReadyMergeGameResult Ok(long tick, long senderUid)
            => new(tick, senderUid, true, null);
        /// <summary>
        /// Fail 메서드입니다.
        /// </summary>

        public static ReadyMergeGameResult Fail(long tick, long senderUid, string reason)
            => new(tick, senderUid, false, reason);
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return 0;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ReadyMergeGameResult ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, senderUid, success, errorMessage, _) = GameCommandResultBase.ReadHeader(src);
            return new ReadyMergeGameResult(tick, senderUid, success, errorMessage);
        }
    }

    /// <summary>
    /// 게임 나가기 Command입니다.
    /// </summary>
    public sealed class ExitMergeGameCommand : MergeGameCommand
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        public ExitMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        private ExitMergeGameCommand(Guid commandId, long senderUid) : base(commandId, senderUid)
        {
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return 0;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ExitMergeGameCommand ReadFrom(ReadOnlySpan<byte> src)
        {
            var (commandId, senderUid, _) = GameCommandBase.ReadHeader(src);
            return new ExitMergeGameCommand(commandId, senderUid);
        }
    }

    /// <summary>
    /// 게임 나가기 Result입니다.
    /// </summary>
    public sealed class ExitMergeGameResult : MergeCommandResult
    {
        private ExitMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
        /// <summary>
        /// Ok 메서드입니다.
        /// </summary>

        public static ExitMergeGameResult Ok(long tick, long senderUid)
            => new(tick, senderUid, true, null);
        /// <summary>
        /// Fail 메서드입니다.
        /// </summary>

        public static ExitMergeGameResult Fail(long tick, long senderUid, string reason)
            => new(tick, senderUid, false, reason);
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return 0;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ExitMergeGameResult ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, senderUid, success, errorMessage, _) = GameCommandResultBase.ReadHeader(src);
            return new ExitMergeGameResult(tick, senderUid, success, errorMessage);
        }
    }

    #endregion

    #region 캐릭터 관리

    /// <summary>
    /// 캐릭터 스폰 Command입니다.
    /// </summary>
    public sealed class SpawnTowerCommand : MergeGameCommand
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        public SpawnTowerCommand(long senderUid) : base(senderUid)
        {
        }

        private SpawnTowerCommand(Guid commandId, long senderUid)
            : base(commandId, senderUid)
        {
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return 0;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static SpawnTowerCommand ReadFrom(ReadOnlySpan<byte> src)
        {
            var (commandId, senderUid, offset) = GameCommandBase.ReadHeader(src);

            return new SpawnTowerCommand(commandId, senderUid);
        }
    }

    /// <summary>
    /// 캐릭터 스폰 Result입니다.
    /// </summary>
    public sealed class SpawnTowerResult : MergeCommandResult
    {
        /// <summary>
        /// 생성된 캐릭터 UID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 캐릭터 정의 ID입니다.
        /// </summary>
        public long TowerId { get; }

        /// <summary>
        /// 캐릭터 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// PositionX 속성입니다.
        /// </summary>
        public float PositionX { get; }
        /// <summary>
        /// PositionY 속성입니다.
        /// </summary>
        public float PositionY { get; }
        /// <summary>
        /// PositionZ 속성입니다.
        /// </summary>
        public float PositionZ { get; }


        private SpawnTowerResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            long towerUid,
            long towerId,
            int grade,
            int slotIndex,
            float positionX,
            float positionY,
            float positionZ)
            : base(tick, senderUid, success, reason)
        {
            TowerUid = towerUid;
            TowerId = towerId;
            Grade = grade;
            SlotIndex = slotIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
        }

        /// <summary>
        /// 타워 스폰 성공 결과를 생성합니다.
        /// </summary>
        public static SpawnTowerResult Ok(
            long tick,
            long senderUid,
            long towerUid,
            long towerId,
            int grade,
            int slotIndex,
            float positionX,
            float positionY,
            float positionZ)
            => new(tick, senderUid, true, null, towerUid, towerId, grade, slotIndex, positionX, positionY, positionZ);
        /// <summary>
        /// Fail 메서드입니다.
        /// </summary>

        public static SpawnTowerResult Fail(long tick, long senderUid, string reason)
            => new(tick, senderUid, false, reason, 0, 0, 0, -1, 0, 0, 0);
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(long)     // TowerUid
                + sizeof(long)      // TowerId
                + sizeof(int)       // Grade
                + sizeof(int)       // SlotIndex
                + sizeof(float)     // PositionX
                + sizeof(float)     // PositionY
                + sizeof(float);    // PositionZ
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TowerId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), Grade);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SlotIndex);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionZ));

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static SpawnTowerResult ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, senderUid, success, errorMessage, offset) = GameCommandResultBase.ReadHeader(src);

            long towerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long towerId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int grade = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int slotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            float positionX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));

            return new SpawnTowerResult(tick, senderUid, success, errorMessage, towerUid, towerId, grade, slotIndex, positionX, positionY, positionZ);
        }
    }

    /// <summary>
    /// 캐릭터 머지 Command입니다.
    /// </summary>
    public sealed class MergeTowerCommand : MergeGameCommand
    {
        /// <summary>
        /// 드래그 시작 슬롯 인덱스 (소스 캐릭터)입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 드래그 종료 슬롯 인덱스 (타겟 캐릭터)입니다.
        /// </summary>
        public int ToSlotIndex { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public MergeTowerCommand(long senderUid, int fromSlotIndex, int toSlotIndex) : base(senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }

        private MergeTowerCommand(Guid commandId, long senderUid, int fromSlotIndex, int toSlotIndex)
            : base(commandId, senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(int)      // FromSlotIndex
                + sizeof(int);      // ToSlotIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), FromSlotIndex);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), ToSlotIndex);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MergeTowerCommand ReadFrom(ReadOnlySpan<byte> src)
        {
            var (commandId, senderUid, offset) = GameCommandBase.ReadHeader(src);

            int fromSlotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int toSlotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new MergeTowerCommand(commandId, senderUid, fromSlotIndex, toSlotIndex);
        }
    }

    /// <summary>
    /// 캐릭터 머지 Result입니다.
    /// </summary>
    public sealed class MergeTowerResult : MergeCommandResult
    {
        /// <summary>
        /// 소스 캐릭터 UID (흡수된 캐릭터)입니다.
        /// </summary>
        public long SourceTowerUid { get; }

        /// <summary>
        /// 타겟 캐릭터 UID (남는 캐릭터)입니다.
        /// </summary>
        public long TargetTowerUid { get; }

        /// <summary>
        /// 머지 결과로 생성된 캐릭터 UID입니다.
        /// </summary>
        public long ResultTowerUid { get; }

        /// <summary>
        /// 결과 캐릭터의 정의 ID입니다.
        /// </summary>
        public long ResultTowerId { get; }

        /// <summary>
        /// 결과 캐릭터의 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 캐릭터가 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        private MergeTowerResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid,
            long resultTowerId,
            int resultGrade,
            int slotIndex)
            : base(tick, senderUid, success, reason)
        {
            SourceTowerUid = sourceTowerUid;
            TargetTowerUid = targetTowerUid;
            ResultTowerUid = resultTowerUid;
            ResultTowerId = resultTowerId;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }

        /// <summary>
        /// 타워 머지 성공 결과를 생성합니다.
        /// </summary>
        public static MergeTowerResult Ok(
            long tick,
            long senderUid,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid,
            long resultTowerId,
            int resultGrade,
            int slotIndex)
            => new(
                tick, senderUid, true, null,
                sourceTowerUid, targetTowerUid, resultTowerUid,
                resultTowerId, resultGrade, slotIndex);
        /// <summary>
        /// Fail 메서드입니다.
        /// </summary>

        public static MergeTowerResult Fail(long tick, long senderUid, string reason)
            => new(tick, senderUid, false, reason, 0, 0, 0, 0, 0, -1);
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(long)     // SourceTowerUid
                + sizeof(long)      // TargetTowerUid
                + sizeof(long)      // ResultTowerUid
                + sizeof(long)      // ResultTowerId
                + sizeof(int)       // ResultGrade
                + sizeof(int);      // SlotIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), SourceTowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TargetTowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), ResultTowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), ResultTowerId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), ResultGrade);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SlotIndex);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MergeTowerResult ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, senderUid, success, errorMessage, offset) = GameCommandResultBase.ReadHeader(src);

            long sourceTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long targetTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long resultTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long resultTowerId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int resultGrade = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int slotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new MergeTowerResult(tick, senderUid, success, errorMessage,
                sourceTowerUid, targetTowerUid, resultTowerUid, resultTowerId, resultGrade, slotIndex);
        }
    }

    #endregion

    #region 멀티플레이(상대 공격)

    /// <summary>
    /// 상대(다른 세션/플레이어)로부터 몬스터를 주입(추가 스폰)하는 Command입니다.
    /// 서버/MatchHost가 상대 공격(garbage) 처리를 위해 사용합니다.
    /// </summary>
    public sealed class InjectMonstersCommand : MergeGameCommand
    {
        /// <summary>
        /// 몬스터를 받을 대상 플레이어 인덱스입니다. -1이면 자동 지정(다음 플레이어)합니다.
        /// </summary>
        public int TargetPlayerIndex { get; }

        /// <summary>
        /// 주입할 몬스터 정의 ID입니다. null/빈 값이면 기본 몬스터를 사용합니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 주입할 몬스터 수입니다.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 스폰할 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// InjectMonstersCommand 생성자입니다.
        /// </summary>
        public InjectMonstersCommand(long senderUid, int targetPlayerIndex, long monsterId, int count, int pathIndex = 0) : base(senderUid)
        {
            TargetPlayerIndex = targetPlayerIndex;
            MonsterId = monsterId;
            Count = count;
            PathIndex = pathIndex;
        }

        private InjectMonstersCommand(Guid commandId, long senderUid, int targetPlayerIndex, long monsterId, int count, int pathIndex)
            : base(commandId, senderUid)
        {
            TargetPlayerIndex = targetPlayerIndex;
            MonsterId = monsterId;
            Count = count;
            PathIndex = pathIndex;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(int)      // TargetPlayerIndex
                + sizeof(long)      // MonsterId
                + sizeof(int)       // Count
                + sizeof(int);      // PathIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), TargetPlayerIndex);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), Count);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), PathIndex);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static InjectMonstersCommand ReadFrom(ReadOnlySpan<byte> src)
        {
            var (commandId, senderUid, offset) = GameCommandBase.ReadHeader(src);

            int targetPlayerIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            long monsterId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int count = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int pathIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new InjectMonstersCommand(commandId, senderUid, targetPlayerIndex, monsterId, count, pathIndex);
        }
    }

    /// <summary>
    /// 몬스터 주입 Result입니다.
    /// </summary>
    public sealed class InjectMonstersResult : MergeCommandResult
    {
        /// <summary>
        /// 실제로 스폰된 몬스터 수입니다.
        /// </summary>
        public int SpawnedCount { get; }

        private InjectMonstersResult(long tick, long senderUid, bool success, string reason, int spawnedCount)
            : base(tick, senderUid, success, reason)
        {
            SpawnedCount = spawnedCount;
        }
        /// <summary>
        /// Ok 메서드입니다.
        /// </summary>

        public static InjectMonstersResult Ok(long tick, long senderUid, int spawnedCount)
            => new(tick, senderUid, true, null, spawnedCount);
        /// <summary>
        /// Fail 메서드입니다.
        /// </summary>

        public static InjectMonstersResult Fail(long tick, long senderUid, string reason)
            => new(tick, senderUid, false, reason, 0);
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(int);     // SpawnedCount
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SpawnedCount);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static InjectMonstersResult ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, senderUid, success, errorMessage, offset) = GameCommandResultBase.ReadHeader(src);

            int spawnedCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new InjectMonstersResult(tick, senderUid, success, errorMessage, spawnedCount);
        }
    }

    #endregion
}
