using System;
using System.Collections.Generic;
using MyProject.MergeGame.Commands;
using Noname.GameHost;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 전용 호스트입니다.
    /// Command를 처리하고 Result/Event를 발행합니다.
    /// </summary>
    public sealed class MergeGameHost
        : GameHostBase<MergeCommand, MergeCommandResult, MergeHostEvent, MergeHostSnapshot>
    {
        /// <summary>
        /// 게임 상태입니다.
        /// </summary>
        private readonly MergeHostState _state;

        /// <summary>
        /// 게임 설정입니다.
        /// </summary>
        private readonly MergeHostConfig _config;

        /// <summary>
        /// 스냅샷 빌드용 임시 리스트입니다.
        /// </summary>
        private readonly List<SlotSnapshot> _tempSlotSnapshots = new();

        public MergeGameHost(MergeHostConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _state = new MergeHostState();
        }

        #region GameHostBase Overrides

        protected override GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleCommand(MergeCommand command)
        {
            switch (command)
            {
                case StartMergeGameCommand startCommand:
                    return HandleStartGame(startCommand);

                case SpawnUnitCommand spawnCommand:
                    return HandleSpawnUnit(spawnCommand);

                case MergeUnitCommand mergeCommand:
                    return HandleMergeUnit(mergeCommand);

                case EndMergeGameCommand endCommand:
                    return HandleEndGame(endCommand);

                default:
                    return default;
            }
        }

        protected override void OnTick(float deltaTime)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return;
            }

            _state.AddElapsedTime(deltaTime);
        }

        protected override void HandleInternalEvent(MergeHostEvent eventData)
        {
            // 내부 이벤트 처리가 필요한 경우 구현
        }

        protected override MergeHostSnapshot BuildSnapshotInternal()
        {
            _tempSlotSnapshots.Clear();

            for (var i = 0; i < _state.Slots.Count; i++)
            {
                var slot = _state.Slots[i];
                _tempSlotSnapshots.Add(new SlotSnapshot(
                    slot.Index,
                    slot.UnitUid,
                    slot.UnitGrade
                ));
            }

            return new MergeHostSnapshot(
                tick: Tick,
                sessionPhase: _state.SessionPhase,
                score: _state.Score,
                maxGrade: _state.MaxGrade,
                totalSlots: _state.Slots.Count,
                usedSlots: _state.UsedSlotCount,
                elapsedTime: _state.ElapsedTime,
                slots: _tempSlotSnapshots.ToArray()
            );
        }

        #endregion

        #region Command Handlers

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleStartGame(StartMergeGameCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.None)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartMergeGameResult.Fail(Tick, command.SenderUid, "이미 게임이 시작되어 있습니다."));
            }

            // 슬롯 초기화
            _state.InitializeSlots(_config.SlotCount);
            _state.SetSessionPhase(MergeSessionPhase.Playing);

            var events = new List<MergeHostEvent>
            {
                new MergeGameStartedEvent(Tick, _config.SlotCount)
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                StartMergeGameResult.Ok(Tick, command.SenderUid),
                events);
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleSpawnUnit(SpawnUnitCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnUnitResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            // 슬롯 인덱스 결정
            var slotIndex = command.SlotIndex;
            if (slotIndex < 0)
            {
                slotIndex = _state.FindEmptySlotIndex();
            }

            if (slotIndex < 0)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnUnitResult.Fail(Tick, command.SenderUid, "빈 슬롯이 없습니다."));
            }

            var slot = _state.GetSlot(slotIndex);
            if (slot == null || !slot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnUnitResult.Fail(Tick, command.SenderUid, "해당 슬롯이 비어있지 않습니다."));
            }

            // 유닛 생성
            var unitUid = _state.GenerateUnitUid();
            var initialGrade = _config.InitialUnitGrade;

            slot.SetUnit(unitUid, initialGrade);
            _state.UpdateMaxGrade(initialGrade);

            var events = new List<MergeHostEvent>
            {
                new MergeUnitSpawnedEvent(Tick, unitUid, initialGrade, slotIndex)
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                SpawnUnitResult.Ok(Tick, command.SenderUid, unitUid, slotIndex),
                events);
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleMergeUnit(MergeUnitCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeUnitResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(command.FromSlotIndex);
            var toSlot = _state.GetSlot(command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeUnitResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            if (fromSlot.IsEmpty || toSlot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeUnitResult.Fail(Tick, command.SenderUid, "빈 슬롯으로는 머지할 수 없습니다."));
            }

            if (fromSlot.UnitGrade != toSlot.UnitGrade)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeUnitResult.Fail(Tick, command.SenderUid, "같은 등급의 유닛만 머지할 수 있습니다."));
            }

            // 머지 실행
            var sourceUid1 = fromSlot.UnitUid;
            var sourceUid2 = toSlot.UnitUid;
            var newGrade = fromSlot.UnitGrade + 1;
            var newUnitUid = _state.GenerateUnitUid();

            // 소스 슬롯 비우기
            fromSlot.Clear();

            // 대상 슬롯에 새 유닛 배치
            toSlot.SetUnit(newUnitUid, newGrade);

            // 점수 추가
            var scoreGained = CalculateMergeScore(newGrade);
            _state.AddScore(scoreGained);
            _state.UpdateMaxGrade(newGrade);

            var events = new List<MergeHostEvent>
            {
                new MergeUnitMergedEvent(Tick, sourceUid1, sourceUid2, newUnitUid, newGrade, command.ToSlotIndex),
                new MergeScoreChangedEvent(Tick, _state.Score, scoreGained)
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                MergeUnitResult.Ok(Tick, command.SenderUid, newUnitUid, newGrade),
                events);
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleEndGame(EndMergeGameCommand command)
        {
            if (_state.SessionPhase == MergeSessionPhase.None ||
                _state.SessionPhase == MergeSessionPhase.GameOver)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    EndMergeGameResult.Fail(Tick, command.SenderUid, "게임이 시작되지 않았습니다."));
            }

            _state.SetSessionPhase(MergeSessionPhase.GameOver);

            var events = new List<MergeHostEvent>
            {
                new MergeGameOverEvent(Tick, true, _state.Score, _state.MaxGrade)
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                EndMergeGameResult.Ok(Tick, command.SenderUid),
                events);
        }

        #endregion

        #region Helpers

        private int CalculateMergeScore(int resultGrade)
        {
            // 등급이 높을수록 더 많은 점수
            return resultGrade * _config.ScorePerGrade;
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _state?.Dispose();
        }
    }
}
