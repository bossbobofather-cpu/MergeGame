using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Noname.GameHost;

namespace MyProject.MergeGame.Events
{
    /// <summary>
    /// MergeGame 이벤트의 기본 타입입니다.
    /// </summary>
    public abstract class MergeGameEvent : GameEventBase
    {
        /// <summary>
        /// PlayerIndex 속성입니다.
        /// </summary>
        public int PlayerIndex { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        protected MergeGameEvent(long tick, int playerIndex) : base(tick)
        {
            PlayerIndex = playerIndex;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return sizeof(int); // PlayerIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), PlayerIndex);
            offset += sizeof(int);
            return offset;
        }
        /// <summary>
        /// ReadMergeHeader 메서드입니다.
        /// </summary>

        public static (long tick, int playerIndex, int bytesRead) ReadMergeHeader(ReadOnlySpan<byte> src)
        {
            var (tick, offset) = GameEventBase.ReadHeader(src);

            int playerIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            return (tick, playerIndex, offset);
        }
    }

    /// <summary>
    /// ConnectedInfoEvent 클래스입니다.
    /// </summary>
    public sealed class ConnectedInfoEvent : MergeGameEvent
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        public ConnectedInfoEvent(long tick, int playerIndex) : base(tick, playerIndex)
        {
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize();
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return base.WritePayload(dst);
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ConnectedInfoEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, _) = ReadMergeHeader(src);
            return new ConnectedInfoEvent(tick, playerIndex);
        }
    }

    /// <summary>
    /// 게임 시작 이벤트입니다.
    /// </summary>
    public sealed class GameStartedEvent : MergeGameEvent
    {
        /// <summary>
        /// base 메서드입니다.
        /// </summary>
        public GameStartedEvent(long tick, int playerIndex) : base(tick, playerIndex)
        {
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize();
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            return base.WritePayload(dst);
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static GameStartedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, _) = ReadMergeHeader(src);

            return new GameStartedEvent(tick, playerIndex);
        }
    }

    /// <summary>
    /// 타워 스폰 이벤트입니다.
    /// </summary>
    public sealed class TowerSpawnedEvent : MergeGameEvent
    {
        /// <summary>
        /// 생성된 타워 UID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 생성된 타워 Id입니다.
        /// </summary>
        public long TowerId { get; }

        /// <summary>
        /// 유닛 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 배치된 위치 X 정보입니다.
        /// </summary>
        public float PositionX { get; }

        /// <summary>
        /// 배치된 위치 Y 정보입니다.
        /// </summary>
        public float PositionY { get; }

        /// <summary>
        /// 배치된 위치 Z 정보입니다.
        /// </summary>
        public float PositionZ { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public TowerSpawnedEvent(long tick, int playerIndex, long towerUid, long towerId, int grade, int slotIndex, float positionX, float positionY, float positionZ) : base(tick, playerIndex)
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
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // TowerUid
                + sizeof(long)  // TowerId
                + sizeof(int)   // Grade
                + sizeof(int)  // SlotIndex
                + sizeof(float)  // PositionX
                + sizeof(float)  // PositionY
                + sizeof(float); // PositionZ
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

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

        public static TowerSpawnedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

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
            offset += sizeof(float);

            return new TowerSpawnedEvent(tick, playerIndex, towerUid, towerId, grade, slotIndex, positionX, positionY, positionZ);
        }
    }

    /// <summary>
    /// 타워 머지 이벤트입니다.
    /// </summary>
    public sealed class TowerMergedEvent : MergeGameEvent
    {
        /// <summary>
        /// 머지 시도 타워 UID입니다.
        /// </summary>
        public long SourceTowerUid { get; }

        /// <summary>
        /// 머지 타겟 타워 UID입니다.
        /// </summary>
        public long TargetTowerUid { get; }

        /// <summary>
        /// 머지 결과 유닛 UID입니다.
        /// </summary>
        public long ResultTowerUid { get; }

        /// <summary>
        /// 머지 결과 타워 ID입니다.
        /// </summary>
        /// </summary>
        public long ResultTowerId { get; }

        /// <summary>
        /// 머지 결과 유닛 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 유닛이 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// TowerMergedEvent 생성자입니다.
        /// </summary>
        public TowerMergedEvent(
            long tick,
            int playerIndex,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid,
            long resultTowerId,
            int resultGrade,
            int slotIndex) : base(tick, playerIndex)
        {
            SourceTowerUid = sourceTowerUid;
            TargetTowerUid = targetTowerUid;
            ResultTowerUid = resultTowerUid;
            ResultTowerId = resultTowerId;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // SourceTowerUid
                + sizeof(long)  // TargetTowerUid
                + sizeof(long)  // ResultTowerUid
                + sizeof(long)  // ResultTowerId
                + sizeof(int)   // ResultGrade
                + sizeof(int);  // SlotIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

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

        public static TowerMergedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

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

            return new TowerMergedEvent(tick, playerIndex, sourceTowerUid, targetTowerUid, resultTowerUid, resultTowerId, resultGrade, slotIndex);
        }
    }

    /// <summary>
    /// 타워 제거 이벤트입니다.
    /// </summary>
    public sealed class TowerRemovedEvent : MergeGameEvent
    {
        /// <summary>
        /// 제거된 타워 UID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 제거된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public TowerRemovedEvent(long tick, int playerIndex, long unitUid, int slotIndex) : base(tick, playerIndex)
        {
            TowerUid = unitUid;
            SlotIndex = slotIndex;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // TowerUid
                + sizeof(int);  // SlotIndex
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SlotIndex);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static TowerRemovedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long towerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int slotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new TowerRemovedEvent(tick, playerIndex, towerUid, slotIndex);
        }
    }

    /// <summary>
    /// 게임 오버 이벤트입니다.
    /// </summary>
    public sealed class GameOverEvent : MergeGameEvent
    {
        /// <summary>
        /// 승리 여부입니다.
        /// </summary>
        public bool IsVictory { get; }

        /// <summary>
        /// 최종 점수입니다.
        /// </summary>
        public int FinalScore { get; }

        /// <summary>
        /// 도달한 최고 등급입니다.
        /// </summary>
        public int MaxGradeReached { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GameOverEvent(long tick, int playerIndex, bool isVictory, int finalScore, int maxGradeReached) : base(tick, playerIndex)
        {
            IsVictory = isVictory;
            FinalScore = finalScore;
            MaxGradeReached = maxGradeReached;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(byte)  // IsVictory
                + sizeof(int)   // FinalScore
                + sizeof(int);  // MaxGradeReached
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            dst[offset] = IsVictory ? (byte)1 : (byte)0;
            offset += sizeof(byte);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), FinalScore);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), MaxGradeReached);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static GameOverEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            bool isVictory = src[offset] != 0;
            offset += sizeof(byte);

            int finalScore = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int maxGradeReached = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new GameOverEvent(tick, playerIndex, isVictory, finalScore, maxGradeReached);
        }
    }

    /// <summary>
    /// 점수 변경 이벤트입니다.
    /// </summary>
    public sealed class ScoreChangedEvent : MergeGameEvent
    {
        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 점수 변화량입니다.
        /// </summary>
        public int ScoreDelta { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public ScoreChangedEvent(long tick, int playerIndex, int currentScore, int scoreDelta) : base(tick, playerIndex)
        {
            CurrentScore = currentScore;
            ScoreDelta = scoreDelta;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(int)   // CurrentScore
                + sizeof(int);  // ScoreDelta
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), CurrentScore);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), ScoreDelta);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static ScoreChangedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            int currentScore = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int scoreDelta = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new ScoreChangedEvent(tick, playerIndex, currentScore, scoreDelta);
        }
    }

    /// <summary>
    /// 캐릭터 공격 이벤트입니다.
    /// </summary>
    public sealed class TowerAttackedEvent : MergeGameEvent
    {
        /// <summary>
        /// 공격한 캐릭터 UID입니다.
        /// </summary>
        public long AttackerUid { get; }

        /// <summary>
        /// 공격 대상 몬스터 UID입니다.
        /// </summary>
        public long TargetUid { get; }

        /// <summary>
        /// 가한 데미지입니다. 
        /// </summary>
        public float Damage { get; }

        /// <summary>
        /// 공격자 위치입니다. 
        /// </summary>
        public float AttackerX { get; }
        /// <summary>
        /// AttackerY 속성입니다.
        /// </summary>
        public float AttackerY { get; }
        /// <summary>
        /// AttackerZ 속성입니다.
        /// </summary>
        public float AttackerZ { get; }

        /// <summary>
        /// 대상 위치입니다.    
        /// </summary>
        public float TargetX { get; }
        /// <summary>
        /// TargetY 속성입니다.
        /// </summary>
        public float TargetY { get; }
        /// <summary>
        /// TargetZ 속성입니다.
        /// </summary>
        public float TargetZ { get; }

        /// <summary>
        /// 공격 방식입니다.
        /// </summary>
        public TowerAttackType AttackType { get; }

        /// <summary>
        /// 투사체 타입입니다.
        /// </summary>
        public ProjectileType ProjectileType { get; }

        /// <summary>
        /// 투사체 속도입니다.  
        /// </summary>
        public float ProjectileSpeed { get; }

        /// <summary>
        /// Throw 타입의 반경입니다.
        /// </summary>
        public float ThrowRadius { get; }

        /// <summary>
        /// 투사체 고유 ID입니다 (HitScan은 0).
        /// </summary>
        public long ProjectileUid { get; }

        /// <summary>
        /// TowerAttackedEvent 생성자입니다.
        /// </summary>
        public TowerAttackedEvent(
            long tick,
            int playerIndex,
            long attackerUid,
            long targetUid,
            float damage,
            float attackerX,
            float attackerY,
            float attackerZ,
            float targetX,
            float targetY,
            float targetZ,
            TowerAttackType attackType,
            ProjectileType projectileType,
            float projectileSpeed,
            float throwRadius,
            long projectileUid = 0) : base(tick, playerIndex)
        {
            AttackerUid = attackerUid;
            TargetUid = targetUid;
            Damage = damage;
            AttackerX = attackerX;
            AttackerY = attackerY;
            AttackerZ = attackerZ;
            TargetX = targetX;
            TargetY = targetY;
            TargetZ = targetZ;
            AttackType = attackType;
            ProjectileType = projectileType;
            ProjectileSpeed = projectileSpeed;
            ThrowRadius = throwRadius;
            ProjectileUid = projectileUid;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // AttackerUid
                + sizeof(long)  // TargetUid
                + sizeof(float)  // Damage
                + sizeof(float)  // AttackerX
                + sizeof(float)  // AttackerY
                + sizeof(float)  // AttackerZ
                + sizeof(float)  // TargetX
                + sizeof(float)  // TargetY
                + sizeof(float)  // TargetZ
                + sizeof(int)   // AttackType
                + sizeof(int)   // ProjectileType
                + sizeof(float)  // ProjectileSpeed
                + sizeof(float)  // ThrowRadius
                + sizeof(long); // ProjectileUid
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), AttackerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TargetUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(Damage));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(AttackerX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(AttackerY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(AttackerZ));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(TargetX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(TargetY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(TargetZ));
            offset += sizeof(float);


            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), (int)AttackType);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), (int)ProjectileType);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                            BitConverter.SingleToInt32Bits(ProjectileSpeed));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(ThrowRadius));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), ProjectileUid);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static TowerAttackedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long attackerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long targetUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            float damage = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float attackerX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float attackerY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float attackerZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetX = BitConverter.Int32BitsToSingle(
    BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetY = BitConverter.Int32BitsToSingle(
    BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetZ = BitConverter.Int32BitsToSingle(
    BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            var attackType = (TowerAttackType)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            var projectileType = (ProjectileType)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            float projectileSpeed = BitConverter.Int32BitsToSingle(
    BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float throwRadius = BitConverter.Int32BitsToSingle(
BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            long projectileUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));

            return new TowerAttackedEvent(
                tick, playerIndex,
                attackerUid, targetUid, damage,
                attackerX, attackerY, attackerZ,
                targetX, targetY, targetZ,
                attackType, projectileType,
                projectileSpeed, throwRadius,
                projectileUid);
        }
    }

    /// <summary>
    /// 머지 이펙트 발동 이벤트입니다.
    /// </summary>
    public sealed class EffectTriggeredEvent : MergeGameEvent
    {
        /// <summary>
        /// 이펙트 ID입니다.
        /// </summary>
        public long EffectId { get; }

        /// <summary>
        /// 이펙트 발동 위치입니다.
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

        /// <summary>
        /// 소스 이펙트인지 타겟 이펙트인지 여부입니다.
        /// </summary>
        public bool IsSourceEffect { get; }

        /// <summary>
        /// 소스 캐릭터 UID입니다.
        /// </summary>
        public long SourceTowerUid { get; }

        /// <summary>
        /// 타겟 캐릭터 UID입니다.
        /// </summary>
        public long TargetTowerUid { get; }

        /// <summary>
        /// 결과 캐릭터 UID입니다.
        /// </summary>
        public long ResultTowerUid { get; }

        /// <summary>
        /// EffectTriggeredEvent 생성자입니다.
        /// </summary>
        public EffectTriggeredEvent(
            long tick,
            int playerIndex,
            long effectId,
            float positionX,
            float positionY,
            float positionZ,
            bool isSourceEffect,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid) : base(tick, playerIndex)
        {
            EffectId = effectId;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            IsSourceEffect = isSourceEffect;
            SourceTowerUid = sourceTowerUid;
            TargetTowerUid = targetTowerUid;
            ResultTowerUid = resultTowerUid;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // EffectId
                + sizeof(float)  // PositionX
                + sizeof(float)  // PositionY
                + sizeof(float)  // PositionZ
                + sizeof(byte)  // IsSourceEffect
                + sizeof(long)  // SourceTowerUid
                + sizeof(long)  // TargetTowerUid
                + sizeof(long); // ResultTowerUid
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), EffectId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionZ));
            offset += sizeof(float);

            dst[offset] = IsSourceEffect ? (byte)1 : (byte)0;
            offset += sizeof(byte);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), SourceTowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), TargetTowerUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), ResultTowerUid);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static EffectTriggeredEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long effectId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            float positionX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            bool isSourceEffect = src[offset] != 0;
            offset += sizeof(byte);

            long sourceTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long targetTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long resultTowerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));

            return new EffectTriggeredEvent(
                tick, playerIndex, effectId,
                positionX, positionY, positionZ,
                isSourceEffect,
                sourceTowerUid, targetTowerUid, resultTowerUid);
        }
    }

    #region 몬스터 이벤트

    /// <summary>
    /// 몬스터 스폰 이벤트입니다.
    /// </summary>
    public sealed class MonsterSpawnedEvent : MergeGameEvent
    {
        /// <summary>
        /// 몬스터 고유 ID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 몬스터 정의 ID입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 이동 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 스폰 위치입니다.
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

        /// <summary>
        /// 최대 체력입니다.    
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// MonsterSpawnedEvent 생성자입니다.
        /// </summary>
        public MonsterSpawnedEvent(
            long tick,
            int playerIndex,
            long monsterUid,
            long monsterId,
            int pathIndex,
            float positionX,
            float positionY,
            float positionZ,
            float maxHealth) : base(tick, playerIndex)
        {
            MonsterUid = monsterUid;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            MaxHealth = maxHealth;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // MonsterUid
                + sizeof(long)  // MonsterId
                + sizeof(int)   // PathIndex
                + sizeof(float)  // PositionX
                + sizeof(float)  // PositionY
                + sizeof(float)  // PositionZ
                + sizeof(float); // MaxHealth
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), PathIndex);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionZ));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(MaxHealth));
            offset += sizeof(float);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MonsterSpawnedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long monsterUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            long monsterId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int pathIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            float positionX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float maxHealth = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));

            return new MonsterSpawnedEvent(tick, playerIndex, monsterUid, monsterId, pathIndex, positionX, positionY, positionZ, maxHealth);
        }
    }

    /// <summary>
    /// 몬스터 데미지 이벤트입니다.
    /// </summary>
    public sealed class MonsterDamagedEvent : MergeGameEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 받은 데미지입니다.  
        /// </summary>
        public float Damage { get; }

        /// <summary>
        /// 현재 체력입니다.    
        /// </summary>
        public float CurrentHealth { get; }

        /// <summary>
        /// 공격자 캐릭터 UID입니다 (0이면 직접 데미지).
        /// </summary>
        public long AttackerUid { get; }

        /// <summary>
        /// MonsterDamagedEvent 생성자입니다.
        /// </summary>
        public MonsterDamagedEvent(
            long tick,
            int playerIndex,
            long monsterUid,
            float damage,
            float currentHealth,
            long attackerUid) : base(tick, playerIndex)
        {
            MonsterUid = monsterUid;
            Damage = damage;
            CurrentHealth = currentHealth;
            AttackerUid = attackerUid;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // MonsterUid
                + sizeof(float)  // Damage
                + sizeof(float)  // CurrentHealth
                + sizeof(long); // AttackerUid
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(Damage));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(CurrentHealth));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), AttackerUid);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MonsterDamagedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long monsterUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            float damage = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float currentHealth = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            long attackerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));

            return new MonsterDamagedEvent(tick, playerIndex, monsterUid, damage, currentHealth, attackerUid);
        }
    }

    /// <summary>
    /// 몬스터 사망 이벤트입니다.
    /// </summary>
    public sealed class MonsterDiedEvent : MergeGameEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 사망 위치입니다.   
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

        /// <summary>
        /// 획득 골드입니다.
        /// </summary>
        public int GoldReward { get; }

        /// <summary>
        /// 처치한 캐릭터 UID입니다 (0이면 직접 처치).
        /// </summary>
        public long KillerUid { get; }

        /// <summary>
        /// MonsterDiedEvent 생성자입니다.
        /// </summary>
        public MonsterDiedEvent(
            long tick,
            int playerIndex,
            long monsterUid,
            float positionX,
            float positionY,
            float positionZ,
            int goldReward,
            long killerUid) : base(tick, playerIndex)
        {
            MonsterUid = monsterUid;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            GoldReward = goldReward;
            KillerUid = killerUid;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // MonsterUid
                + sizeof(float)  // PositionX
                + sizeof(float)  // PositionY
                + sizeof(float)  // PositionZ
                + sizeof(int)   // GoldReward
                + sizeof(long); // KillerUid
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionZ));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), GoldReward);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), KillerUid);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MonsterDiedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long monsterUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            float positionX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            int goldReward = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            long killerUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));

            return new MonsterDiedEvent(tick, playerIndex, monsterUid, positionX, positionY, positionZ, goldReward, killerUid);
        }
    }

    /// <summary>
    /// 몬스터 이동 이벤트입니다.
    /// </summary>
    public sealed class MonsterMovedEvent : MergeGameEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 새 위치입니다.  
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

        /// <summary>
        /// 경로 진행도입니다 (0.0 ~ 1.0). 
        /// </summary>
        public float PathProgress { get; }

        /// <summary>
        /// MonsterMovedEvent 생성자입니다.
        /// </summary>
        public MonsterMovedEvent(
            long tick,
            int playerIndex,
            long monsterUid,
            float positionX,
            float positionY,
            float positionZ,
            float pathProgress) : base(tick, playerIndex)
        {
            MonsterUid = monsterUid;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            PathProgress = pathProgress;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(long)  // MonsterUid
                + sizeof(float)  // PositionX
                + sizeof(float)  // PositionY
                + sizeof(float)  // PositionZ
                + sizeof(float); // PathProgress
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterUid);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PositionZ));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)),
                BitConverter.SingleToInt32Bits(PathProgress));

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MonsterMovedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            long monsterUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            float positionX = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionY = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float positionZ = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float pathProgress = BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));

            return new MonsterMovedEvent(tick, playerIndex, monsterUid, positionX, positionY, positionZ, pathProgress);
        }
    }

    /// <summary>
    /// 몬스터 주입(공격) 연출 이벤트입니다.
    /// PlayerIndex는 주입을 발생시킨 소스 플레이어를 의미합니다.
    /// </summary>
    public sealed class MonsterInjectionTriggeredEvent : MergeGameEvent
    {
        /// <summary>
        /// 주입 대상 플레이어 인덱스입니다.
        /// </summary>
        public int TargetPlayerIndex { get; }

        /// <summary>
        /// 주입 몬스터 ID입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 한 번에 주입되는 몬스터 수량입니다.
        /// </summary>
        public int InjectedCount { get; }

        /// <summary>
        /// 주입을 유도한 소스 몬스터 UID입니다. (없으면 0)
        /// </summary>
        public long SourceMonsterUid { get; }

        /// <summary>
        /// 소스 위치가 유효한지 여부입니다.
        /// </summary>
        public bool HasSourcePosition { get; }

        /// <summary>
        /// 소스 위치입니다.
        /// </summary>
        public float SourceX { get; }
        /// <summary>
        /// SourceY 속성입니다.
        /// </summary>
        public float SourceY { get; }
        /// <summary>
        /// SourceZ 속성입니다.
        /// </summary>
        public float SourceZ { get; }

        /// <summary>
        /// 타겟 플레이어 보드에서 주입 몬스터가 스폰되는 시작 위치입니다.
        /// </summary>
        public float TargetSpawnX { get; }
        /// <summary>
        /// TargetSpawnY 속성입니다.
        /// </summary>
        public float TargetSpawnY { get; }
        /// <summary>
        /// TargetSpawnZ 속성입니다.
        /// </summary>
        public float TargetSpawnZ { get; }

        /// <summary>
        /// SourcePlayerIndex 속성입니다.
        /// </summary>
        public int SourcePlayerIndex => PlayerIndex;

        /// <summary>
        /// MonsterInjectionTriggeredEvent 생성자입니다.
        /// </summary>
        public MonsterInjectionTriggeredEvent(
            long tick,
            int sourcePlayerIndex,
            int targetPlayerIndex,
            long monsterId,
            int injectedCount,
            long sourceMonsterUid,
            bool hasSourcePosition,
            float sourceX,
            float sourceY,
            float sourceZ,
            float targetSpawnX,
            float targetSpawnY,
            float targetSpawnZ) : base(tick, sourcePlayerIndex)
        {
            TargetPlayerIndex = targetPlayerIndex;
            MonsterId = monsterId;
            InjectedCount = injectedCount;
            SourceMonsterUid = sourceMonsterUid;
            HasSourcePosition = hasSourcePosition;
            SourceX = sourceX;
            SourceY = sourceY;
            SourceZ = sourceZ;
            TargetSpawnX = targetSpawnX;
            TargetSpawnY = targetSpawnY;
            TargetSpawnZ = targetSpawnZ;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                   + sizeof(int)   // TargetPlayerIndex
                   + sizeof(long)  // MonsterId
                   + sizeof(int)   // InjectedCount
                   + sizeof(long)  // SourceMonsterUid
                   + sizeof(byte)  // HasSourcePosition
                   + sizeof(float) // SourceX
                   + sizeof(float) // SourceY
                   + sizeof(float) // SourceZ
                   + sizeof(float) // TargetSpawnX
                   + sizeof(float) // TargetSpawnY
                   + sizeof(float);// TargetSpawnZ
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), TargetPlayerIndex);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), MonsterId);
            offset += sizeof(long);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), InjectedCount);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset, sizeof(long)), SourceMonsterUid);
            offset += sizeof(long);

            dst[offset] = HasSourcePosition ? (byte)1 : (byte)0;
            offset += sizeof(byte);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(SourceX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(SourceY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(SourceZ));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(TargetSpawnX));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(TargetSpawnY));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(float)), BitConverter.SingleToInt32Bits(TargetSpawnZ));

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MonsterInjectionTriggeredEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, sourcePlayerIndex, offset) = ReadMergeHeader(src);

            int targetPlayerIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            long monsterId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            int injectedCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            long sourceMonsterUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset, sizeof(long)));
            offset += sizeof(long);

            bool hasSourcePosition = src[offset] != 0;
            offset += sizeof(byte);

            float sourceX = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float sourceY = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float sourceZ = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetSpawnX = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetSpawnY = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));
            offset += sizeof(float);

            float targetSpawnZ = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(float))));

            return new MonsterInjectionTriggeredEvent(
                tick,
                sourcePlayerIndex,
                targetPlayerIndex,
                monsterId,
                injectedCount,
                sourceMonsterUid,
                hasSourcePosition,
                sourceX,
                sourceY,
                sourceZ,
                targetSpawnX,
                targetSpawnY,
                targetSpawnZ);
        }
    }

    #endregion

    #region 난이도 이벤트

    /// <summary>
    /// 난이도 스텝 변경 이벤트입니다.
    /// </summary>
    public sealed class DifficultyStepChangedEvent : MergeGameEvent
    {
        /// <summary>
        /// Step 속성입니다.
        /// </summary>
        public int Step { get; }
        /// <summary>
        /// SpawnCount 속성입니다.
        /// </summary>
        public int SpawnCount { get; }
        /// <summary>
        /// HealthMultiplier 속성입니다.
        /// </summary>
        public float HealthMultiplier { get; }
        /// <summary>
        /// SpawnInterval 속성입니다.
        /// </summary>
        public float SpawnInterval { get; }

        /// <summary>
        /// DifficultyStepChangedEvent 생성자입니다.
        /// </summary>
        public DifficultyStepChangedEvent(long tick, int playerIndex, int step, int spawnCount, float healthMultiplier, float spawnInterval)
            : base(tick, playerIndex)
        {
            Step = step;
            SpawnCount = spawnCount;
            HealthMultiplier = healthMultiplier;
            SpawnInterval = spawnInterval;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(int)    // Step
                + sizeof(int)    // SpawnCount
                + sizeof(float)  // HealthMultiplier
                + sizeof(float); // SpawnInterval
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), Step);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SpawnCount);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(HealthMultiplier));
            offset += sizeof(float);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(SpawnInterval));

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static DifficultyStepChangedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            int step = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int spawnCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            float healthMultiplier = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
            offset += sizeof(float);

            float spawnInterval = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));

            return new DifficultyStepChangedEvent(tick, playerIndex, step, spawnCount, healthMultiplier, spawnInterval);
        }
    }

    #endregion

    #region 플레이어 상태 이벤트

    /// <summary>
    /// 플레이어 골드 변경 이벤트입니다.
    /// </summary>
    public sealed class PlayerGoldChangedEvent : MergeGameEvent
    {
        /// <summary>
        /// 현재 골드입니다.
        /// </summary>
        public int CurrentGold { get; }

        /// <summary>
        /// 골드 변화량입니다.
        /// </summary>
        public int GoldDelta { get; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public PlayerGoldChangedEvent(long tick, int playerIndex, int currentGold, int goldDelta) : base(tick, playerIndex)
        {
            CurrentGold = currentGold;
            GoldDelta = goldDelta;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            return base.GetPayloadSize()
                + sizeof(int)   // CurrentGold
                + sizeof(int);  // GoldDelta
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), CurrentGold);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), GoldDelta);

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static PlayerGoldChangedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            int currentGold = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            int goldDelta = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));

            return new PlayerGoldChangedEvent(tick, playerIndex, currentGold, goldDelta);
        }
    }

    #endregion

    #region 맵 이벤트

    /// <summary>
    /// 슬롯 위치 데이터입니다.
    /// </summary>
    public readonly struct SlotPositionData
    {
        /// <summary>
        /// Index 속성입니다.
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// X 속성입니다.
        /// </summary>
        public float X { get; }
        /// <summary>
        /// Y 속성입니다.
        /// </summary>
        public float Y { get; }
        /// <summary>
        /// Z 속성입니다.
        /// </summary>
        public float Z { get; }

        /// <summary>
        /// SlotPositionData 생성자입니다.
        /// </summary>
        public SlotPositionData(int index, float x, float y, float z)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// 경로 웨이포인트 데이터입니다.
    /// </summary>
    public readonly struct PathWaypointData
    {
        /// <summary>
        /// X 속성입니다.
        /// </summary>
        public float X { get; }
        /// <summary>
        /// Y 속성입니다.
        /// </summary>
        public float Y { get; }
        /// <summary>
        /// Z 속성입니다.
        /// </summary>
        public float Z { get; }

        /// <summary>
        /// PathWaypointData 생성자입니다.
        /// </summary>
        public PathWaypointData(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// 경로 데이터입니다.
    /// </summary>
    public sealed class PathData
    {
        /// <summary>
        /// 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 웨이포인트 목록입니다.
        /// </summary>
        public IReadOnlyList<PathWaypointData> Waypoints { get; }

        /// <summary>
        /// PathData 생성자입니다.
        /// </summary>
        public PathData(int pathIndex, IReadOnlyList<PathWaypointData> waypoints)
        {
            PathIndex = pathIndex;
            Waypoints = waypoints;
        }
    }

    /// <summary>
    /// 맵 초기화 이벤트입니다.
    /// View에서 맵을 구성할 때 사용합니다.
    /// </summary>
    public sealed class MapInitializedEvent : MergeGameEvent
    {
        /// <summary>
        /// 맵 ID입니다.
        /// </summary>
        public int MapId { get; }

        /// <summary>
        /// 슬롯 위치 데이터 목록입니다.
        /// </summary>
        public IReadOnlyList<SlotPositionData> SlotPositions { get; }

        /// <summary>
        /// 경로 데이터 목록입니다.
        /// </summary>
        public IReadOnlyList<PathData> Paths { get; }

        /// <summary>
        /// MapInitializedEvent 생성자입니다.
        /// </summary>
        public MapInitializedEvent(
            long tick,
            int playerIndex,
            int mapId,
            IReadOnlyList<SlotPositionData> slotPositions,
            IReadOnlyList<PathData> paths) : base(tick, playerIndex)
        {
            MapId = mapId;
            SlotPositions = slotPositions;
            Paths = paths;
        }
        /// <summary>
        /// GetPayloadSize 메서드입니다.
        /// </summary>

        protected override int GetPayloadSize()
        {
            var size = base.GetPayloadSize()
                + sizeof(int)   // MapId
                + sizeof(int)   // SlotPositions.Count
                + SlotPositions.Count * (sizeof(int) + sizeof(float) * 3) // 각 슬롯: Index + XYZ
                + sizeof(int);  // Paths.Count

            for (int i = 0; i < Paths.Count; i++)
            {
                size += sizeof(int)   // PathIndex
                    + sizeof(int)     // Waypoints.Count
                    + Paths[i].Waypoints.Count * (sizeof(float) * 3); // 각 웨이포인트: XYZ
            }

            return size;
        }
        /// <summary>
        /// WritePayload 메서드입니다.
        /// </summary>

        protected override int WritePayload(Span<byte> dst)
        {
            var offset = base.WritePayload(dst);

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), MapId);
            offset += sizeof(int);

            // 슬롯 데이터
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), SlotPositions.Count);
            offset += sizeof(int);

            for (int i = 0; i < SlotPositions.Count; i++)
            {
                var slot = SlotPositions[i];
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), slot.Index);
                offset += sizeof(int);

                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(slot.X));
                offset += sizeof(float);

                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(slot.Y));
                offset += sizeof(float);

                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(slot.Z));
                offset += sizeof(float);
            }

            // 경로 데이터
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), Paths.Count);
            offset += sizeof(int);

            for (int i = 0; i < Paths.Count; i++)
            {
                var path = Paths[i];
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), path.PathIndex);
                offset += sizeof(int);

                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), path.Waypoints.Count);
                offset += sizeof(int);

                for (int j = 0; j < path.Waypoints.Count; j++)
                {
                    var wp = path.Waypoints[j];
                    BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(wp.X));
                    offset += sizeof(float);

                    BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(wp.Y));
                    offset += sizeof(float);

                    BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset, sizeof(int)), BitConverter.SingleToInt32Bits(wp.Z));
                    offset += sizeof(float);
                }
            }

            return GetPayloadSize();
        }
        /// <summary>
        /// ReadFrom 메서드입니다.
        /// </summary>

        public static MapInitializedEvent ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, playerIndex, offset) = ReadMergeHeader(src);

            int mapId = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            // 슬롯 데이터
            int slotCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            var slots = new SlotPositionData[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                int index = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
                offset += sizeof(int);

                float x = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                offset += sizeof(float);

                float y = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                offset += sizeof(float);

                float z = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                offset += sizeof(float);

                slots[i] = new SlotPositionData(index, x, y, z);
            }

            // 경로 데이터
            int pathCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            var paths = new PathData[pathCount];
            for (int i = 0; i < pathCount; i++)
            {
                int pathIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
                offset += sizeof(int);

                int wpCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int)));
                offset += sizeof(int);

                var waypoints = new PathWaypointData[wpCount];
                for (int j = 0; j < wpCount; j++)
                {
                    float wx = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                    offset += sizeof(float);

                    float wy = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                    offset += sizeof(float);

                    float wz = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset, sizeof(int))));
                    offset += sizeof(float);

                    waypoints[j] = new PathWaypointData(wx, wy, wz);
                }
                paths[i] = new PathData(pathIndex, waypoints);
            }

            return new MapInitializedEvent(tick, playerIndex, mapId, slots, paths);
        }
    }

    #endregion
}






