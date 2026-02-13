namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// ?밸━ 議곌굔 ??낆엯?덈떎.
    /// </summary>
    public enum VictoryConditionType
    {
        /// <summary>
        /// ?쇱젙 ?먯닔 ?ъ꽦
        /// </summary>
        ScoreReach,

        /// <summary>
        /// ?앹〈 (?쒓컙 ?쒗븳)
        /// </summary>
        Survive,

        /// <summary>
        /// ?뱀젙 ?깃툒 ?좊떅 蹂댁쑀
        /// </summary>
        UnitGradeReach,

        /// <summary>
        /// 留덉?留??앹〈??(硫?고뵆?덉씠??
        /// </summary>
        LastPlayerStanding
    }

    /// <summary>
    /// ?⑤같 議곌굔 ??낆엯?덈떎.
    /// </summary>
    public enum DefeatConditionType
    {
        /// <summary>
        /// ?뚮젅?댁뼱 HP ?뚯쭊
        /// </summary>
        PlayerDeath,

        /// <summary>
        /// ?쒓컙 珥덇낵
        /// </summary>
        TimeOut
    }

    /// <summary>
    /// ?ㅽ룿 洹쒖튃 ??낆엯?덈떎.
    /// </summary>
    public enum SpawnRuleType
    {
        /// <summary>
        /// ?깆뿉??臾댁옉???좏깮
        /// </summary>
        RandomFromDeck,

        /// <summary>
        /// 媛以묒튂 湲곕컲 臾댁옉???좏깮
        /// </summary>
        WeightedRandom,

        /// <summary>
        /// ?쒖감 ?좏깮
        /// </summary>
        Sequential
    }

    /// <summary>
    /// RuleModule ?ㅼ젙?낅땲??
    /// </summary>
    public sealed class RuleModuleConfig
    {
        /// <summary>
        /// ?밸━ 議곌굔?낅땲??
        /// </summary>
        public VictoryConditionType VictoryCondition { get; set; } = VictoryConditionType.LastPlayerStanding;

        /// <summary>
        /// ?밸━ 議곌굔 媛믪엯?덈떎. (?⑥씠釉??? ?먯닔, ?쒓컙 ??
        /// </summary>
        public int VictoryConditionValue { get; set; } = 10;

        /// <summary>
        /// ?⑤같 議곌굔?낅땲??
        /// </summary>
        public DefeatConditionType DefeatCondition { get; set; } = DefeatConditionType.PlayerDeath;

        /// <summary>
        /// ?ㅽ룿 洹쒖튃?낅땲??
        /// </summary>
        public SpawnRuleType SpawnRule { get; set; } = SpawnRuleType.RandomFromDeck;

        /// <summary>
        /// 珥덇린 ?좊떅 ?깃툒?낅땲??
        /// </summary>
        public int InitialUnitGrade { get; set; } = 1;

        /// <summary>
        /// 理쒕? ?좊떅 ?깃툒?낅땲??
        /// </summary>
        public int MaxUnitGrade { get; set; } = 10;

        /// <summary>
        /// ?좊떅 ?ㅽ룿 鍮꾩슜?낅땲??
        /// </summary>
        public int UnitSpawnCost { get; set; } = 10;

        /// <summary>
        /// ?깃툒???먯닔?낅땲??
        /// </summary>
        public int ScorePerGrade { get; set; } = 100;

        /// <summary>
        /// ?뚮젅?댁뼱 ?쒖옉 怨⑤뱶?낅땲??
        /// </summary>
        public int PlayerStartGold { get; set; } = 100;

        /// <summary>
        /// 紐ъ뒪?곕떦 蹂댁긽 怨⑤뱶?낅땲??
        /// </summary>
        public int GoldPerMonsterKill { get; set; } = 5;

        /// <summary>
        /// ?숈씪 ??낅쭔 癒몄? 媛???щ??낅땲??
        /// </summary>
        public bool RequireSameTypeForMerge { get; set; } = true;
    }
}
