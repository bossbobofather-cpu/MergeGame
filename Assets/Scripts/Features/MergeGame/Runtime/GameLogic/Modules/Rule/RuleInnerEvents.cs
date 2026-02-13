using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 癒몄? 媛???щ? 寃利??붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class MergeValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?뚯뒪 ?좊떅 ?깃툒?낅땲??
        /// </summary>
        public int SourceGrade { get; }

        /// <summary>
        /// ?寃??좊떅 ?깃툒?낅땲??
        /// </summary>
        public int TargetGrade { get; }

        /// <summary>
        /// ?뚯뒪 ?좊떅 ??낆엯?덈떎.
        /// </summary>
        public string SourceType { get; }

        /// <summary>
        /// ?寃??좊떅 ??낆엯?덈떎.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// 癒몄? 媛???щ? 寃곌낵?낅땲??
        /// </summary>
        public bool CanMerge { get; set; }

        /// <summary>
        /// ?ㅽ뙣 ?ъ쑀?낅땲??
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
    /// ?ㅽ룿 寃利??붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class SpawnValidationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?꾩옱 怨⑤뱶?낅땲??
        /// </summary>
        public int CurrentGold { get; }

        /// <summary>
        /// ?ㅽ룿 媛???щ? 寃곌낵?낅땲??
        /// </summary>
        public bool CanSpawn { get; set; }

        /// <summary>
        /// ?ㅽ룿 鍮꾩슜?낅땲??
        /// </summary>
        public int SpawnCost { get; set; }

        /// <summary>
        /// ?ㅽ뙣 ?ъ쑀?낅땲??
        /// </summary>
        public string FailReason { get; set; }

        public SpawnValidationRequestInnerEvent(long tick, int currentGold)
            : base(tick)
        {
            CurrentGold = currentGold;
        }
    }

    /// <summary>
    /// ?밸━/?⑤같 議곌굔 寃利??붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class GameEndCheckRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?꾩옱 ?뚮젅?댁뼱 HP?낅땲??
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// ?꾩옱 ?⑥씠釉?踰덊샇?낅땲??
        /// </summary>
        public int CurrentWaveNumber { get; }

        /// <summary>
        /// ?꾩옱 ?먯닔?낅땲??
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 理쒕? ?좊떅 ?깃툒?낅땲??
        /// </summary>
        public int MaxUnitGrade { get; }

        /// <summary>
        /// 寃뚯엫 醫낅즺 ?щ??낅땲??
        /// </summary>
        public bool IsGameEnd { get; set; }

        /// <summary>
        /// ?밸━ ?щ??낅땲??
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
    /// ?먯닔 怨꾩궛 ?붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class ScoreCalculationRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 癒몄????좊떅 ?깃툒?낅땲??
        /// </summary>
        public int MergedGrade { get; }

        /// <summary>
        /// 怨꾩궛???먯닔?낅땲??
        /// </summary>
        public int CalculatedScore { get; set; }

        public ScoreCalculationRequestInnerEvent(long tick, int mergedGrade)
            : base(tick)
        {
            MergedGrade = mergedGrade;
        }
    }

    /// <summary>
    /// 紐ъ뒪??泥섏튂 蹂댁긽 ?붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class MonsterKillRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 紐ъ뒪??ID?낅땲??
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 蹂댁긽 怨⑤뱶?낅땲??
        /// </summary>
        public int RewardGold { get; set; }

        /// <summary>
        /// 蹂댁긽 ?먯닔?낅땲??
        /// </summary>
        public int RewardScore { get; set; }

        public MonsterKillRewardRequestInnerEvent(long tick, string monsterId)
            : base(tick)
        {
            MonsterId = monsterId;
        }
    }

    /// <summary>
    /// ?⑥씠釉??꾨즺 蹂댁긽 ?붿껌 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class WaveCompletionRewardRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?꾨즺???⑥씠釉?踰덊샇?낅땲??
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 蹂댁긽 怨⑤뱶?낅땲??
        /// </summary>
        public int RewardGold { get; set; }

        public WaveCompletionRewardRequestInnerEvent(long tick, int waveNumber)
            : base(tick)
        {
            WaveNumber = waveNumber;
        }
    }
}
