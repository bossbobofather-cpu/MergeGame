using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 규칙 모듈입니다.
    /// 스폰 규칙, 머지 규칙, 승리/패배 조건을 관리합니다.
    /// </summary>
    public sealed class RuleModule : HostModuleBase<RuleModuleConfig>
    {
        public const string MODULE_ID = "rule";

        /// <inheritdoc />
        public override string ModuleId => MODULE_ID;

        /// <inheritdoc />
        public override bool IsRequired => true;

        /// <inheritdoc />
        public override int Priority => 90; // MapModule 다음으로 초기화

        /// <summary>
        /// 승리 조건입니다.
        /// </summary>
        public VictoryConditionType VictoryCondition => Config.VictoryCondition;

        /// <summary>
        /// 패배 조건입니다.
        /// </summary>
        public DefeatConditionType DefeatCondition => Config.DefeatCondition;

        /// <summary>
        /// 스폰 규칙입니다.
        /// </summary>
        public SpawnRuleType SpawnRule => Config.SpawnRule;

        /// <summary>
        /// 유닛 스폰 비용입니다.
        /// </summary>
        public int UnitSpawnCost => Config.UnitSpawnCost;

        /// <summary>
        /// 최대 유닛 등급입니다.
        /// </summary>
        public int MaxUnitGrade => Config.MaxUnitGrade;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // 내부 이벤트 구독 (모듈 전용)
            SubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            SubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            SubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            SubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            SubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);
            SubscribeInnerEvent<WaveCompletionRewardRequestInnerEvent>(OnWaveCompletionRewardRequest);

            // 공유 내부 이벤트 구독 (다른 모듈에서 호출)
            SubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            SubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            SubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            SubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            // 내부 이벤트 구독 해제 (모듈 전용)
            UnsubscribeInnerEvent<MergeValidationRequestInnerEvent>(OnMergeValidationRequest);
            UnsubscribeInnerEvent<SpawnValidationRequestInnerEvent>(OnSpawnValidationRequest);
            UnsubscribeInnerEvent<GameEndCheckRequestInnerEvent>(OnGameEndCheckRequest);
            UnsubscribeInnerEvent<ScoreCalculationRequestInnerEvent>(OnScoreCalculationRequest);
            UnsubscribeInnerEvent<MonsterKillRewardRequestInnerEvent>(OnMonsterKillRewardRequest);
            UnsubscribeInnerEvent<WaveCompletionRewardRequestInnerEvent>(OnWaveCompletionRewardRequest);

            // 공유 내부 이벤트 구독 해제
            UnsubscribeInnerEvent<GetSpawnCostRequest>(OnGetSpawnCostRequest);
            UnsubscribeInnerEvent<CanMergeRequest>(OnCanMergeRequest);
            UnsubscribeInnerEvent<CheckGameEndRequest>(OnCheckGameEndRequest);
            UnsubscribeInnerEvent<CalculateRewardRequest>(OnCalculateRewardRequest);
        }

        #region 검증 메서드

        /// <summary>
        /// 머지 가능 여부를 검증합니다.
        /// </summary>
        public bool CanMerge(int sourceGrade, int targetGrade, string sourceType, string targetType, out string failReason)
        {
            failReason = null;

            // 등급 체크
            if (sourceGrade != targetGrade)
            {
                failReason = "등급이 다릅니다.";
                return false;
            }

            // 최대 등급 체크
            if (sourceGrade >= Config.MaxUnitGrade)
            {
                failReason = "최대 등급에 도달했습니다.";
                return false;
            }

            // 타입 체크 (설정에 따라)
            if (Config.RequireSameTypeForMerge && sourceType != targetType)
            {
                failReason = "타입이 다릅니다.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 스폰 가능 여부를 검증합니다.
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
        /// 게임 종료 여부를 검증합니다.
        /// </summary>
        public (bool isEnd, bool isVictory) CheckGameEnd(
            int currentHp,
            int currentWaveNumber,
            int currentScore,
            int maxUnitGrade)
        {
            // 패배 조건 체크
            switch (Config.DefeatCondition)
            {
                case DefeatConditionType.PlayerDeath:
                    if (currentHp <= 0)
                    {
                        return (true, false);
                    }
                    break;
            }

            // 승리 조건 체크
            switch (Config.VictoryCondition)
            {
                case VictoryConditionType.WaveClear:
                    if (currentWaveNumber >= Config.VictoryConditionValue)
                    {
                        return (true, true);
                    }
                    break;

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

        #region 계산 메서드

        /// <summary>
        /// 머지 점수를 계산합니다.
        /// </summary>
        public int CalculateMergeScore(int resultGrade)
        {
            return resultGrade * Config.ScorePerGrade;
        }

        /// <summary>
        /// 몬스터 처치 보상을 계산합니다.
        /// </summary>
        public (int gold, int score) CalculateMonsterKillReward(string monsterId)
        {
            // 기본 보상
            return (Config.GoldPerMonsterKill, 10);
        }

        /// <summary>
        /// 웨이브 완료 보상을 계산합니다.
        /// </summary>
        public int CalculateWaveCompletionReward(int waveNumber)
        {
            // 웨이브 번호에 따라 보상 증가
            return Config.WaveCompletionBonusGold + (waveNumber - 1) * 10;
        }

        #endregion

        #region 내부 이벤트 핸들러

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

        private void OnSpawnValidationRequest(SpawnValidationRequestInnerEvent evt)
        {
            evt.CanSpawn = CanSpawn(evt.CurrentGold, out var failReason);
            evt.SpawnCost = Config.UnitSpawnCost;
            evt.FailReason = failReason;
        }

        private void OnGameEndCheckRequest(GameEndCheckRequestInnerEvent evt)
        {
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentWaveNumber,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }

        private void OnScoreCalculationRequest(ScoreCalculationRequestInnerEvent evt)
        {
            evt.CalculatedScore = CalculateMergeScore(evt.MergedGrade);
        }

        private void OnMonsterKillRewardRequest(MonsterKillRewardRequestInnerEvent evt)
        {
            var (gold, score) = CalculateMonsterKillReward(evt.MonsterId);
            evt.RewardGold = gold;
            evt.RewardScore = score;
        }

        private void OnWaveCompletionRewardRequest(WaveCompletionRewardRequestInnerEvent evt)
        {
            evt.RewardGold = CalculateWaveCompletionReward(evt.WaveNumber);
        }

        #endregion

        #region 공유 내부 이벤트 핸들러

        private void OnGetSpawnCostRequest(GetSpawnCostRequest evt)
        {
            evt.Cost = Config.UnitSpawnCost;
        }

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

        private void OnCheckGameEndRequest(CheckGameEndRequest evt)
        {
            var (isEnd, isVictory) = CheckGameEnd(
                evt.CurrentHp,
                evt.CurrentWave,
                evt.CurrentScore,
                evt.MaxUnitGrade);

            evt.IsGameEnd = isEnd;
            evt.IsVictory = isVictory;
        }

        private void OnCalculateRewardRequest(CalculateRewardRequest evt)
        {
            switch (evt.Type)
            {
                case CalculateRewardRequest.RewardType.MonsterKill:
                    var (gold, score) = CalculateMonsterKillReward(evt.Value.ToString());
                    evt.GoldReward = gold;
                    evt.ScoreReward = score;
                    break;

                case CalculateRewardRequest.RewardType.WaveComplete:
                    evt.GoldReward = CalculateWaveCompletionReward(evt.Value);
                    break;

                case CalculateRewardRequest.RewardType.Merge:
                    evt.ScoreReward = CalculateMergeScore(evt.Value);
                    break;
            }
        }

        #endregion
    }
}
