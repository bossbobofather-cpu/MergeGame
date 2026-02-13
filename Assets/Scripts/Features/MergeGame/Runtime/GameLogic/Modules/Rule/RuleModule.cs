using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 洹쒖튃 紐⑤뱢?낅땲??
    /// ?ㅽ룿 洹쒖튃, 癒몄? 洹쒖튃, ?밸━/?⑤같 議곌굔??愿由ы빀?덈떎.
    /// </summary>
    public sealed class RuleModule : HostModuleBase<RuleModuleConfig>
    {
        public const string MODULE_ID = "rule";

        /// <inheritdoc />
        public override string ModuleId => MODULE_ID;

        /// <inheritdoc />
        public override bool IsRequired => true;

        /// <inheritdoc />
        public override int Priority => 90; // MapModule ?ㅼ쓬?쇰줈 珥덇린??

        /// <summary>
        /// ?밸━ 議곌굔?낅땲??
        /// </summary>
        public VictoryConditionType VictoryCondition => Config.VictoryCondition;

        /// <summary>
        /// ?⑤같 議곌굔?낅땲??
        /// </summary>
        public DefeatConditionType DefeatCondition => Config.DefeatCondition;

        /// <summary>
        /// ?ㅽ룿 洹쒖튃?낅땲??
        /// </summary>
        public SpawnRuleType SpawnRule => Config.SpawnRule;

        /// <summary>
        /// ?좊떅 ?ㅽ룿 鍮꾩슜?낅땲??
        /// </summary>
        public int UnitSpawnCost => Config.UnitSpawnCost;

        /// <summary>
        /// 理쒕? ?좊떅 ?깃툒?낅땲??
        /// </summary>
        public int MaxUnitGrade => Config.MaxUnitGrade;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // ?대? ?대깽??援щ룆 (紐⑤뱢 ?꾩슜)
            SubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            SubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            SubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            SubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            SubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);

            // 怨듭쑀 ?대? ?대깽??援щ룆 (?ㅻⅨ 紐⑤뱢?먯꽌 ?몄텧)
            SubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            SubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            SubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            SubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            // ?대? ?대깽??援щ룆 ?댁젣 (紐⑤뱢 ?꾩슜)
            UnsubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            UnsubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            UnsubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            UnsubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            UnsubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);

            // 怨듭쑀 ?대? ?대깽??援щ룆 ?댁젣
            UnsubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            UnsubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            UnsubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            UnsubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        #region 寃利?硫붿꽌??

        /// <summary>
        /// 癒몄? 媛???щ?瑜?寃利앺빀?덈떎.
        /// </summary>
        public bool CanMerge(int sourceGrade, int targetGrade, string sourceType, string targetType, out string failReason)
        {
            // 핵심 로직을 처리합니다.
            failReason = null;

            // ?깃툒 泥댄겕
            if (sourceGrade != targetGrade)
            {
                failReason = "?깃툒???ㅻ쫭?덈떎.";
                return false;
            }

            // 理쒕? ?깃툒 泥댄겕
            if (sourceGrade >= Config.MaxUnitGrade)
            {
                failReason = "理쒕? ?깃툒???꾨떖?덉뒿?덈떎.";
                return false;
            }

            // ???泥댄겕 (?ㅼ젙???곕씪)
            if (Config.RequireSameTypeForMerge && sourceType != targetType)
            {
                failReason = "??낆씠 ?ㅻ쫭?덈떎.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// ?ㅽ룿 媛???щ?瑜?寃利앺빀?덈떎.
        /// </summary>
        public bool CanSpawn(int currentGold, out string failReason)
        {
            // 핵심 로직을 처리합니다.
            failReason = null;

            if (currentGold < Config.UnitSpawnCost)
            {
                failReason = "怨⑤뱶媛 遺議깊빀?덈떎.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 寃뚯엫 醫낅즺 ?щ?瑜?寃利앺빀?덈떎.
        /// </summary>
        public (bool isEnd, bool isVictory) CheckGameEnd(
            int currentHp,
            int currentScore,
            int maxUnitGrade)
        {
            // ?⑤같 議곌굔 泥댄겕
            switch (Config.DefeatCondition)
            {
                case DefeatConditionType.PlayerDeath:
                    if (currentHp <= 0)
                    {
                        return (true, false);
                    }
                    break;
            }

            // ?밸━ 議곌굔 泥댄겕
            // LastPlayerStanding? Host??CheckGlobalGameOver()?먯꽌 泥섎━
            switch (Config.VictoryCondition)
            {
                case VictoryConditionType.ScoreReach:
                    if (currentScore >= Config.VictoryConditionValue)
                    {
                        return (true, true);
                    }
                    break;

                case VictoryConditionType.UnitGradeReach:
                    if (maxUnitGrade >= Config.VictoryConditionValue)
                    {
                        return (true, true);
                    }
                    break;
            }

            return (false, false);
        }

        #endregion

        #region 怨꾩궛 硫붿꽌??

        /// <summary>
        /// 癒몄? ?먯닔瑜?怨꾩궛?⑸땲??
        /// </summary>
        public int CalculateMergeScore(int resultGrade)
        {
            // 핵심 로직을 처리합니다.
            return resultGrade * Config.ScorePerGrade;
        }

        /// <summary>
        /// 紐ъ뒪??泥섏튂 蹂댁긽??怨꾩궛?⑸땲??
        /// </summary>
        public (int gold, int score) CalculateMonsterKillReward(string monsterId)
        {
            // 湲곕낯 蹂댁긽
            return (Config.GoldPerMonsterKill, 10);
        }

        /// <summary>
        /// ?⑥씠釉??꾨즺 蹂댁긽??怨꾩궛?⑸땲??
        /// </summary>
        #endregion

        #region ?대? ?대깽???몃뱾??
        /// <summary>
        /// OnMergeValidationRequest 함수를 처리합니다.
        /// </summary>

        private void OnMergeValidationRequest(MergeValidationRequestInnerEvent evt)
        {
            // 핵심 로직을 처리합니다.
            evt.CanMerge = CanMerge(
                evt.SourceGrade,
                evt.TargetGrade,
                evt.SourceType,
                evt.TargetType,
                out var failReason);
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnSpawnValidationRequest 함수를 처리합니다.
        /// </summary>

        private void OnSpawnValidationRequest(SpawnValidationRequestInnerEvent evt)
        {
            // 핵심 로직을 처리합니다.
            evt.CanSpawn = CanSpawn(evt.CurrentGold, out var failReason);
            evt.SpawnCost = Config.UnitSpawnCost;
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnGameEndCheckRequest 함수를 처리합니다.
        /// </summary>

        private void OnGameEndCheckRequest(GameEndCheckRequestInnerEvent evt)
        {
            // 핵심 로직을 처리합니다.
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }
        /// <summary>
        /// OnScoreCalculationRequest 함수를 처리합니다.
        /// </summary>

        private void OnScoreCalculationRequest(ScoreCalculationRequestInnerEvent evt)
        {
            // 핵심 로직을 처리합니다.
            evt.CalculatedScore = CalculateMergeScore(evt.MergedGrade);
        }
        /// <summary>
        /// OnMonsterKillRewardRequest 함수를 처리합니다.
        /// </summary>

        private void OnMonsterKillRewardRequest(MonsterKillRewardRequestInnerEvent evt)
        {
            // 핵심 로직을 처리합니다.
            var (gold, score) = CalculateMonsterKillReward(evt.MonsterId);
            evt.RewardGold = gold;
            evt.RewardScore = score;
        }

        #endregion

        #region 怨듭쑀 ?대? ?대깽???몃뱾??
        /// <summary>
        /// OnGetSpawnCostRequest 함수를 처리합니다.
        /// </summary>

        private void OnGetSpawnCostRequest(GetSpawnCostRequest evt)
        {
            // 핵심 로직을 처리합니다.
            evt.Cost = Config.UnitSpawnCost;
        }
        /// <summary>
        /// OnCanMergeRequest 함수를 처리합니다.
        /// </summary>

        private void OnCanMergeRequest(CanMergeRequest evt)
        {
            // 핵심 로직을 처리합니다.
            evt.CanMerge = CanMerge(
                evt.SourceGrade,
                evt.TargetGrade,
                evt.SourceType,
                evt.TargetType,
                out var failReason);
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnCheckGameEndRequest 함수를 처리합니다.
        /// </summary>

        private void OnCheckGameEndRequest(CheckGameEndRequest evt)
        {
            // 핵심 로직을 처리합니다.
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }
        /// <summary>
        /// OnCalculateRewardRequest 함수를 처리합니다.
        /// </summary>

        private void OnCalculateRewardRequest(CalculateRewardRequest evt)
        {
            // 핵심 로직을 처리합니다.
            switch (evt.Type)
            {
                case CalculateRewardRequest.RewardType.MonsterKill:
                    var (gold, score) = CalculateMonsterKillReward(evt.Value.ToString());
                    evt.GoldReward = gold;
                    evt.ScoreReward = score;
                    break;

                case CalculateRewardRequest.RewardType.Merge:
                    evt.ScoreReward = CalculateMergeScore(evt.Value);
                    break;
            }
        }

        #endregion
    }
}
