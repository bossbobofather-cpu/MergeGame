using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class MergeValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SourceGrade { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int TargetGrade { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string SourceType { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool CanMerge { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string FailReason { get; set; }

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
        /// 요약 설명입니다.
        /// </summary>
    public sealed class SpawnValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CurrentGold { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool CanSpawn { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SpawnCost { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string FailReason { get; set; }

        public SpawnValidationRequestInnerEvent(long tick, int currentGold)
            : base(tick)
        {
            CurrentGold = currentGold;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class GameEndCheckRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CurrentWaveNumber { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int MaxUnitGrade { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool IsGameEnd { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool IsVictory { get; set; }

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
        /// 요약 설명입니다.
        /// </summary>
    public sealed class ScoreCalculationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int MergedGrade { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CalculatedScore { get; set; }

        public ScoreCalculationRequestInnerEvent(long tick, int mergedGrade)
            : base(tick)
        {
            MergedGrade = mergedGrade;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class MonsterKillRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int RewardGold { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int RewardScore { get; set; }

        public MonsterKillRewardRequestInnerEvent(long tick, string monsterId)
            : base(tick)
        {
            MonsterId = monsterId;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class WaveCompletionRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int RewardGold { get; set; }

        public WaveCompletionRewardRequestInnerEvent(long tick, int waveNumber)
            : base(tick)
        {
            WaveNumber = waveNumber;
        }
    }
}
