using System.Collections.Generic;
using Noname.GameHost;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 이벤트의 기본 타입입니다.
    /// </summary>
    public abstract class MergeHostEvent : GameEventBase
    {
        protected MergeHostEvent(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 게임 시작 이벤트입니다.
    /// </summary>
    public sealed class MergeGameStartedEvent : MergeHostEvent
    {
        /// <summary>
        /// 초기 슬롯 개수입니다.
        /// </summary>
        public int SlotCount { get; }

        public MergeGameStartedEvent(long tick, int slotCount) : base(tick)
        {
            SlotCount = slotCount;
        }
    }

    /// <summary>
    /// 유닛 스폰 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitSpawnedEvent : MergeHostEvent
    {
        /// <summary>
        /// 생성된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 유닛 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitSpawnedEvent(long tick, long unitUid, int grade, int slotIndex) : base(tick)
        {
            UnitUid = unitUid;
            Grade = grade;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 머지 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitMergedEvent : MergeHostEvent
    {
        /// <summary>
        /// 머지에 사용된 유닛1 UID입니다.
        /// </summary>
        public long SourceUnitUid1 { get; }

        /// <summary>
        /// 머지에 사용된 유닛2 UID입니다.
        /// </summary>
        public long SourceUnitUid2 { get; }

        /// <summary>
        /// 머지 결과 유닛 UID입니다.
        /// </summary>
        public long ResultUnitUid { get; }

        /// <summary>
        /// 머지 결과 유닛 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 유닛이 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitMergedEvent(
            long tick,
            long sourceUnitUid1,
            long sourceUnitUid2,
            long resultUnitUid,
            int resultGrade,
            int slotIndex) : base(tick)
        {
            SourceUnitUid1 = sourceUnitUid1;
            SourceUnitUid2 = sourceUnitUid2;
            ResultUnitUid = resultUnitUid;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 제거 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitRemovedEvent : MergeHostEvent
    {
        /// <summary>
        /// 제거된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 제거된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitRemovedEvent(long tick, long unitUid, int slotIndex) : base(tick)
        {
            UnitUid = unitUid;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 게임 오버 이벤트입니다.
    /// </summary>
    public sealed class MergeGameOverEvent : MergeHostEvent
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

        public MergeGameOverEvent(long tick, bool isVictory, int finalScore, int maxGradeReached) : base(tick)
        {
            IsVictory = isVictory;
            FinalScore = finalScore;
            MaxGradeReached = maxGradeReached;
        }
    }

    /// <summary>
    /// 점수 변경 이벤트입니다.
    /// </summary>
    public sealed class MergeScoreChangedEvent : MergeHostEvent
    {
        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 점수 변화량입니다.
        /// </summary>
        public int ScoreDelta { get; }

        public MergeScoreChangedEvent(long tick, int currentScore, int scoreDelta) : base(tick)
        {
            CurrentScore = currentScore;
            ScoreDelta = scoreDelta;
        }
    }

    #region 캐릭터 이벤트

    /// <summary>
    /// 캐릭터 스폰 이벤트입니다.
    /// </summary>
    public sealed class TowerSpawnedEvent : MergeHostEvent
    {
        /// <summary>
        /// 캐릭터 고유 ID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 캐릭터 정의 ID입니다.
        /// </summary>
        public string TowerId { get; }

        /// <summary>
        /// 캐릭터 타입입니다.
        /// </summary>
        public string TowerType { get; }

        /// <summary>
        /// 캐릭터 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 배치된 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
        public float PositionZ { get; }

        public TowerSpawnedEvent(
            long tick,
            long towerUid,
            string towerId,
            string towerType,
            int grade,
            int slotIndex,
            float positionX,
            float positionY,
            float positionZ) : base(tick)
        {
            TowerUid = towerUid;
            TowerId = towerId;
            TowerType = towerType;
            Grade = grade;
            SlotIndex = slotIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
        }
    }

    /// <summary>
    /// 캐릭터 공격 이벤트입니다.
    /// </summary>
    public sealed class TowerAttackedEvent : MergeHostEvent
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
        public float AttackerY { get; }
        public float AttackerZ { get; }

        /// <summary>
        /// 대상 위치입니다.
        /// </summary>
        public float TargetX { get; }
        public float TargetY { get; }
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

        public TowerAttackedEvent(
            long tick,
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
            float throwRadius) : base(tick)
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
        }
    }

    /// <summary>
    /// 캐릭터 제거 이벤트입니다.
    /// </summary>
    public sealed class TowerRemovedEvent : MergeHostEvent
    {
        /// <summary>
        /// 제거된 캐릭터 UID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 제거된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 제거 사유입니다.
        /// </summary>
        public string Reason { get; }

        public TowerRemovedEvent(long tick, long towerUid, int slotIndex, string reason) : base(tick)
        {
            TowerUid = towerUid;
            SlotIndex = slotIndex;
            Reason = reason;
        }
    }

    /// <summary>
    /// 캐릭터 머지 이벤트입니다.
    /// </summary>
    public sealed class TowerMergedEvent : MergeHostEvent
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
        /// 결과 캐릭터 UID입니다.
        /// </summary>
        public long ResultTowerUid { get; }

        /// <summary>
        /// 결과 캐릭터 정의 ID입니다.
        /// </summary>
        public string ResultTowerId { get; }

        /// <summary>
        /// 결과 캐릭터 타입입니다.
        /// </summary>
        public string ResultTowerType { get; }

        /// <summary>
        /// 결과 캐릭터 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 캐릭터가 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public TowerMergedEvent(
            long tick,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid,
            string resultTowerId,
            string resultTowerType,
            int resultGrade,
            int slotIndex) : base(tick)
        {
            SourceTowerUid = sourceTowerUid;
            TargetTowerUid = targetTowerUid;
            ResultTowerUid = resultTowerUid;
            ResultTowerId = resultTowerId;
            ResultTowerType = resultTowerType;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 캐릭터 이동 이벤트입니다.
    /// </summary>
    public sealed class TowerMovedEvent : MergeHostEvent
    {
        /// <summary>
        /// 이동한 캐릭터 UID입니다.
        /// </summary>
        public long TowerUid { get; }

        /// <summary>
        /// 이동 전 슬롯 인덱스입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 이동 후 슬롯 인덱스입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        /// <summary>
        /// 새 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
        public float PositionZ { get; }

        public TowerMovedEvent(
            long tick,
            long towerUid,
            int fromSlotIndex,
            int toSlotIndex,
            float positionX,
            float positionY,
            float positionZ) : base(tick)
        {
            TowerUid = towerUid;
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
        }
    }

    /// <summary>
    /// 머지 이펙트 발동 이벤트입니다.
    /// </summary>
    public sealed class MergeEffectTriggeredEvent : MergeHostEvent
    {
        /// <summary>
        /// 이펙트 ID입니다.
        /// </summary>
        public string EffectId { get; }

        /// <summary>
        /// 이펙트 발동 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
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

        public MergeEffectTriggeredEvent(
            long tick,
            string effectId,
            float positionX,
            float positionY,
            float positionZ,
            bool isSourceEffect,
            long sourceTowerUid,
            long targetTowerUid,
            long resultTowerUid) : base(tick)
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
    }

    #endregion

    #region 몬스터 이벤트

    /// <summary>
    /// 몬스터 스폰 이벤트입니다.
    /// </summary>
    public sealed class MonsterSpawnedEvent : MergeHostEvent
    {
        /// <summary>
        /// 몬스터 고유 ID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 몬스터 정의 ID입니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 이동 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 스폰 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
        public float PositionZ { get; }

        /// <summary>
        /// 최대 체력입니다.
        /// </summary>
        public float MaxHealth { get; }

        public MonsterSpawnedEvent(
            long tick,
            long monsterUid,
            string monsterId,
            int pathIndex,
            float positionX,
            float positionY,
            float positionZ,
            float maxHealth) : base(tick)
        {
            MonsterUid = monsterUid;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            MaxHealth = maxHealth;
        }
    }

    /// <summary>
    /// 몬스터 데미지 이벤트입니다.
    /// </summary>
    public sealed class MonsterDamagedEvent : MergeHostEvent
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

        public MonsterDamagedEvent(
            long tick,
            long monsterUid,
            float damage,
            float currentHealth,
            long attackerUid) : base(tick)
        {
            MonsterUid = monsterUid;
            Damage = damage;
            CurrentHealth = currentHealth;
            AttackerUid = attackerUid;
        }
    }

    /// <summary>
    /// 몬스터 사망 이벤트입니다.
    /// </summary>
    public sealed class MonsterDiedEvent : MergeHostEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 사망 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
        public float PositionZ { get; }

        /// <summary>
        /// 획득 골드입니다.
        /// </summary>
        public int GoldReward { get; }

        /// <summary>
        /// 처치한 캐릭터 UID입니다 (0이면 직접 처치).
        /// </summary>
        public long KillerUid { get; }

        public MonsterDiedEvent(
            long tick,
            long monsterUid,
            float positionX,
            float positionY,
            float positionZ,
            int goldReward,
            long killerUid) : base(tick)
        {
            MonsterUid = monsterUid;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            GoldReward = goldReward;
            KillerUid = killerUid;
        }
    }

    /// <summary>
    /// 몬스터 목적지 도달 이벤트입니다.
    /// </summary>
    public sealed class MonsterReachedGoalEvent : MergeHostEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 플레이어에게 주는 데미지입니다.
        /// </summary>
        public int DamageToPlayer { get; }

        public MonsterReachedGoalEvent(long tick, long monsterUid, int damageToPlayer) : base(tick)
        {
            MonsterUid = monsterUid;
            DamageToPlayer = damageToPlayer;
        }
    }

    /// <summary>
    /// 몬스터 이동 이벤트입니다.
    /// </summary>
    public sealed class MonsterMovedEvent : MergeHostEvent
    {
        /// <summary>
        /// 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 새 위치입니다.
        /// </summary>
        public float PositionX { get; }
        public float PositionY { get; }
        public float PositionZ { get; }

        /// <summary>
        /// 경로 진행도입니다 (0.0 ~ 1.0).
        /// </summary>
        public float PathProgress { get; }

        public MonsterMovedEvent(
            long tick,
            long monsterUid,
            float positionX,
            float positionY,
            float positionZ,
            float pathProgress) : base(tick)
        {
            MonsterUid = monsterUid;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            PathProgress = pathProgress;
        }
    }

    #endregion

    #region 웨이브 이벤트

    /// <summary>
    /// 웨이브 시작 이벤트입니다.
    /// </summary>
    public sealed class WaveStartedEvent : MergeHostEvent
    {
        /// <summary>
        /// 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 이 웨이브의 총 몬스터 수입니다.
        /// </summary>
        public int TotalMonsterCount { get; }

        public WaveStartedEvent(long tick, int waveNumber, int totalMonsterCount) : base(tick)
        {
            WaveNumber = waveNumber;
            TotalMonsterCount = totalMonsterCount;
        }
    }

    /// <summary>
    /// 웨이브 완료 이벤트입니다.
    /// </summary>
    public sealed class WaveCompletedEvent : MergeHostEvent
    {
        /// <summary>
        /// 완료된 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 보너스 골드입니다.
        /// </summary>
        public int BonusGold { get; }

        public WaveCompletedEvent(long tick, int waveNumber, int bonusGold) : base(tick)
        {
            WaveNumber = waveNumber;
            BonusGold = bonusGold;
        }
    }

    #endregion

    #region 플레이어 상태 이벤트

    /// <summary>
    /// 플레이어 HP 변경 이벤트입니다.
    /// </summary>
    public sealed class PlayerHpChangedEvent : MergeHostEvent
    {
        /// <summary>
        /// 현재 HP입니다.
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// 최대 HP입니다.
        /// </summary>
        public int MaxHp { get; }

        /// <summary>
        /// HP 변화량입니다 (음수면 데미지).
        /// </summary>
        public int HpDelta { get; }

        /// <summary>
        /// 변화 원인입니다.
        /// </summary>
        public string Reason { get; }

        public PlayerHpChangedEvent(long tick, int currentHp, int maxHp, int hpDelta, string reason) : base(tick)
        {
            CurrentHp = currentHp;
            MaxHp = maxHp;
            HpDelta = hpDelta;
            Reason = reason;
        }
    }

    /// <summary>
    /// 플레이어 골드 변경 이벤트입니다.
    /// </summary>
    public sealed class PlayerGoldChangedEvent : MergeHostEvent
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
        /// 변화 원인입니다.
        /// </summary>
        public string Reason { get; }

        public PlayerGoldChangedEvent(long tick, int currentGold, int goldDelta, string reason) : base(tick)
        {
            CurrentGold = currentGold;
            GoldDelta = goldDelta;
            Reason = reason;
        }
    }

    #endregion

    #region 맵 이벤트

    /// <summary>
    /// 슬롯 위치 데이터입니다.
    /// </summary>
    public readonly struct SlotPositionData
    {
        public int Index { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

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
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

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
    public sealed class MapInitializedEvent : MergeHostEvent
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

        public MapInitializedEvent(
            long tick,
            int mapId,
            IReadOnlyList<SlotPositionData> slotPositions,
            IReadOnlyList<PathData> paths) : base(tick)
        {
            MapId = mapId;
            SlotPositions = slotPositions;
            Paths = paths;
        }
    }

    #endregion
}