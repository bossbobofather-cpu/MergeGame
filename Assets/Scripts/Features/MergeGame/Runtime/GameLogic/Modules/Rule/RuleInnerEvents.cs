using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// MergeValidationRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class MergeValidationRequestInnerEvent : InnerEventBase
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
        /// MergeValidationRequestInnerEvent 생성자입니다.
        /// </summary>
        public MergeValidationRequestInnerEvent(
            long tick,
            int sourceGrade,
            int targetGrade,
            string sourceType,
            string targetType)
            : base(tick)
        {
            SourceGrade = sourceGrade;
            TargetGrade = targetGrade;
            SourceType = sourceType;
            TargetType = targetType;
        }
    }

    /// <summary>
        /// SpawnValidationRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class SpawnValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// CurrentGold 속성입니다.
        /// </summary>
        public int CurrentGold { get; }

        /// <summary>
        /// CanSpawn 속성입니다.
        /// </summary>
        public bool CanSpawn { get; set; }

        /// <summary>
        /// SpawnCost 속성입니다.
        /// </summary>
        public int SpawnCost { get; set; }

        /// <summary>
        /// FailReason 속성입니다.
        /// </summary>
        public string FailReason { get; set; }

        /// <summary>
        /// SpawnValidationRequestInnerEvent 생성자입니다.
        /// </summary>
        public SpawnValidationRequestInnerEvent(long tick, int currentGold)
            : base(tick)
        {
            CurrentGold = currentGold;
        }
    }

    /// <summary>
        /// GameEndCheckRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class GameEndCheckRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// CurrentHp 속성입니다.
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// CurrentWaveNumber 속성입니다.
        /// </summary>
        public int CurrentWaveNumber { get; }

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
        /// GameEndCheckRequestInnerEvent 생성자입니다.
        /// </summary>
        public GameEndCheckRequestInnerEvent(
            long tick,
            int currentHp,
            int currentWaveNumber,
            int currentScore,
            int maxUnitGrade)
            : base(tick)
        {
            CurrentHp = currentHp;
            CurrentWaveNumber = currentWaveNumber;
            CurrentScore = currentScore;
            MaxUnitGrade = maxUnitGrade;
        }
    }

    /// <summary>
        /// ScoreCalculationRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class ScoreCalculationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// MergedGrade 속성입니다.
        /// </summary>
        public int MergedGrade { get; }

        /// <summary>
        /// CalculatedScore 속성입니다.
        /// </summary>
        public int CalculatedScore { get; set; }

        /// <summary>
        /// ScoreCalculationRequestInnerEvent 생성자입니다.
        /// </summary>
        public ScoreCalculationRequestInnerEvent(long tick, int mergedGrade)
            : base(tick)
        {
            MergedGrade = mergedGrade;
        }
    }

    /// <summary>
        /// MonsterKillRewardRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class MonsterKillRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// MonsterId 속성입니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// RewardGold 속성입니다.
        /// </summary>
        public int RewardGold { get; set; }

        /// <summary>
        /// RewardScore 속성입니다.
        /// </summary>
        public int RewardScore { get; set; }

        /// <summary>
        /// MonsterKillRewardRequestInnerEvent 생성자입니다.
        /// </summary>
        public MonsterKillRewardRequestInnerEvent(long tick, string monsterId)
            : base(tick)
        {
            MonsterId = monsterId;
        }
    }

    /// <summary>
        /// WaveCompletionRewardRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class WaveCompletionRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// WaveNumber 속성입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// RewardGold 속성입니다.
        /// </summary>
        public int RewardGold { get; set; }

        /// <summary>
        /// WaveCompletionRewardRequestInnerEvent 생성자입니다.
        /// </summary>
        public WaveCompletionRewardRequestInnerEvent(long tick, int waveNumber)
            : base(tick)
        {
            WaveNumber = waveNumber;
        }
    }
}
