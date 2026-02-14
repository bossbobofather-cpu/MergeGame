namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// VictoryConditionType 열거형입니다.
        /// </summary>
    public enum VictoryConditionType
    {
        /// <summary>
        /// 점수가 목표 값에 도달하면 승리합니다.
        /// </summary>
        ScoreReach,

        /// <summary>
        /// 제한 시간 또는 라운드를 생존하면 승리합니다.
        /// </summary>
        Survive,

        /// <summary>
        /// 유닛 등급이 목표 값에 도달하면 승리합니다.
        /// </summary>
        UnitGradeReach,

        /// <summary>
        /// 마지막까지 생존한 플레이어가 승리합니다.
        /// </summary>
        LastPlayerStanding
    }

    /// <summary>
        /// DefeatConditionType 열거형입니다.
        /// </summary>
    public enum DefeatConditionType
    {
        /// <summary>
        /// 플레이어 체력이 0 이하가 되면 패배합니다.
        /// </summary>
        PlayerDeath,

        /// <summary>
        /// 제한 시간을 초과하면 패배합니다.
        /// </summary>
        TimeOut
    }

    /// <summary>
        /// SpawnRuleType 열거형입니다.
        /// </summary>
    public enum SpawnRuleType
    {
        /// <summary>
        /// 덱에서 임의의 유닛을 스폰합니다.
        /// </summary>
        RandomFromDeck,

        /// <summary>
        /// 가중치 기반 확률로 유닛을 스폰합니다.
        /// </summary>
        WeightedRandom,

        /// <summary>
        /// 순차 규칙에 따라 유닛을 스폰합니다.
        /// </summary>
        Sequential
    }

    /// <summary>
        /// RuleModuleConfig 클래스입니다.
        /// </summary>
    public sealed class RuleModuleConfig
    {
        /// <summary>
        /// VictoryCondition 속성입니다.
        /// </summary>
        public VictoryConditionType VictoryCondition { get; set; } = VictoryConditionType.LastPlayerStanding;

        /// <summary>
        /// VictoryConditionValue 속성입니다.
        /// </summary>
        public int VictoryConditionValue { get; set; } = 10;

        /// <summary>
        /// DefeatCondition 속성입니다.
        /// </summary>
        public DefeatConditionType DefeatCondition { get; set; } = DefeatConditionType.PlayerDeath;

        /// <summary>
        /// SpawnRule 속성입니다.
        /// </summary>
        public SpawnRuleType SpawnRule { get; set; } = SpawnRuleType.RandomFromDeck;

        /// <summary>
        /// InitialUnitGrade 속성입니다.
        /// </summary>
        public int InitialUnitGrade { get; set; } = 1;

        /// <summary>
        /// MaxUnitGrade 속성입니다.
        /// </summary>
        public int MaxUnitGrade { get; set; } = 10;

        /// <summary>
        /// UnitSpawnCost 속성입니다.
        /// </summary>
        public int UnitSpawnCost { get; set; } = 10;

        /// <summary>
        /// ScorePerGrade 속성입니다.
        /// </summary>
        public int ScorePerGrade { get; set; } = 100;

        /// <summary>
        /// PlayerStartGold 속성입니다.
        /// </summary>
        public int PlayerStartGold { get; set; } = 100;

        /// <summary>
        /// GoldPerMonsterKill 속성입니다.
        /// </summary>
        public int GoldPerMonsterKill { get; set; } = 5;

        /// <summary>
        /// RequireSameTypeForMerge 속성입니다.
        /// </summary>
        public bool RequireSameTypeForMerge { get; set; } = true;
    }
}
