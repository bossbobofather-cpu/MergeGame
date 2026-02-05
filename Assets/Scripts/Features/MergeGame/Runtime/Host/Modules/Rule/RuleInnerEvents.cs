using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 머지 가능 여부 검증 요청 이벤트입니다.
    /// </summary>
    public sealed class MergeValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 소스 유닛 등급입니다.
        /// </summary>
        public int SourceGrade { get; }

        /// <summary>
        /// 타겟 유닛 등급입니다.
        /// </summary>
        public int TargetGrade { get; }

        /// <summary>
        /// 소스 유닛 타입입니다.
        /// </summary>
        public string SourceType { get; }

        /// <summary>
        /// 타겟 유닛 타입입니다.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// 머지 가능 여부 결과입니다.
        /// </summary>
        public bool CanMerge { get; set; }

        /// <summary>
        /// 실패 사유입니다.
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
    /// 스폰 검증 요청 이벤트입니다.
    /// </summary>
    public sealed class SpawnValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 현재 골드입니다.
        /// </summary>
        public int CurrentGold { get; }

        /// <summary>
        /// 스폰 가능 여부 결과입니다.
        /// </summary>
        public bool CanSpawn { get; set; }

        /// <summary>
        /// 스폰 비용입니다.
        /// </summary>
        public int SpawnCost { get; set; }

        /// <summary>
        /// 실패 사유입니다.
        /// </summary>
        public string FailReason { get; set; }

        public SpawnValidationRequestInnerEvent(long tick, int currentGold)
            : base(tick)
        {
            CurrentGold = currentGold;
        }
    }

    /// <summary>
    /// 승리/패배 조건 검증 요청 이벤트입니다.
    /// </summary>
    public sealed class GameEndCheckRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 현재 플레이어 HP입니다.
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
        public int CurrentWaveNumber { get; }

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 최대 유닛 등급입니다.
        /// </summary>
        public int MaxUnitGrade { get; }

        /// <summary>
        /// 게임 종료 여부입니다.
        /// </summary>
        public bool IsGameEnd { get; set; }

        /// <summary>
        /// 승리 여부입니다.
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
    /// 점수 계산 요청 이벤트입니다.
    /// </summary>
    public sealed class ScoreCalculationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 머지된 유닛 등급입니다.
        /// </summary>
        public int MergedGrade { get; }

        /// <summary>
        /// 계산된 점수입니다.
        /// </summary>
        public int CalculatedScore { get; set; }

        public ScoreCalculationRequestInnerEvent(long tick, int mergedGrade)
            : base(tick)
        {
            MergedGrade = mergedGrade;
        }
    }

    /// <summary>
    /// 몬스터 처치 보상 요청 이벤트입니다.
    /// </summary>
    public sealed class MonsterKillRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 몬스터 ID입니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 보상 골드입니다.
        /// </summary>
        public int RewardGold { get; set; }

        /// <summary>
        /// 보상 점수입니다.
        /// </summary>
        public int RewardScore { get; set; }

        public MonsterKillRewardRequestInnerEvent(long tick, string monsterId)
            : base(tick)
        {
            MonsterId = monsterId;
        }
    }

    /// <summary>
    /// 웨이브 완료 보상 요청 이벤트입니다.
    /// </summary>
    public sealed class WaveCompletionRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 완료된 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 보상 골드입니다.
        /// </summary>
        public int RewardGold { get; set; }

        public WaveCompletionRewardRequestInnerEvent(long tick, int waveNumber)
            : base(tick)
        {
            WaveNumber = waveNumber;
        }
    }
}
