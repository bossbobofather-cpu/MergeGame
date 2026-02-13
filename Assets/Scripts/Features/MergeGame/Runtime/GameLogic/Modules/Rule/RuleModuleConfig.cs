namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public enum VictoryConditionType
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        ScoreReach,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        Survive,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        UnitGradeReach,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        LastPlayerStanding
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public enum DefeatConditionType
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        PlayerDeath,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        TimeOut
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public enum SpawnRuleType
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        RandomFromDeck,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        WeightedRandom,

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        Sequential
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class RuleModuleConfig
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public VictoryConditionType VictoryCondition { get; set; } = VictoryConditionType.LastPlayerStanding;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int VictoryConditionValue { get; set; } = 10;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public DefeatConditionType DefeatCondition { get; set; } = DefeatConditionType.PlayerDeath;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public SpawnRuleType SpawnRule { get; set; } = SpawnRuleType.RandomFromDeck;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int InitialUnitGrade { get; set; } = 1;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int MaxUnitGrade { get; set; } = 10;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int UnitSpawnCost { get; set; } = 10;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int ScorePerGrade { get; set; } = 100;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int PlayerStartGold { get; set; } = 100;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int GoldPerMonsterKill { get; set; } = 5;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool RequireSameTypeForMerge { get; set; } = true;
    }
}
