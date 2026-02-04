using System;
using MyProject.Common.GameMode;
using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using UnityEngine;

namespace MyProject.MergeGame.Presentation
{
    /// <summary>
    /// MergeGame의 프레젠테이션 모드입니다.
    /// 호스트 이벤트를 수신해 시스템 메시지로 출력합니다.
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

        private MergeHostSnapshot _latestSnapshot = null;
        private long _latestSnapshotTick = -1;

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
        }

        private void CheckInput()
        {
            // if (_latestSnapshot == null || _latestSnapshot.SessionPhase != MergeSessionPhase.Playing)
            // {
            //     return;
            // }

            // var keyboard = Keyboard.current;
            // if (keyboard == null)
            // {
            //     return;
            // }

            // // Space: 유닛 스폰
            // if (keyboard.spaceKey.wasPressedThisFrame)
            // {
            //     Host?.SendCommand(new SpawnUnitCommand(_localUserId));
            // }

            // // 1-9: 슬롯 선택 (머지용)
            // // 간단한 예시: 1번 키로 첫 번째 슬롯, 2번 키로 두 번째 슬롯 선택 후 머지
            // // 실제 구현에서는 드래그앤드롭 UI가 필요합니다.
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
            switch (evt)
            {
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

        /// <summary>
        /// 시스템 메시지로 로그를 출력합니다.
        /// </summary>
        private void PublishMessage(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log($"[MergeGame] {message}");
        }

        /// <summary>
        /// 기본 색상으로 시스템 메시지를 출력합니다.
        /// </summary>
        private void PublishMessage(string message)
        {
            PublishMessage(message, _logColor);
        }

        internal void Initialize(MergeGameHost mergeGameHost)
        {
            throw new NotImplementedException();
        }

    }
}
