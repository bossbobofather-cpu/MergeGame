using MyProject.MergeGame;
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
        public int SlotIndex { get; }
        public Point3D Position { get; set; }
        public bool Found { get; set; }

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
        public int SlotIndex { get; set; } = -1;
        public bool Found => SlotIndex >= 0;

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
        public int Cost { get; set; }

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
        public int SourceGrade { get; }
        public int TargetGrade { get; }
        public string SourceType { get; }
        public string TargetType { get; }

        public bool CanMerge { get; set; }
        public string FailReason { get; set; }

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
        public int PathIndex { get; }
        public MapPath Path { get; set; }
        public bool Found => Path != null;

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
        public int PathIndex { get; }
        public float Progress { get; }
        public Point3D Position { get; set; }
        public bool Found { get; set; }

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
        public int CurrentHp { get; }
        public int CurrentWave { get; }
        public int CurrentScore { get; }
        public int MaxUnitGrade { get; }

        public bool IsGameEnd { get; set; }
        public bool IsVictory { get; set; }

        public CheckGameEndRequest(long tick, int currentHp, int currentWave, int currentScore, int maxUnitGrade)
            : base(tick)
        {
            CurrentHp = currentHp;
            CurrentWave = currentWave;
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
        public enum RewardType { MonsterKill, WaveComplete, Merge }

        public RewardType Type { get; }
        public int Value { get; } // MonsterKill: monsterId hash, WaveComplete: waveNumber, Merge: resultGrade

        public int GoldReward { get; set; }
        public int ScoreReward { get; set; }

        public CalculateRewardRequest(long tick, RewardType type, int value) : base(tick)
        {
            Type = type;
            Value = value;
        }
    }

    #endregion

    #region 웨이브 상태 이벤트

    /// <summary>
    /// 웨이브 상태 조회 요청입니다.
    /// WaveModule이 응답합니다.
    /// </summary>
    public sealed class GetWaveStatusRequest : InnerEventBase
    {
        public int CurrentWaveNumber { get; set; }
        public WavePhase Phase { get; set; }
        public int TotalMonsters { get; set; }
        public int RemainingMonsters { get; set; }

        public GetWaveStatusRequest(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 웨이브 시작 요청입니다.
    /// WaveModule이 처리합니다.
    /// </summary>
    public sealed class RequestWaveStart : InnerEventBase
    {
        public bool Success { get; set; }
        public string FailReason { get; set; }

        public RequestWaveStart(long tick) : base(tick)
        {
        }
    }

    #endregion
}

