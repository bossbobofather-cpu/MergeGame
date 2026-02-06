using System.Collections.Generic;
using Noname.GameHost;

namespace MyProject.MergeGame
{
    // MergeSessionPhase, WavePhase는 Shared/Enums/MergeEnums.cs에 정의됨

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
        /// X 좌표입니다.
        /// </summary>
        public float PositionX { get; }

        /// <summary>
        /// Y 좌표입니다.
        /// </summary>
        public float PositionY { get; }

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

        public TowerSnapshot(
            long uid,
            string towerId,
            string towerType,
            int grade,
            int slotIndex,
            float positionX,
            float positionY,
            float attackDamage,
            float attackSpeed,
            float attackRange)
        {
            Uid = uid;
            TowerId = towerId;
            TowerType = towerType;
            Grade = grade;
            SlotIndex = slotIndex;
            PositionX = positionX;
            PositionY = positionY;
            AttackDamage = attackDamage;
            AttackSpeed = attackSpeed;
            AttackRange = attackRange;
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
        public string MonsterId { get; }

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
            string monsterId,
            int pathIndex,
            float pathProgress,
            float positionX,
            float positionY,
            float currentHealth,
            float maxHealth)
        {
            Uid = uid;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            PathProgress = pathProgress;
            PositionX = positionX;
            PositionY = positionY;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    /// <summary>
    /// MergeGame 인스턴스 상태를 담는 스냅샷입니다.
    /// View/클라이언트에 이 데이터를 전달해 화면을 갱신합니다.
    /// </summary>
    public sealed class MergeHostSnapshot : GameSnapshotBase
    {
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
        /// 플레이어 현재 HP입니다.
        /// </summary>
        public int PlayerHp { get; }

        /// <summary>
        /// 플레이어 최대 HP입니다.
        /// </summary>
        public int PlayerMaxHp { get; }

        /// <summary>
        /// 플레이어 HP 비율입니다 (0.0 ~ 1.0).
        /// </summary>
        public float PlayerHpRatio => PlayerMaxHp > 0 ? (float)PlayerHp / PlayerMaxHp : 0f;

        /// <summary>
        /// 플레이어 골드입니다.
        /// </summary>
        public int PlayerGold { get; }

        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
        public int CurrentWaveNumber { get; }

        /// <summary>
        /// 웨이브 단계입니다.
        /// </summary>
        public WavePhase WavePhase { get; }

        /// <summary>
        /// 캐릭터 목록입니다.
        /// </summary>
        public IReadOnlyList<TowerSnapshot> Towers { get; }

        /// <summary>
        /// 몬스터 목록입니다.
        /// </summary>
        public IReadOnlyList<MonsterSnapshot> Monsters { get; }

        /// <summary>
        /// 기존 생성자 (하위 호환용).
        /// </summary>
        public MergeHostSnapshot(
            long tick,
            MergeSessionPhase sessionPhase,
            int score,
            int maxGrade,
            int totalSlots,
            int usedSlots,
            float elapsedTime,
            IReadOnlyList<SlotSnapshot> slots) : base(tick)
        {
            SessionPhase = sessionPhase;
            Score = score;
            MaxGrade = maxGrade;
            TotalSlots = totalSlots;
            UsedSlots = usedSlots;
            ElapsedTime = elapsedTime;
            Slots = slots;

            // 기본값
            PlayerHp = 0;
            PlayerMaxHp = 0;
            PlayerGold = 0;
            CurrentWaveNumber = 0;
            WavePhase = WavePhase.Idle;
            Towers = System.Array.Empty<TowerSnapshot>();
            Monsters = System.Array.Empty<MonsterSnapshot>();
        }

        /// <summary>
        /// 전체 생성자입니다.
        /// </summary>
        public MergeHostSnapshot(
            long tick,
            MergeSessionPhase sessionPhase,
            int score,
            int maxGrade,
            int totalSlots,
            int usedSlots,
            float elapsedTime,
            IReadOnlyList<SlotSnapshot> slots,
            int playerHp,
            int playerMaxHp,
            int playerGold,
            int currentWaveNumber,
            WavePhase wavePhase,
            IReadOnlyList<TowerSnapshot> towers,
            IReadOnlyList<MonsterSnapshot> monsters) : base(tick)
        {
            SessionPhase = sessionPhase;
            Score = score;
            MaxGrade = maxGrade;
            TotalSlots = totalSlots;
            UsedSlots = usedSlots;
            ElapsedTime = elapsedTime;
            Slots = slots;
            PlayerHp = playerHp;
            PlayerMaxHp = playerMaxHp;
            PlayerGold = playerGold;
            CurrentWaveNumber = currentWaveNumber;
            WavePhase = wavePhase;
            Towers = towers;
            Monsters = monsters;
        }
    }
}


