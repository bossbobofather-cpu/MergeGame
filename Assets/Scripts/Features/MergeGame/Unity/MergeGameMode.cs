using MyProject.Common.GameMode;
using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame 클라이언트 모드의 루트입니다.
    /// Host에서 전달되는 Result/Event/Snapshot을 수신해 View 모듈로 라우팅하고 로그를 출력합니다.
    /// </summary>
    public class MergeGameMode : GameMode<MergeCommand, MergeCommandResult, MergeHostEvent, MergeHostSnapshot>
    {
        [SerializeField] private long _localUserId = 1;

        private readonly Color _logColor = new Color(0f, 0f, 0f, 0.6f);
        private readonly Color _errorColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
        private readonly Color _startColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        private readonly Color _spawnColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        private readonly Color _mergeColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        private readonly Color _scoreColor = new Color(0.4f, 0.8f, 0.4f, 0.6f);
        private readonly Color _gameOverColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);

        private MergeHostSnapshot _latestSnapshot;
        private long _latestSnapshotTick = -1;

        /// <summary>
        /// Host에서 수신한 최신 스냅샷입니다.
        /// </summary>
        public MergeHostSnapshot LatestSnapshot => _latestSnapshot;

        /// <summary>
        /// 현재 클라이언트에 매핑된 로컬 유저 UID입니다.
        /// </summary>
        public long LocalUserId => _localUserId;

        /// <summary>
        /// Host로 커맨드를 전달합니다. null 커맨드는 무시합니다.
        /// </summary>
        public void SendCommand(MergeCommand command)
        {
            if (command == null)
            {
                return;
            }

            Host?.SendCommand(command);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Host?.SendCommand(new StartMergeGameCommand(_localUserId));
        }

        protected override void Update()
        {
            base.Update();
            SyncSnapshot();
            CheckInput();
        }

        private void SyncSnapshot()
        {
            var snapshot = Host?.GetLatestSnapshot();
            if (snapshot == null)
            {
                return;
            }

            if (snapshot.Tick == _latestSnapshotTick)
            {
                return;
            }

            _latestSnapshot = snapshot;
            _latestSnapshotTick = snapshot.Tick;

            // 최신 스냅샷을 모든 View 모듈에 전달합니다.
            RouteSnapshotToModules(snapshot);
        }

        private void CheckInput()
        {
            var input = GetModule<InputViewModule>();
            input?.TickInput();
        }

        protected override void OnHostResult(MergeCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            if (!result.Success)
            {
                PublishMessage($"[커맨드 실패] {result.ErrorMessage}", _errorColor);
            }
        }

        /// <summary>
        /// Host 이벤트를 모듈에 전달하고, 주요 이벤트를 시스템 메시지로 기록합니다.
        /// </summary>
        protected override void OnHostEvent(MergeHostEvent evt)
        {
            // 수신한 이벤트를 모든 View 모듈에 브로드캐스트합니다.
            RouteEventToModules(evt);

            // 디버그/시스템 메시지 출력
            switch (evt)
            {
                case MapInitializedEvent e:
                    PublishMessage($"[맵 초기화] MapId: {e.MapId}, 슬롯: {e.SlotPositions.Count}, 경로: {e.Paths.Count}", _startColor);
                    break;

                case MergeGameStartedEvent e:
                    PublishMessage($"[게임 시작] 슬롯 수: {e.SlotCount}", _startColor);
                    break;

                case WaveStartedEvent e:
                    PublishMessage($"[웨이브 시작] Wave: {e.WaveNumber}, 몬스터: {e.TotalMonsterCount}", _startColor);
                    break;

                case WaveCompletedEvent e:
                    PublishMessage($"[웨이브 완료] Wave: {e.WaveNumber}, 보너스 골드: +{e.BonusGold}", _scoreColor);
                    break;

                case TowerSpawnedEvent e:
                    PublishMessage($"[타워 생성] {e.TowerId}({e.TowerType}) G{e.Grade}, 슬롯: {e.SlotIndex}, UID: {e.TowerUid}", _spawnColor);
                    break;

                case TowerMovedEvent e:
                    PublishMessage($"[타워 이동] UID: {e.TowerUid}, {e.FromSlotIndex} -> {e.ToSlotIndex}", _logColor);
                    break;

                case TowerMergedEvent e:
                    PublishMessage($"[타워 머지] {e.SourceTowerUid} + {e.TargetTowerUid} -> {e.ResultTowerUid} (G{e.ResultGrade}) 슬롯: {e.SlotIndex}", _mergeColor);
                    break;

                case TowerRemovedEvent e:
                    PublishMessage($"[타워 제거] UID: {e.TowerUid}, 슬롯: {e.SlotIndex}, 사유: {e.Reason}", _logColor);
                    break;

                case TowerAttackedEvent e:
                    PublishMessage($"[타워 공격] {e.AttackerUid} -> {e.TargetUid}, 데미지: {e.Damage}", _logColor);
                    break;

                case MonsterSpawnedEvent e:
                    PublishMessage($"[몬스터 생성] {e.MonsterId}, UID: {e.MonsterUid}, Path: {e.PathIndex}", _spawnColor);
                    break;

                case MonsterDamagedEvent e:
                    PublishMessage($"[몬스터 피격] UID: {e.MonsterUid}, -{e.Damage} (HP: {e.CurrentHealth})", _logColor);
                    break;

                case MonsterDiedEvent e:
                    PublishMessage($"[몬스터 처치] UID: {e.MonsterUid}, 보상 골드: +{e.GoldReward}", _scoreColor);
                    break;

                case PlayerHpChangedEvent e:
                    PublishMessage($"[HP] {e.CurrentHp}/{e.MaxHp} ({e.HpDelta:+#;-#;0}) 이유: {e.Reason}", _logColor);
                    break;

                case PlayerGoldChangedEvent e:
                    PublishMessage($"[골드] {e.CurrentGold} ({e.GoldDelta:+#;-#;0}) 이유: {e.Reason}", _scoreColor);
                    break;

                case MergeScoreChangedEvent e:
                    PublishMessage($"[점수] +{e.ScoreDelta} (총: {e.CurrentScore})", _scoreColor);
                    break;

                case MergeEffectTriggeredEvent e:
                    PublishMessage($"[이펙트] {e.EffectId} at ({e.PositionX:F1},{e.PositionY:F1})", _mergeColor);
                    break;

                case MergeGameOverEvent e:
                    PublishMessage(
                        $"[게임 종료] 승리: {e.IsVictory}, 최종 점수: {e.FinalScore}, 최고 등급: {e.MaxGradeReached}",
                        _gameOverColor);
                    break;

                case MonsterMovedEvent:
                case MonsterReachedGoalEvent:
                    // 이동 이벤트는 너무 자주 발생하므로 기본 로그에서 제외합니다.
                    break;

                case MergeUnitSpawnedEvent:
                case MergeUnitMergedEvent:
                case MergeUnitRemovedEvent:
                    // 레거시(Unit) 이벤트는 타워 이벤트로 대체되었습니다.
                    break;
            }
        }

        #region Module Event Routing

        /// <summary>
        /// Host 이벤트를 모든 View 모듈에 전달합니다.
        /// </summary>
        private void RouteEventToModules(MergeHostEvent evt)
        {
            for (var i = 0; i < Modules.Count; i++)
            {
                if (Modules[i] is IMergeViewModule viewModule)
                {
                    viewModule.OnHostEvent(evt);
                }
            }
        }

        /// <summary>
        /// 최신 스냅샷을 모든 View 모듈에 전달합니다.
        /// </summary>
        private void RouteSnapshotToModules(MergeHostSnapshot snapshot)
        {
            for (var i = 0; i < Modules.Count; i++)
            {
                if (Modules[i] is IMergeViewModule viewModule)
                {
                    viewModule.OnSnapshotUpdated(snapshot);
                }
            }
        }

        #endregion

        /// <summary>
        /// 지정한 색상으로 시스템 메시지와 콘솔 로그를 동시에 출력합니다.
        /// </summary>
        private void PublishMessage(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log($"[MergeGameMode] {message}");
        }

        /// <summary>
        /// 기본 로그 색상을 사용해 시스템 메시지를 출력합니다.
        /// </summary>
        private void PublishMessage(string message)
        {
            PublishMessage(message, _logColor);
        }
    }
}