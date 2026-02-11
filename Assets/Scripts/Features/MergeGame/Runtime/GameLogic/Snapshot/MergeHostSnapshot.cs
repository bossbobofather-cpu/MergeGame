﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Noname.GameHost;

namespace MyProject.MergeGame.Snapshots
{
    // MergeSessionPhase는 Enums/MergeEnums.cs에 정의됨

    /// <summary>
    /// 슬롯 정보 스냅샷입니다.
    /// </summary>
    public readonly struct SlotSnapshot
    {
        /// <summary>
        /// 슬롯 인덱스입니다.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 유닛 UID입니다. 비어있으면 0입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 유닛 등급입니다. 비어있으면 0입니다.
        /// </summary>
        public int TowerGrade { get; }

        /// <summary>
        /// 슬롯이 비어있는지 여부입니다.
        /// </summary>
        public bool IsEmpty => TowerUid == 0;

        public SlotSnapshot(int index, long unitUid, int unitGrade)
        {
            Index = index;
            TowerUid = unitUid;
            TowerGrade = unitGrade;
        }
    }

    /// <summary>
    /// 캐릭터 스냅샷입니다.
    /// </summary>
    public readonly struct TowerSnapshot
    {
        /// <summary>
        /// 캐릭터 고유 ID입니다.
        /// </summary>
        public long Uid { get; }

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
        /// X 좌표입니다.
        /// </summary>
        public float PositionX { get; }

        /// <summary>
        /// Y 좌표입니다.
        /// </summary>
        public float PositionY { get; }

        /// <summary>
        /// Z 좌표입니다.
        /// </summary>
        public float PositionZ { get; }

        /// <summary>
        /// 공격력입니다.
        /// </summary>
        public float AttackDamage { get; }

        /// <summary>
        /// 공격 속도입니다.
        /// </summary>
        public float AttackSpeed { get; }

        /// <summary>
        /// 공격 범위입니다.
        /// </summary>
        public float AttackRange { get; }

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

        public TowerSnapshot(
            long uid,
            long towerId,
            int grade,
            int slotIndex,
            float positionX,
            float positionY,
            float positionZ,
            float attackDamage,
            float attackSpeed,
            float attackRange,
            TowerAttackType attackType,
            ProjectileType projectileType,
            float projectileSpeed,
            float throwRadius)
        {
            Uid = uid;
            TowerId = towerId;
            Grade = grade;
            SlotIndex = slotIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            AttackDamage = attackDamage;
            AttackSpeed = attackSpeed;
            AttackRange = attackRange;
            AttackType = attackType;
            ProjectileType = projectileType;
            ProjectileSpeed = projectileSpeed;
            ThrowRadius = throwRadius;
        }
    }

    /// <summary>
    /// 몬스터 스냅샷입니다.
    /// </summary>
    public readonly struct MonsterSnapshot
    {
        /// <summary>
        /// 몬스터 고유 ID입니다.
        /// </summary>
        public long Uid { get; }

        /// <summary>
        /// 몬스터 정의 ID입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 이동 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 경로 진행도입니다 (0.0 ~ 1.0).
        /// </summary>
        public float PathProgress { get; }

        /// <summary>
        /// X 좌표입니다.
        /// </summary>
        public float PositionX { get; }

        /// <summary>
        /// Y 좌표입니다.
        /// </summary>
        public float PositionY { get; }

        /// <summary>
        /// Z 좌표입니다.
        /// </summary>
        public float PositionZ { get; }

        /// <summary>
        /// 현재 체력입니다.
        /// </summary>
        public float CurrentHealth { get; }

        /// <summary>
        /// 최대 체력입니다.
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// 체력 비율입니다 (0.0 ~ 1.0).
        /// </summary>
        public float HealthRatio => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;

        public MonsterSnapshot(
            long uid,
            long monsterId,
            int pathIndex,
            float pathProgress,
            float positionX,
            float positionY,
            float positionZ,
            float currentHealth,
            float maxHealth)
        {
            Uid = uid;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            PathProgress = pathProgress;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    /// <summary>
    /// 투사체 스냅샷입니다.
    /// </summary>
    public readonly struct ProjectileSnapshot
    {
        public long Uid { get; }
        public float StartX { get; }
        public float StartY { get; }
        public float StartZ { get; }
        public float ImpactX { get; }
        public float ImpactY { get; }
        public float ImpactZ { get; }
        public float Progress { get; }
        public ProjectileType ProjectileType { get; }
        public bool IsLanded { get; }

        public ProjectileSnapshot(
            long uid,
            float startX, float startY, float startZ,
            float impactX, float impactY, float impactZ,
            float progress,
            ProjectileType projectileType,
            bool isLanded)
        {
            Uid = uid;
            StartX = startX;
            StartY = startY;
            StartZ = startZ;
            ImpactX = impactX;
            ImpactY = impactY;
            ImpactZ = impactZ;
            Progress = progress;
            ProjectileType = projectileType;
            IsLanded = isLanded;
        }
    }

    /// <summary>
    /// MergeGame 인스턴스 상태를 담는 스냅샷입니다.
    /// View/클라이언트에 이 데이터를 전달해 화면을 갱신합니다.
    /// </summary>
    public sealed class MergeHostSnapshot : GameSnapshotBase
    {
        /// <summary>
        /// 플레이어 인덱스입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// 현재 세션 단계입니다.
        /// </summary>
        public MergeSessionPhase SessionPhase { get; }

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// 도달한 최고 등급입니다.
        /// </summary>
        public int MaxGrade { get; }

        /// <summary>
        /// 전체 슬롯 개수입니다.
        /// </summary>
        public int TotalSlots { get; }

        /// <summary>
        /// 사용 중인 슬롯 개수입니다.
        /// </summary>
        public int UsedSlots { get; }

        /// <summary>
        /// 경과 시간(초)입니다.
        /// </summary>
        public float ElapsedTime { get; }

        /// <summary>
        /// 슬롯 상태 목록입니다.
        /// </summary>
        public IReadOnlyList<SlotSnapshot> Slots { get; }

        /// <summary>
        /// 플레이어 골드입니다.
        /// </summary>
        public int PlayerGold { get; }

        /// <summary>
        /// 현재 난이도 스텝입니다.
        /// </summary>
        public int DifficultyStep { get; }

        /// <summary>
        /// 현재 스폰 수입니다.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// 현재 체력 배율입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// 현재 스폰 간격입니다.
        /// </summary>
        public float SpawnInterval { get; }

        /// <summary>
        /// 몬스터 누적 패배 기준 수량입니다.
        /// </summary>
        public int MaxMonsterStack { get; }


        /// <summary>
        /// 캐릭터 목록입니다.
        /// </summary>
        public IReadOnlyList<TowerSnapshot> Towers { get; }

        /// <summary>
        /// 몬스터 목록입니다.
        /// </summary>
        public IReadOnlyList<MonsterSnapshot> Monsters { get; }

        /// <summary>
        /// 활성 투사체 목록입니다.
        /// </summary>
        public IReadOnlyList<ProjectileSnapshot> Projectiles { get; }

        /// <summary>
        /// 전체 생성자입니다.
        /// </summary>
        public MergeHostSnapshot(
            long tick,
            int playerIndex,
            MergeSessionPhase sessionPhase,
            int score,
            int maxGrade,
            int totalSlots,
            int usedSlots,
            float elapsedTime,
            IReadOnlyList<SlotSnapshot> slots,
            int playerGold,
            int difficultyStep,
            int spawnCount,
            float healthMultiplier,
            float spawnInterval,
            int maxMonsterStack,
            IReadOnlyList<TowerSnapshot> towers,
            IReadOnlyList<MonsterSnapshot> monsters,
            IReadOnlyList<ProjectileSnapshot> projectiles = null) : base(tick)
        {
            PlayerIndex = playerIndex;
            SessionPhase = sessionPhase;
            Score = score;
            MaxGrade = maxGrade;
            TotalSlots = totalSlots;
            UsedSlots = usedSlots;
            ElapsedTime = elapsedTime;
            Slots = slots;
            PlayerGold = playerGold;
            DifficultyStep = difficultyStep;
            SpawnCount = spawnCount;
            HealthMultiplier = healthMultiplier;
            SpawnInterval = spawnInterval;
            MaxMonsterStack = maxMonsterStack;
            Towers = towers;
            Monsters = monsters;
            Projectiles = projectiles ?? Array.Empty<ProjectileSnapshot>();
        }
        protected override int GetPayloadSize()
        {
            return
                // Fixed header fields
                sizeof(int) // PlayerIndex
                + sizeof(int) // SessionPhase
                + sizeof(int) // Score
                + sizeof(int) // MaxGrade
                + sizeof(int) // TotalSlots
                + sizeof(int) // UsedSlots
                + sizeof(float) // ElapsedTime
                + sizeof(int) // PlayerGold
                + sizeof(int) // DifficultyStep
                + sizeof(int) // SpawnCount
                + sizeof(float) // HealthMultiplier
                + sizeof(float) // SpawnInterval
                + sizeof(int) // MaxMonsterStack

                // Slots
                + sizeof(int) // Slots.Count
                + Slots.Count * (
                    sizeof(int) // Slot.Index
                    + sizeof(long) // Slot.TowerUid
                    + sizeof(int) // Slot.TowerGrade
                )

                // Towers
                + sizeof(int) // Towers.Count
                + Towers.Count * (
                    sizeof(long) // Tower.Uid
                    + sizeof(long) // Tower.TowerId
                    + sizeof(int) // Tower.Grade
                    + sizeof(int) // Tower.SlotIndex
                    + sizeof(float) // Tower.PositionX
                    + sizeof(float) // Tower.PositionY
                    + sizeof(float) // Tower.PositionZ
                    + sizeof(float) // Tower.AttackDamage
                    + sizeof(float) // Tower.AttackSpeed
                    + sizeof(float) // Tower.AttackRange
                    + sizeof(int) // Tower.AttackType
                    + sizeof(int) // Tower.ProjectileType
                    + sizeof(float) // Tower.ProjectileSpeed
                    + sizeof(float) // Tower.ThrowRadius
                )

                // Monsters
                + sizeof(int) // Monsters.Count
                + Monsters.Count * (
                    sizeof(long) // Monster.Uid
                    + sizeof(long) // Monster.MonsterId
                    + sizeof(int) // Monster.PathIndex
                    + sizeof(float) // Monster.PathProgress
                    + sizeof(float) // Monster.PositionX
                    + sizeof(float) // Monster.PositionY
                    + sizeof(float) // Monster.PositionZ
                    + sizeof(float) // Monster.CurrentHealth
                    + sizeof(float) // Monster.MaxHealth
                )

                // Projectiles
                + sizeof(int) // Projectiles.Count
                + Projectiles.Count * (
                    sizeof(long) // Projectile.Uid
                    + sizeof(float) // Projectile.StartX
                    + sizeof(float) // Projectile.StartY
                    + sizeof(float) // Projectile.StartZ
                    + sizeof(float) // Projectile.ImpactX
                    + sizeof(float) // Projectile.ImpactY
                    + sizeof(float) // Projectile.ImpactZ
                    + sizeof(float) // Projectile.Progress
                    + sizeof(int) // Projectile.ProjectileType
                    + sizeof(byte) // Projectile.IsLanded
                );
        }


        protected override int WritePayload(Span<byte> dst)
        {
            var offset = 0;

            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), PlayerIndex); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), (int)SessionPhase); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), Score); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), MaxGrade); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), TotalSlots); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), UsedSlots); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(ElapsedTime)); offset += sizeof(float);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), PlayerGold); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), DifficultyStep); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), SpawnCount); offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(HealthMultiplier)); offset += sizeof(float);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(SpawnInterval)); offset += sizeof(float);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), MaxMonsterStack); offset += sizeof(int);

            // Slots
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), Slots.Count); offset += sizeof(int);
            for (int i = 0; i < Slots.Count; i++)
            {
                var s = Slots[i];
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), s.Index); offset += sizeof(int);
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), s.TowerUid); offset += sizeof(long);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), s.TowerGrade); offset += sizeof(int);
            }

            // Towers
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), Towers.Count); offset += sizeof(int);
            for (int i = 0; i < Towers.Count; i++)
            {
                var t = Towers[i];
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), t.Uid); offset += sizeof(long);
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), t.TowerId); offset += sizeof(long);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), t.Grade); offset += sizeof(int);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), t.SlotIndex); offset += sizeof(int);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.PositionX)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.PositionY)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.PositionZ)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.AttackDamage)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.AttackSpeed)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.AttackRange)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), (int)t.AttackType); offset += sizeof(int);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), (int)t.ProjectileType); offset += sizeof(int);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.ProjectileSpeed)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(t.ThrowRadius)); offset += sizeof(float);
            }

            // Monsters
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), Monsters.Count); offset += sizeof(int);
            for (int i = 0; i < Monsters.Count; i++)
            {
                var m = Monsters[i];
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), m.Uid); offset += sizeof(long);
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), m.MonsterId); offset += sizeof(long);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), m.PathIndex); offset += sizeof(int);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.PathProgress)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.PositionX)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.PositionY)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.PositionZ)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.CurrentHealth)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(m.MaxHealth)); offset += sizeof(float);
            }

            // Projectiles
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), Projectiles.Count); offset += sizeof(int);
            for (int i = 0; i < Projectiles.Count; i++)
            {
                var p = Projectiles[i];
                BinaryPrimitives.WriteInt64LittleEndian(dst.Slice(offset), p.Uid); offset += sizeof(long);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.StartX)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.StartY)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.StartZ)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.ImpactX)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.ImpactY)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.ImpactZ)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), BitConverter.SingleToInt32Bits(p.Progress)); offset += sizeof(float);
                BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(offset), (int)p.ProjectileType); offset += sizeof(int);
                dst[offset] = p.IsLanded ? (byte)1 : (byte)0; offset += sizeof(byte);
            }

            return GetPayloadSize();
        }

        public static MergeHostSnapshot ReadFrom(ReadOnlySpan<byte> src)
        {
            var (tick, headerOffset) = ReadHeader(src);
            var offset = headerOffset;

            int playerIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            var sessionPhase = (MergeSessionPhase)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int score = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int maxGrade = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int totalSlots = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int usedSlots = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            float elapsedTime = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
            int playerGold = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int difficultyStep = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            int spawnCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            float healthMultiplier = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
            float spawnInterval = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
            int maxMonsterStack = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);

            // Slots
            int slotCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            var slots = new SlotSnapshot[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                int idx = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                long tUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                int tGrade = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                slots[i] = new SlotSnapshot(idx, tUid, tGrade);
            }

            // Towers
            int towerCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            var towers = new TowerSnapshot[towerCount];
            for (int i = 0; i < towerCount; i++)
            {
                long uid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                long towerId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                int grade = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                int slotIndex = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                float px = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float py = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float pz = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float atkDmg = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float atkSpd = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float atkRng = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                var atkType = (TowerAttackType)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                var projType = (ProjectileType)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                float projSpd = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float throwRad = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                towers[i] = new TowerSnapshot(uid, towerId, grade, slotIndex, px, py, pz, atkDmg, atkSpd, atkRng, atkType, projType, projSpd, throwRad);
            }

            // Monsters
            int monsterCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            var monsters = new MonsterSnapshot[monsterCount];
            for (int i = 0; i < monsterCount; i++)
            {
                long uid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                long mId = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                int pathIdx = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                float progress = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float px = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float py = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float pz = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float hp = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float maxHp = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                monsters[i] = new MonsterSnapshot(uid, mId, pathIdx, progress, px, py, pz, hp, maxHp);
            }

            // Projectiles
            int projCount = BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
            var projectiles = new ProjectileSnapshot[projCount];
            for (int i = 0; i < projCount; i++)
            {
                long pUid = BinaryPrimitives.ReadInt64LittleEndian(src.Slice(offset)); offset += sizeof(long);
                float sx = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float sy = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float sz = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float ix = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float iy = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float iz = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                float prog = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset))); offset += sizeof(float);
                var pType = (ProjectileType)BinaryPrimitives.ReadInt32LittleEndian(src.Slice(offset)); offset += sizeof(int);
                bool landed = src[offset] != 0; offset += sizeof(byte);
                projectiles[i] = new ProjectileSnapshot(pUid, sx, sy, sz, ix, iy, iz, prog, pType, landed);
            }

            return new MergeHostSnapshot(tick, playerIndex, sessionPhase, score, maxGrade,
                totalSlots, usedSlots, elapsedTime, slots, playerGold,
                difficultyStep, spawnCount, healthMultiplier, spawnInterval, maxMonsterStack, towers, monsters, projectiles);
        }
    }
}

