using System;
using System.Collections.Generic;
using System.Linq;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Models;
using MyProject.MergeGame.Modules;
using MyProject.MergeGame.Systems;
using Noname.GameAbilitySystem;
using Noname.GameHost;
using Noname.GameHost.Module;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 전용 호스트입니다.
    /// Command를 처리하고 Result/Event를 발행합니다.
    /// IHostContext를 구현하여 모듈들에게 컨텍스트를 제공합니다.
    /// </summary>
    public sealed class MergeGameHost
        : GameHostBase<MergeCommand, MergeCommandResult, MergeHostEvent, MergeHostSnapshot>,
          IHostContext
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
        /// 전투 시스템입니다.
        /// </summary>
        private readonly MergeCombatSystem _combatSystem;

        /// <summary>
        /// 머지 이펙트 시스템입니다.
        /// </summary>
        private readonly MergeEffectSystem _effectSystem;

        /// <summary>
        /// 몬스터 이동 시스템입니다.
        /// </summary>
        private readonly MonsterMovementSystem _movementSystem;

        /// <summary>
        /// 스냅샷 빌드용 임시 리스트입니다.
        /// </summary>
        private readonly List<SlotSnapshot> _tempSlotSnapshots = new();
        private readonly List<CharacterSnapshot> _tempCharacterSnapshots = new();
        private readonly List<MonsterSnapshot> _tempMonsterSnapshots = new();

        /// <summary>
        /// 웨이브 스폰용 타이머입니다.
        /// </summary>
        private float _waveSpawnTimer;
        private int _waveSpawnedCount;
        private int _waveTotalCount;

        /// <summary>
        /// 이벤트 버퍼입니다.
        /// </summary>
        private readonly List<MergeHostEvent> _tickEventBuffer = new();

        /// <summary>
        /// 캐릭터 정의 조회 콜백입니다.
        /// </summary>
        public Func<string, CharacterDefinition> GetCharacterDefinition { get; set; }

        /// <summary>
        /// 랜덤 캐릭터 ID 획득 콜백입니다 (머지 결과용).
        /// </summary>
        public Func<int, string> GetRandomCharacterIdForGrade { get; set; }

        #region Module System

        /// <summary>
        /// 내부 이벤트 버스입니다.
        /// </summary>
        private readonly InnerEventBus _innerEventBus = new();

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        private readonly List<IHostModule> _modules = new();

        /// <summary>
        /// 모듈 ID로 인덱싱된 맵입니다.
        /// </summary>
        private readonly Dictionary<string, IHostModule> _moduleMap = new();

        /// <summary>
        /// 모듈 초기화 여부입니다.
        /// </summary>
        private bool _modulesInitialized;

        /// <inheritdoc />
        long IHostContext.CurrentTick => Tick;

        /// <inheritdoc />
        IInnerEventBus IHostContext.InnerEventBus => _innerEventBus;

        /// <inheritdoc />
        void IHostContext.PublishEvent<TEvent>(TEvent eventData)
        {
            if (eventData is MergeHostEvent mergeEvent)
            {
                PublishEvent(mergeEvent);
            }
        }

        /// <inheritdoc />
        TModule IHostContext.GetModule<TModule>()
        {
            return _modules.OfType<TModule>().FirstOrDefault();
        }

        /// <inheritdoc />
        IHostModule IHostContext.GetModule(string moduleId)
        {
            return _moduleMap.TryGetValue(moduleId, out var module) ? module : null;
        }

        #endregion

        public MergeGameHost(MergeHostConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _state = new MergeHostState();

            _combatSystem = new MergeCombatSystem(_state, _config.DefaultAttackRange);
            _effectSystem = new MergeEffectSystem(_state);
            _movementSystem = new MonsterMovementSystem(_state);

            // 내부 이벤트 구독 (모듈 → Host 통신용)
            _innerEventBus.Subscribe<MonsterSpawnRequestInnerEvent>(OnMonsterSpawnRequest);
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

                // 새 커맨드 핸들러
                case SpawnCharacterCommand spawnCharCmd:
                    return HandleSpawnCharacter(spawnCharCmd);

                case MergeCharacterCommand mergeCharCmd:
                    return HandleMergeCharacter(mergeCharCmd);

                case MoveCharacterCommand moveCharCmd:
                    return HandleMoveCharacter(moveCharCmd);

                case StartWaveCommand startWaveCmd:
                    return HandleStartWave(startWaveCmd);

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
            _tickEventBuffer.Clear();

            // 0. 모듈 틱
            TickModules(deltaTime);

            // 1. 전투 시스템 업데이트
            var combatEvents = _combatSystem.Tick(Tick, deltaTime);
            foreach (var evt in combatEvents)
            {
                _tickEventBuffer.Add(evt);
            }

            // 2. 몬스터 이동 업데이트
            var moveEvents = _movementSystem.Tick(Tick, deltaTime);
            foreach (var evt in moveEvents)
            {
                _tickEventBuffer.Add(evt);
            }

            // 3. 목적지 도달 몬스터 처리
            ProcessMonstersReachedGoal();

            // 4. 사망한 몬스터 처리
            ProcessDeadMonsters();

            // 5. 웨이브 스폰 처리
            ProcessWaveSpawning(deltaTime);

            // 6. 웨이브 완료 체크
            CheckWaveCompletion();

            // 7. 게임 오버 체크
            if (_state.PlayerHp <= 0)
            {
                HandleGameOver(false);
            }

            // 이벤트 발행
            foreach (var evt in _tickEventBuffer)
            {
                PublishEvent(evt);
            }
        }

        protected override void HandleInternalEvent(MergeHostEvent eventData)
        {
            // 내부 이벤트 처리가 필요한 경우 구현
        }

        protected override MergeHostSnapshot BuildSnapshotInternal()
        {
            _tempSlotSnapshots.Clear();
            _tempCharacterSnapshots.Clear();
            _tempMonsterSnapshots.Clear();

            for (var i = 0; i < _state.Slots.Count; i++)
            {
                var slot = _state.Slots[i];
                _tempSlotSnapshots.Add(new SlotSnapshot(
                    slot.Index,
                    slot.UnitUid,
                    slot.UnitGrade
                ));
            }

            foreach (var character in _state.Characters.Values)
            {
                _tempCharacterSnapshots.Add(new CharacterSnapshot(
                    character.Uid,
                    character.CharacterId,
                    character.CharacterType,
                    character.Grade,
                    character.SlotIndex,
                    character.Position.X,
                    character.Position.Y,
                    character.ASC.Get(AttributeId.AttackDamage),
                    character.ASC.Get(AttributeId.AttackSpeed),
                    character.ASC.Get(AttributeId.AttackRange)
                ));
            }

            foreach (var monster in _state.Monsters.Values)
            {
                _tempMonsterSnapshots.Add(new MonsterSnapshot(
                    monster.Uid,
                    monster.MonsterId,
                    monster.PathIndex,
                    monster.PathProgress,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.ASC.Get(AttributeId.Health),
                    monster.ASC.Get(AttributeId.MaxHealth)
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
                slots: _tempSlotSnapshots.ToArray(),
                playerHp: _state.PlayerHp,
                playerMaxHp: _state.PlayerMaxHp,
                playerGold: _state.PlayerGold,
                currentWaveNumber: _state.CurrentWaveNumber,
                wavePhase: _state.CurrentWavePhase,
                characters: _tempCharacterSnapshots.ToArray(),
                monsters: _tempMonsterSnapshots.ToArray()
            );
        }

        #endregion

        #region Command Handlers - Legacy

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleStartGame(StartMergeGameCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.None)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartMergeGameResult.Fail(Tick, command.SenderUid, "이미 게임이 시작되어 있습니다."));
            }

            // MapModule에서 슬롯 데이터를 가져와서 State 초기화
            var mapModule = GetModule<MapModule>();
            if (mapModule == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartMergeGameResult.Fail(Tick, command.SenderUid, "MapModule이 등록되지 않았습니다."));
            }

            _state.InitializeSlots(mapModule.Slots);
            _state.SetSessionPhase(MergeSessionPhase.Playing);
            _state.SetPlayerHp(_config.PlayerMaxHp, _config.PlayerMaxHp);
            _state.SetPlayerGold(_config.PlayerStartGold);

            // MapModule에서 경로를 State에 등록
            foreach (var mapPath in mapModule.Paths)
            {
                var monsterPath = new Models.MonsterPath(mapPath.PathIndex, mapPath.Waypoints);
                _state.SetMonsterPath(mapPath.PathIndex, monsterPath);
            }

            // MapInitializedEvent 생성 (View용)
            var slotPositions = new List<SlotPositionData>();
            foreach (var slot in mapModule.Slots)
            {
                slotPositions.Add(new SlotPositionData(slot.Index, slot.Position.X, slot.Position.Y));
            }

            var pathDataList = new List<PathData>();
            foreach (var path in mapModule.Paths)
            {
                var waypoints = new List<PathWaypointData>();
                foreach (var wp in path.Waypoints)
                {
                    waypoints.Add(new PathWaypointData(wp.X, wp.Y));
                }
                pathDataList.Add(new PathData(path.PathIndex, waypoints));
            }

            var events = new List<MergeHostEvent>
            {
                new MapInitializedEvent(Tick, mapModule.MapId, slotPositions, pathDataList),
                new MergeGameStartedEvent(Tick, _state.Slots.Count)
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

        #region Command Handlers - New

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleSpawnCharacter(SpawnCharacterCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnCharacterResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
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
                    SpawnCharacterResult.Fail(Tick, command.SenderUid, "빈 슬롯이 없습니다."));
            }

            var slot = _state.GetSlot(slotIndex);
            if (slot == null || !slot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnCharacterResult.Fail(Tick, command.SenderUid, "해당 슬롯이 비어있지 않습니다."));
            }

            // 캐릭터 정의 조회
            var definition = GetCharacterDefinition?.Invoke(command.CharacterId);
            if (definition == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnCharacterResult.Fail(Tick, command.SenderUid, "캐릭터 정의를 찾을 수 없습니다."));
            }

            // 캐릭터 생성
            var character = _state.CreateCharacter(
                definition.CharacterId,
                definition.CharacterType,
                definition.InitialGrade,
                slotIndex,
                slot.Position,
                definition.OnMergeSourceEffectId,
                definition.OnMergeTargetEffectId
            );

            // ASC 초기화
            character.ASC.Set(AttributeId.AttackDamage, definition.BaseAttackDamage);
            character.ASC.Set(AttributeId.AttackSpeed, definition.BaseAttackSpeed);
            character.ASC.Set(AttributeId.AttackRange, definition.BaseAttackRange);

            // 슬롯에 캐릭터 배치
            slot.SetUnit(character.Uid, character.Grade);

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, slotIndex, true));

            var events = new List<MergeHostEvent>
            {
                new CharacterSpawnedEvent(
                    Tick,
                    character.Uid,
                    character.CharacterId,
                    character.CharacterType,
                    character.Grade,
                    slotIndex,
                    character.Position.X,
                    character.Position.Y
                )
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                SpawnCharacterResult.Ok(
                    Tick,
                    command.SenderUid,
                    character.Uid,
                    character.CharacterId,
                    character.CharacterType,
                    character.Grade,
                    slotIndex
                ),
                events
            );
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleMergeCharacter(MergeCharacterCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeCharacterResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(command.FromSlotIndex);
            var toSlot = _state.GetSlot(command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeCharacterResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            // 캐릭터 찾기
            var sourceChar = _state.GetCharacterBySlot(command.FromSlotIndex);
            var targetChar = _state.GetCharacterBySlot(command.ToSlotIndex);

            if (sourceChar == null || targetChar == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeCharacterResult.Fail(Tick, command.SenderUid, "캐릭터를 찾을 수 없습니다."));
            }

            // 머지 가능 여부 확인
            if (!sourceChar.CanMergeWith(targetChar))
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeCharacterResult.Fail(Tick, command.SenderUid, "같은 타입과 등급의 캐릭터만 머지할 수 있습니다."));
            }

            // 새 등급 계산
            var newGrade = sourceChar.Grade + 1;

            // 새 캐릭터 ID 결정 (덱에서 랜덤)
            var newCharacterId = GetRandomCharacterIdForGrade?.Invoke(newGrade) ?? sourceChar.CharacterId;
            var newDefinition = GetCharacterDefinition?.Invoke(newCharacterId);

            // 결과 캐릭터 생성
            var resultChar = _state.CreateCharacter(
                newCharacterId,
                newDefinition?.CharacterType ?? sourceChar.CharacterType,
                newGrade,
                command.ToSlotIndex,
                toSlot.Position,
                newDefinition?.OnMergeSourceEffectId,
                newDefinition?.OnMergeTargetEffectId
            );

            // ASC 초기화 (등급에 따른 스케일링)
            var gradeMultiplier = 1f + (newGrade - 1) * 0.2f;
            resultChar.ASC.Set(AttributeId.AttackDamage, (newDefinition?.BaseAttackDamage ?? 10f) * gradeMultiplier);
            resultChar.ASC.Set(AttributeId.AttackSpeed, newDefinition?.BaseAttackSpeed ?? 1f);
            resultChar.ASC.Set(AttributeId.AttackRange, newDefinition?.BaseAttackRange ?? 5f);

            // 머지 이펙트 적용
            var effectResult = _effectSystem.ApplyMergeEffects(Tick, sourceChar, targetChar, resultChar);

            // 골드 처리
            if (effectResult.BonusGold > 0)
            {
                _state.AddPlayerGold(effectResult.BonusGold);
            }

            // 소스 캐릭터 제거
            _state.RemoveCharacter(sourceChar.Uid);
            fromSlot.Clear();

            // 타겟 캐릭터 제거
            _state.RemoveCharacter(targetChar.Uid);

            // 결과 캐릭터 슬롯에 배치
            toSlot.SetUnit(resultChar.Uid, resultChar.Grade);

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.FromSlotIndex, false));
            // toSlot은 여전히 점유 상태이므로 별도 알림 불필요

            // 점수 추가
            var scoreGained = CalculateMergeScore(newGrade);
            _state.AddScore(scoreGained);
            _state.UpdateMaxGrade(newGrade);

            var events = new List<MergeHostEvent>
            {
                new CharacterMergedEvent(
                    Tick,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.CharacterId,
                    resultChar.CharacterType,
                    resultChar.Grade,
                    command.ToSlotIndex
                ),
                new MergeScoreChangedEvent(Tick, _state.Score, scoreGained)
            };

            // 이펙트 이벤트 추가
            foreach (var effectEvent in effectResult.Events)
            {
                events.Add(effectEvent);
            }

            // 골드 이벤트
            if (effectResult.BonusGold > 0)
            {
                events.Add(new PlayerGoldChangedEvent(
                    Tick,
                    _state.PlayerGold,
                    effectResult.BonusGold,
                    "MergeBonus"
                ));
            }

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                MergeCharacterResult.Ok(
                    Tick,
                    command.SenderUid,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.CharacterId,
                    resultChar.CharacterType,
                    resultChar.Grade,
                    command.ToSlotIndex
                ),
                events
            );
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleMoveCharacter(MoveCharacterCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveCharacterResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(command.FromSlotIndex);
            var toSlot = _state.GetSlot(command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveCharacterResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            if (fromSlot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveCharacterResult.Fail(Tick, command.SenderUid, "이동할 캐릭터가 없습니다."));
            }

            if (!toSlot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveCharacterResult.Fail(Tick, command.SenderUid, "대상 슬롯이 비어있지 않습니다."));
            }

            var character = _state.GetCharacterBySlot(command.FromSlotIndex);
            if (character == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveCharacterResult.Fail(Tick, command.SenderUid, "캐릭터를 찾을 수 없습니다."));
            }

            // 캐릭터 이동
            character.SlotIndex = command.ToSlotIndex;
            character.Position = toSlot.Position;

            // 슬롯 상태 업데이트
            toSlot.SetUnit(fromSlot.UnitUid, fromSlot.UnitGrade);
            fromSlot.Clear();

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.FromSlotIndex, false));
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.ToSlotIndex, true));

            var events = new List<MergeHostEvent>
            {
                new CharacterMovedEvent(
                    Tick,
                    character.Uid,
                    command.FromSlotIndex,
                    command.ToSlotIndex,
                    character.Position.X,
                    character.Position.Y
                )
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                MoveCharacterResult.Ok(
                    Tick,
                    command.SenderUid,
                    character.Uid,
                    command.FromSlotIndex,
                    command.ToSlotIndex
                ),
                events
            );
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleStartWave(StartWaveCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartWaveResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            if (_state.CurrentWavePhase == WavePhase.Spawning || _state.CurrentWavePhase == WavePhase.InProgress)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartWaveResult.Fail(Tick, command.SenderUid, "웨이브가 이미 진행 중입니다."));
            }

            // 웨이브 번호 결정
            var waveNumber = command.WaveNumber > 0 ? command.WaveNumber : _state.CurrentWaveNumber + 1;
            _state.SetCurrentWaveNumber(waveNumber);
            _state.SetWavePhase(WavePhase.Spawning);

            // 웨이브 몬스터 수 (웨이브 번호에 따라 증가)
            _waveTotalCount = 5 + waveNumber * 2;
            _waveSpawnedCount = 0;
            _waveSpawnTimer = 0;

            var events = new List<MergeHostEvent>
            {
                new WaveStartedEvent(Tick, waveNumber, _waveTotalCount)
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                StartWaveResult.Ok(Tick, command.SenderUid, waveNumber, _waveTotalCount),
                events
            );
        }

        #endregion

        #region Tick Processing

        private void ProcessMonstersReachedGoal()
        {
            foreach (var monsterUid in _movementSystem.MonstersReachedGoal)
            {
                var monster = _state.GetMonster(monsterUid);
                if (monster == null)
                {
                    continue;
                }

                // 플레이어에게 데미지
                var hpBefore = _state.PlayerHp;
                _state.DamagePlayer(monster.DamageToPlayer);

                _tickEventBuffer.Add(new PlayerHpChangedEvent(
                    Tick,
                    _state.PlayerHp,
                    _state.PlayerMaxHp,
                    _state.PlayerHp - hpBefore,
                    "MonsterReachedGoal"
                ));

                // 몬스터 제거
                _state.RemoveMonster(monsterUid);
            }
        }

        private void ProcessDeadMonsters()
        {
            var deadMonsterUids = new List<long>();

            foreach (var monster in _state.Monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    deadMonsterUids.Add(monster.Uid);
                }
            }

            foreach (var uid in deadMonsterUids)
            {
                var monster = _state.GetMonster(uid);
                if (monster == null)
                {
                    continue;
                }

                // 골드 보상
                if (monster.GoldReward > 0)
                {
                    _state.AddPlayerGold(monster.GoldReward);

                    _tickEventBuffer.Add(new PlayerGoldChangedEvent(
                        Tick,
                        _state.PlayerGold,
                        monster.GoldReward,
                        "MonsterKill"
                    ));
                }

                // 사망 이벤트 (외부 - View/Client용)
                _tickEventBuffer.Add(new MonsterDiedEvent(
                    Tick,
                    monster.Uid,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.GoldReward,
                    0
                ));

                // 사망 알림 (내부 - WaveModule용)
                _innerEventBus.Publish(new MonsterDiedInnerEvent(
                    Tick,
                    monster.Uid,
                    _state.CurrentWaveNumber
                ));

                _state.RemoveMonster(uid);
            }
        }

        private void ProcessWaveSpawning(float deltaTime)
        {
            if (_state.CurrentWavePhase != WavePhase.Spawning)
            {
                return;
            }

            _waveSpawnTimer += deltaTime;

            while (_waveSpawnTimer >= _config.WaveSpawnInterval && _waveSpawnedCount < _waveTotalCount)
            {
                _waveSpawnTimer -= _config.WaveSpawnInterval;
                SpawnWaveMonster();
                _waveSpawnedCount++;
            }

            // 모든 몬스터 스폰 완료
            if (_waveSpawnedCount >= _waveTotalCount)
            {
                _state.SetWavePhase(WavePhase.InProgress);
            }
        }

        private void SpawnWaveMonster()
        {
            // 기본 몬스터 스폰 (실제로는 웨이브 데이터에서 가져와야 함)
            var pathIndex = 0;
            var path = _state.GetMonsterPath(pathIndex);
            if (path == null)
            {
                return;
            }

            var monster = _state.CreateMonster(
                "monster_basic",
                pathIndex,
                path.GetStartPosition(),
                damageToPlayer: 10,
                goldReward: 10 + _state.CurrentWaveNumber
            );

            // ASC 초기화 (웨이브에 따라 스케일링)
            var waveMultiplier = 1f + (_state.CurrentWaveNumber - 1) * 0.1f;
            monster.ASC.Set(AttributeId.MaxHealth, 100f * waveMultiplier);
            monster.ASC.Set(AttributeId.Health, 100f * waveMultiplier);
            monster.ASC.Set(AttributeId.MoveSpeed, 2f);

            _tickEventBuffer.Add(new MonsterSpawnedEvent(
                Tick,
                monster.Uid,
                monster.MonsterId,
                monster.PathIndex,
                monster.Position.X,
                monster.Position.Y,
                monster.ASC.Get(AttributeId.MaxHealth)
            ));
        }

        private void CheckWaveCompletion()
        {
            if (_state.CurrentWavePhase != WavePhase.InProgress)
            {
                return;
            }

            // 모든 몬스터가 처리되었는지 확인
            if (_state.Monsters.Count == 0)
            {
                _state.SetWavePhase(WavePhase.Completed);

                // 웨이브 보너스 골드
                _state.AddPlayerGold(_config.WaveCompletionBonusGold);

                _tickEventBuffer.Add(new WaveCompletedEvent(
                    Tick,
                    _state.CurrentWaveNumber,
                    _config.WaveCompletionBonusGold
                ));

                _tickEventBuffer.Add(new PlayerGoldChangedEvent(
                    Tick,
                    _state.PlayerGold,
                    _config.WaveCompletionBonusGold,
                    "WaveCompletion"
                ));
            }
        }

        private void HandleGameOver(bool isVictory)
        {
            _state.SetSessionPhase(MergeSessionPhase.GameOver);

            _tickEventBuffer.Add(new MergeGameOverEvent(
                Tick,
                isVictory,
                _state.Score,
                _state.MaxGrade
            ));
        }

        #endregion

        #region Helpers

        private int CalculateMergeScore(int resultGrade)
        {
            // 등급이 높을수록 더 많은 점수
            return resultGrade * _config.ScorePerGrade;
        }

        #endregion

        #region Public API

        /// <summary>
        /// 몬스터 경로를 설정합니다.
        /// </summary>
        public void SetMonsterPath(int pathIndex, MonsterPath path)
        {
            _state.SetMonsterPath(pathIndex, path);
        }

        /// <summary>
        /// 머지 이펙트를 등록합니다.
        /// </summary>
        public void RegisterMergeEffect(IMergeEffect effect)
        {
            _effectSystem.RegisterEffect(effect);
        }

        #endregion

        #region Module Management

        /// <summary>
        /// 모듈을 추가합니다.
        /// </summary>
        /// <typeparam name="TModule">모듈 타입</typeparam>
        /// <param name="module">모듈 인스턴스</param>
        public void AddModule<TModule>(TModule module) where TModule : IHostModule
        {
            if (module == null || _moduleMap.ContainsKey(module.ModuleId))
            {
                return;
            }

            _modules.Add(module);
            _moduleMap[module.ModuleId] = module;

            // 이미 초기화된 상태라면 즉시 초기화
            if (_modulesInitialized)
            {
                module.Initialize(this);
                module.Startup();
            }
        }

        /// <summary>
        /// 설정과 함께 모듈을 추가합니다.
        /// </summary>
        public void AddModule<TModule, TConfig>(TModule module, TConfig config)
            where TModule : IHostModule<TConfig>
            where TConfig : class
        {
            if (module == null || _moduleMap.ContainsKey(module.ModuleId))
            {
                return;
            }

            module.Configure(config);

            _modules.Add(module);
            _moduleMap[module.ModuleId] = module;

            if (_modulesInitialized)
            {
                module.Initialize(this);
                module.Startup();
            }
        }

        /// <summary>
        /// 모듈을 가져옵니다.
        /// </summary>
        public TModule GetModule<TModule>() where TModule : class, IHostModule
        {
            return _modules.OfType<TModule>().FirstOrDefault();
        }

        /// <summary>
        /// 모듈 ID로 모듈을 가져옵니다.
        /// </summary>
        public IHostModule GetModule(string moduleId)
        {
            return _moduleMap.TryGetValue(moduleId, out var module) ? module : null;
        }

        /// <summary>
        /// 모든 모듈을 초기화합니다.
        /// 게임 시작 전에 호출해야 합니다.
        /// </summary>
        public void InitializeModules()
        {
            if (_modulesInitialized)
            {
                return;
            }

            // 우선순위로 정렬 (높은 우선순위 먼저)
            _modules.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var module in _modules)
            {
                module.Initialize(this);
            }

            foreach (var module in _modules)
            {
                module.Startup();
            }

            _modulesInitialized = true;
        }

        /// <summary>
        /// 모든 모듈을 틱합니다.
        /// </summary>
        private void TickModules(float deltaTime)
        {
            foreach (var module in _modules)
            {
                if (module.IsInitialized)
                {
                    module.Tick(Tick, deltaTime);
                }
            }
        }

        /// <summary>
        /// 모든 모듈을 정리합니다.
        /// </summary>
        private void DisposeModules()
        {
            foreach (var module in _modules)
            {
                module.Shutdown();
                module.Dispose();
            }

            _modules.Clear();
            _moduleMap.Clear();
            _innerEventBus.Clear();
            _modulesInitialized = false;
        }

        #endregion

        #region Inner Event Handlers

        /// <summary>
        /// WaveModule에서 몬스터 스폰 요청 시 호출됩니다.
        /// </summary>
        private void OnMonsterSpawnRequest(MonsterSpawnRequestInnerEvent evt)
        {
            var pathIndex = evt.PathIndex;
            var path = _state.GetMonsterPath(pathIndex);
            if (path == null)
            {
                return;
            }

            var monster = _state.CreateMonster(
                evt.MonsterId,
                pathIndex,
                path.GetStartPosition(),
                damageToPlayer: 10,
                goldReward: 10 + evt.WaveNumber
            );

            // ASC 초기화 (웨이브에 따라 스케일링)
            var waveMultiplier = 1f + (evt.WaveNumber - 1) * 0.1f;
            monster.ASC.Set(AttributeId.MaxHealth, 100f * waveMultiplier);
            monster.ASC.Set(AttributeId.Health, 100f * waveMultiplier);
            monster.ASC.Set(AttributeId.MoveSpeed, 2f);

            PublishEvent(new MonsterSpawnedEvent(
                Tick,
                monster.Uid,
                monster.MonsterId,
                monster.PathIndex,
                monster.Position.X,
                monster.Position.Y,
                monster.ASC.Get(AttributeId.MaxHealth)
            ));

            evt.Handled = true;
        }

        #endregion

        public override void Dispose()
        {
            DisposeModules();
            _innerEventBus.Unsubscribe<MonsterSpawnRequestInnerEvent>(OnMonsterSpawnRequest);
            base.Dispose();
            _state?.Dispose();
        }
    }

    #region Definition Classes

    /// <summary>
    /// 캐릭터 정의 데이터입니다.
    /// </summary>
    public class CharacterDefinition
    {
        public string CharacterId { get; set; }
        public string CharacterType { get; set; }
        public int InitialGrade { get; set; } = 1;
        public float BaseAttackDamage { get; set; } = 10f;
        public float BaseAttackSpeed { get; set; } = 1f;
        public float BaseAttackRange { get; set; } = 5f;
        public string OnMergeSourceEffectId { get; set; }
        public string OnMergeTargetEffectId { get; set; }
    }

    #endregion
}
