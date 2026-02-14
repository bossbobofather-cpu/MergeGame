using Noname.GameAbilitySystem;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 모듈간 공유되는 내부 이벤트들입니다.
    /// 이 이벤트들은 여러 모듈에서 구독/발행할 수 있습니다.
    /// </summary>

    #region 슬롯 관련 이벤트

    /// <summary>
    /// 슬롯 위치 조회 요청입니다.
    /// MapModule이 응답합니다.
    /// </summary>
    public sealed class GetSlotPositionRequest : InnerEventBase
    {
        /// <summary>
        /// SlotIndex 속성입니다.
        /// </summary>
        public int SlotIndex { get; }
        /// <summary>
        /// Position 속성입니다.
        /// </summary>
        public Point3D Position { get; set; }
        /// <summary>
        /// Found 속성입니다.
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetSlotPositionRequest(long tick, int slotIndex) : base(tick)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 빈 슬롯 조회 요청입니다.
    /// MapModule이 응답합니다.
    /// </summary>
    public sealed class GetEmptySlotRequest : InnerEventBase
    {
        /// <summary>
        /// SlotIndex 속성입니다.
        /// </summary>
        public int SlotIndex { get; set; } = -1;
        /// <summary>
        /// Found 속성입니다.
        /// </summary>
        public bool Found => SlotIndex >= 0;
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetEmptySlotRequest(long tick) : base(tick)
        {
        }
    }

    #endregion

    #region 규칙 검증 이벤트

    /// <summary>
    /// 스폰 비용 조회 요청입니다.
    /// RuleModule이 응답합니다.
    /// </summary>
    public sealed class GetSpawnCostRequest : InnerEventBase
    {
        /// <summary>
        /// Cost 속성입니다.
        /// </summary>
        public int Cost { get; set; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetSpawnCostRequest(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 머지 가능 여부 조회 요청입니다.
    /// RuleModule이 응답합니다.
    /// </summary>
    public sealed class CanMergeRequest : InnerEventBase
    {
        /// <summary>
        /// SourceGrade 속성입니다.
        /// </summary>
        public int SourceGrade { get; }
        /// <summary>
        /// TargetGrade 속성입니다.
        /// </summary>
        public int TargetGrade { get; }
        /// <summary>
        /// SourceType 속성입니다.
        /// </summary>
        public string SourceType { get; }
        /// <summary>
        /// TargetType 속성입니다.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// CanMerge 속성입니다.
        /// </summary>
        public bool CanMerge { get; set; }
        /// <summary>
        /// FailReason 속성입니다.
        /// </summary>
        public string FailReason { get; set; }

        /// <summary>
        /// CanMergeRequest 생성자입니다.
        /// </summary>
        public CanMergeRequest(long tick, int sourceGrade, int targetGrade, string sourceType, string targetType)
            : base(tick)
        {
            SourceGrade = sourceGrade;
            TargetGrade = targetGrade;
            SourceType = sourceType;
            TargetType = targetType;
        }
    }

    #endregion

    #region 경로 관련 이벤트

    /// <summary>
    /// 경로 조회 요청입니다.
    /// MapModule이 응답합니다.
    /// </summary>
    public sealed class GetPathRequest : InnerEventBase
    {
        /// <summary>
        /// PathIndex 속성입니다.
        /// </summary>
        public int PathIndex { get; }
        /// <summary>
        /// Path 속성입니다.
        /// </summary>
        public MapPath Path { get; set; }
        /// <summary>
        /// Found 속성입니다.
        /// </summary>
        public bool Found => Path != null;
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetPathRequest(long tick, int pathIndex) : base(tick)
        {
            PathIndex = pathIndex;
        }
    }

    /// <summary>
    /// 경로상 위치 조회 요청입니다.
    /// MapModule이 응답합니다.
    /// </summary>
    public sealed class GetPathPositionRequest : InnerEventBase
    {
        /// <summary>
        /// PathIndex 속성입니다.
        /// </summary>
        public int PathIndex { get; }
        /// <summary>
        /// Progress 속성입니다.
        /// </summary>
        public float Progress { get; }
        /// <summary>
        /// Position 속성입니다.
        /// </summary>
        public Point3D Position { get; set; }
        /// <summary>
        /// Found 속성입니다.
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetPathPositionRequest(long tick, int pathIndex, float progress) : base(tick)
        {
            PathIndex = pathIndex;
            Progress = progress;
        }
    }

    #endregion

    #region 게임 상태 이벤트

    /// <summary>
    /// 게임 종료 조건 체크 요청입니다.
    /// RuleModule이 응답합니다.
    /// </summary>
    public sealed class CheckGameEndRequest : InnerEventBase
    {
        /// <summary>
        /// CurrentHp 속성입니다.
        /// </summary>
        public int CurrentHp { get; }
        /// <summary>
        /// CurrentScore 속성입니다.
        /// </summary>
        public int CurrentScore { get; }
        /// <summary>
        /// MaxUnitGrade 속성입니다.
        /// </summary>
        public int MaxUnitGrade { get; }

        /// <summary>
        /// IsGameEnd 속성입니다.
        /// </summary>
        public bool IsGameEnd { get; set; }
        /// <summary>
        /// IsVictory 속성입니다.
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// CheckGameEndRequest 생성자입니다.
        /// </summary>
        public CheckGameEndRequest(long tick, int currentHp, int currentScore, int maxUnitGrade)
            : base(tick)
        {
            CurrentHp = currentHp;
            CurrentScore = currentScore;
            MaxUnitGrade = maxUnitGrade;
        }
    }

    /// <summary>
    /// 보상 계산 요청입니다.
    /// RuleModule이 응답합니다.
    /// </summary>
    public sealed class CalculateRewardRequest : InnerEventBase
    {
        /// <summary>
        /// RewardType 열거형입니다.
        /// </summary>
        public enum RewardType { MonsterKill, Merge }

        /// <summary>
        /// Type 속성입니다.
        /// </summary>
        public RewardType Type { get; }
        /// <summary>
        /// Value 속성입니다.
        /// </summary>
        public int Value { get; } // MonsterKill: monsterId hash, Merge: resultGrade

        /// <summary>
        /// GoldReward 속성입니다.
        /// </summary>
        public int GoldReward { get; set; }
        /// <summary>
        /// ScoreReward 속성입니다.
        /// </summary>
        public int ScoreReward { get; set; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public CalculateRewardRequest(long tick, RewardType type, int value) : base(tick)
        {
            Type = type;
            Value = value;
        }
    }

    #endregion

    #region 난이도 상태 이벤트

    /// <summary>
    /// 난이도 상태 조회 요청입니다.
    /// DifficultyModule이 응답합니다.
    /// </summary>
    public sealed class GetDifficultyStatusRequest : InnerEventBase
    {
        /// <summary>
        /// CurrentStep 속성입니다.
        /// </summary>
        public int CurrentStep { get; set; }
        /// <summary>
        /// SpawnCount 속성입니다.
        /// </summary>
        public int SpawnCount { get; set; }
        /// <summary>
        /// HealthMultiplier 속성입니다.
        /// </summary>
        public float HealthMultiplier { get; set; }
        /// <summary>
        /// SpawnInterval 속성입니다.
        /// </summary>
        public float SpawnInterval { get; set; }
        /// <summary>
        /// base 메서드입니다.
        /// </summary>

        public GetDifficultyStatusRequest(long tick) : base(tick)
        {
        }
    }

    #endregion
}
