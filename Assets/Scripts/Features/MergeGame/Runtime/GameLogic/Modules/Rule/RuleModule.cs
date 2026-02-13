using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
        /// </summary>
        public VictoryConditionType VictoryCondition => Config.VictoryCondition;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public DefeatConditionType DefeatCondition => Config.DefeatCondition;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public SpawnRuleType SpawnRule => Config.SpawnRule;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int UnitSpawnCost => Config.UnitSpawnCost;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int MaxUnitGrade => Config.MaxUnitGrade;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            SubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            SubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            SubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            SubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            SubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);
            SubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            SubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            SubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            SubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            UnsubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            UnsubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            UnsubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            UnsubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            UnsubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);
            UnsubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            UnsubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            UnsubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            UnsubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        #region 검증 메서드

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool CanMerge(int sourceGrade, int targetGrade, string sourceType, string targetType, out string failReason)
        {
            failReason = null;
            if (sourceGrade != targetGrade)
            {
                failReason = "?깃툒???ㅻ쫭?덈떎.";
                return false;
            }
            if (sourceGrade >= Config.MaxUnitGrade)
            {
                failReason = "理쒕? ?깃툒???꾨떖?덉뒿?덈떎.";
                return false;
            }
            if (Config.RequireSameTypeForMerge && sourceType != targetType)
            {
                failReason = "??낆씠 ?ㅻ쫭?덈떎.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool CanSpawn(int currentGold, out string failReason)
        {
            failReason = null;

            if (currentGold < Config.UnitSpawnCost)
            {
                failReason = "골드가 부족합니다.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public (bool isEnd, bool isVictory) CheckGameEnd(
            int currentHp,
            int currentScore,
            int maxUnitGrade)
        {
            switch (Config.DefeatCondition)
            {
                case DefeatConditionType.PlayerDeath:
                    if (currentHp <= 0)
                    {
                        return (true, false);
                    }
                    break;
            }
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

        #region 계산 메서??

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int CalculateMergeScore(int resultGrade)
        {
            return resultGrade * Config.ScorePerGrade;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public (int gold, int score) CalculateMonsterKillReward(string monsterId)
        {
            // 기본 보상
            return (Config.GoldPerMonsterKill, 10);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        #endregion

        #region ?대? ?대깽???몃뱾??
        /// <summary>
        /// OnMergeValidationRequest 메서드입니다.
        /// </summary>

        private void OnMergeValidationRequest(MergeValidationRequestInnerEvent evt)
        {
            evt.CanMerge = CanMerge(
                evt.SourceGrade,
                evt.TargetGrade,
                evt.SourceType,
                evt.TargetType,
                out var failReason);
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnSpawnValidationRequest 메서드입니다.
        /// </summary>

        private void OnSpawnValidationRequest(SpawnValidationRequestInnerEvent evt)
        {
            evt.CanSpawn = CanSpawn(evt.CurrentGold, out var failReason);
            evt.SpawnCost = Config.UnitSpawnCost;
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnGameEndCheckRequest 메서드입니다.
        /// </summary>

        private void OnGameEndCheckRequest(GameEndCheckRequestInnerEvent evt)
        {
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }
        /// <summary>
        /// OnScoreCalculationRequest 메서드입니다.
        /// </summary>

        private void OnScoreCalculationRequest(ScoreCalculationRequestInnerEvent evt)
        {
            evt.CalculatedScore = CalculateMergeScore(evt.MergedGrade);
        }
        /// <summary>
        /// OnMonsterKillRewardRequest 메서드입니다.
        /// </summary>

        private void OnMonsterKillRewardRequest(MonsterKillRewardRequestInnerEvent evt)
        {
            var (gold, score) = CalculateMonsterKillReward(evt.MonsterId);
            evt.RewardGold = gold;
            evt.RewardScore = score;
        }

        #endregion

        #region 공용 내부 이벤트 핸들러
        /// <summary>
        /// OnGetSpawnCostRequest 메서드입니다.
        /// </summary>

        private void OnGetSpawnCostRequest(GetSpawnCostRequest evt)
        {
            evt.Cost = Config.UnitSpawnCost;
        }
        /// <summary>
        /// OnCanMergeRequest 메서드입니다.
        /// </summary>

        private void OnCanMergeRequest(CanMergeRequest evt)
        {
            evt.CanMerge = CanMerge(
                evt.SourceGrade,
                evt.TargetGrade,
                evt.SourceType,
                evt.TargetType,
                out var failReason);
            evt.FailReason = failReason;
        }
        /// <summary>
        /// OnCheckGameEndRequest 메서드입니다.
        /// </summary>

        private void OnCheckGameEndRequest(CheckGameEndRequest evt)
        {
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }
        /// <summary>
        /// OnCalculateRewardRequest 메서드입니다.
        /// </summary>

        private void OnCalculateRewardRequest(CalculateRewardRequest evt)
        {
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
