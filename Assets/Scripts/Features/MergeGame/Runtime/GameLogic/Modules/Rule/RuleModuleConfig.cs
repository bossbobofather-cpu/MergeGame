namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 승리 조건 타입입니다.
    /// </summary>
    public enum VictoryConditionType
    {
        /// <summary>
        /// 일정 점수 달성
        /// </summary>
        ScoreReach,

        /// <summary>
        /// 생존 (시간 제한)
        /// </summary>
        Survive,

        /// <summary>
        /// 특정 등급 유닛 보유
        /// </summary>
        UnitGradeReach,

        /// <summary>
        /// 마지막 생존자 (멀티플레이어)
        /// </summary>
        LastPlayerStanding
    }

    /// <summary>
    /// 패배 조건 타입입니다.
    /// </summary>
    public enum DefeatConditionType
    {
        /// <summary>
        /// 플레이어 HP 소진
        /// </summary>
        PlayerDeath,

        /// <summary>
        /// 시간 초과
        /// </summary>
        TimeOut
    }

    /// <summary>
    /// 스폰 규칙 타입입니다.
    /// </summary>
    public enum SpawnRuleType
    {
        /// <summary>
        /// 덱에서 무작위 선택
        /// </summary>
        RandomFromDeck,

        /// <summary>
        /// 가중치 기반 무작위 선택
        /// </summary>
        WeightedRandom,

        /// <summary>
        /// 순차 선택
        /// </summary>
        Sequential
    }

    /// <summary>
    /// RuleModule 설정입니다.
    /// </summary>
    public sealed class RuleModuleConfig
    {
        /// <summary>
        /// 승리 조건입니다.
        /// </summary>
        public VictoryConditionType VictoryCondition { get; set; } = VictoryConditionType.LastPlayerStanding;

        /// <summary>
        /// 승리 조건 값입니다. (웨이브 수, 점수, 시간 등)
        /// </summary>
        public int VictoryConditionValue { get; set; } = 10;

        /// <summary>
        /// 패배 조건입니다.
        /// </summary>
        public DefeatConditionType DefeatCondition { get; set; } = DefeatConditionType.PlayerDeath;

        /// <summary>
        /// 스폰 규칙입니다.
        /// </summary>
        public SpawnRuleType SpawnRule { get; set; } = SpawnRuleType.RandomFromDeck;

        /// <summary>
        /// 초기 유닛 등급입니다.
        /// </summary>
        public int InitialUnitGrade { get; set; } = 1;

        /// <summary>
        /// 최대 유닛 등급입니다.
        /// </summary>
        public int MaxUnitGrade { get; set; } = 10;

        /// <summary>
        /// 유닛 스폰 비용입니다.
        /// </summary>
        public int UnitSpawnCost { get; set; } = 10;

        /// <summary>
        /// 등급당 점수입니다.
        /// </summary>
        public int ScorePerGrade { get; set; } = 100;

        /// <summary>
        /// 플레이어 시작 골드입니다.
        /// </summary>
        public int PlayerStartGold { get; set; } = 100;

        /// <summary>
        /// 플레이어 최대 HP입니다.
        /// </summary>
        public int PlayerMaxHp { get; set; } = 100;

        /// <summary>
        /// 몬스터당 보상 골드입니다.
        /// </summary>
        public int GoldPerMonsterKill { get; set; } = 5;

        /// <summary>
        /// 동일 타입만 머지 가능 여부입니다.
        /// </summary>
        public bool RequireSameTypeForMerge { get; set; } = true;
    }
}
