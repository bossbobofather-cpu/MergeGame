using MyProject.Common.GameMode;
using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame의 뷰입니다.
    /// 호스트 이벤트를 수신해 모듈에 라우팅하고 시스템 메시지로 출력합니다.
    /// </summary>
    public class MergeGameView : GameMode<MergeCommand, MergeCommandResult, MergeHostEvent, MergeHostSnapshot>
    {
        [SerializeField] private long _localUserId = 1;

        private readonly Color _logColor = new Color(0f, 0f, 0f, 0.6f);
        private readonly Color _errorColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
        private readonly Color _startColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        private readonly Color _spawnColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        private readonly Color _mergeColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        private readonly Color _scoreColor = new Color(0.4f, 0.8f, 0.4f, 0.6f);
        private readonly Color _gameOverColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);

        private MergeHostSnapshot _latestSnapshot = null;
        private long _latestSnapshotTick = -1;

        /// <summary>
        /// 최신 스냅샷입니다.
        /// </summary>
        public MergeHostSnapshot LatestSnapshot => _latestSnapshot;
        /// <summary>
        /// 로컬 유저 UID입니다.
        /// </summary>
        public long LocalUserId => _localUserId;

        /// <summary>
        /// 호스트에 커맨드를 전송합니다.
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

            // 모듈에 스냅샷 전파
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
        /// 호스트 이벤트를 처리합니다.
        /// </summary>
        protected override void OnHostEvent(MergeHostEvent evt)
        {
            // 모듈에 이벤트 전파
            RouteEventToModules(evt);

            // 로그 출력
            switch (evt)
            {
                case MapInitializedEvent e:
                    PublishMessage($"[맵 초기화] MapId: {e.MapId}, 슬롯: {e.SlotPositions.Count}, 경로: {e.Paths.Count}", _startColor);
                    break;

                case MergeGameStartedEvent e:
                    PublishMessage($"[게임 시작] 슬롯 수: {e.SlotCount}", _startColor);
                    break;

                case MergeUnitSpawnedEvent e:
                    PublishMessage($"[유닛 생성] 등급: {e.Grade}, 슬롯: {e.SlotIndex}, UID: {e.UnitUid}", _spawnColor);
                    break;

                case MergeUnitMergedEvent e:
                    PublishMessage($"[머지 성공] 새 등급: {e.ResultGrade}, 슬롯: {e.SlotIndex}, UID: {e.ResultUnitUid}", _mergeColor);
                    break;

                case MergeUnitRemovedEvent e:
                    PublishMessage($"[유닛 제거] 슬롯: {e.SlotIndex}, UID: {e.UnitUid}", _logColor);
                    break;

                case MergeScoreChangedEvent e:
                    PublishMessage($"[점수] +{e.ScoreDelta} (총: {e.CurrentScore})", _scoreColor);
                    break;

                case MergeGameOverEvent e:
                    PublishMessage(
                        $"[게임 종료] 승리: {e.IsVictory}, 최종 점수: {e.FinalScore}, 최고 등급: {e.MaxGradeReached}",
                        _gameOverColor);
                    break;
            }
        }

        #region Module Routing

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
        /// 시스템 메시지로 로그를 출력합니다.
        /// </summary>
        private void PublishMessage(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log($"[MergeGameView] {message}");
        }

        /// <summary>
        /// 기본 색상으로 시스템 메시지를 출력합니다.
        /// </summary>
        private void PublishMessage(string message)
        {
            PublishMessage(message, _logColor);
        }
    }
}

