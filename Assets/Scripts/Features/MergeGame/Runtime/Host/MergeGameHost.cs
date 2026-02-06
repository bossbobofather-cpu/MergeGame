using System;
using System.Collections.Generic;
using MyProject.MergeGame.AI;
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
        /// 캐릭터 정의/랜덤 선택을 제공하는 데이터베이스입니다.
        /// </summary>
        private readonly ITowerDatabase _towerDatabase;

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
        private readonly IMergeTowerAI _defaultTowerAI;
        private readonly IMergeMonsterAI _defaultMonsterAI;

        /// <summary>
        /// 스냅샷 빌드용 임시 리스트입니다.
        /// </summary>
        private readonly List<SlotSnapshot> _tempSlotSnapshots = new();
        private readonly List<TowerSnapshot> _tempTowerSnapshots = new();
        private readonly List<MonsterSnapshot> _tempMonsterSnapshots = new();

        /// <summary>
        /// 이벤트 버퍼입니다.
        /// </summary>
        private readonly List<MergeHostEvent> _tickEventBuffer = new();

        /// <summary>
        /// WaveModule 상태 동기화/이벤트 발행을 위한 이전 상태 캐시입니다.
        /// </summary>
        private int _lastWaveNumber;
        private WavePhase _lastWavePhase = WavePhase.Idle;

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

        public MergeGameHost(MergeHostConfig config, ITowerDatabase towerDatabase)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _towerDatabase = towerDatabase ?? throw new ArgumentNullException(nameof(towerDatabase));
            _state = new MergeHostState();

            _combatSystem = new MergeCombatSystem(_state, _config.DefaultAttackRange);
            _effectSystem = new MergeEffectSystem(_state);
            _defaultTowerAI = new TowerBaseAttackAI();
            _defaultMonsterAI = new MonsterPathMoveAI();

            // 내부 이벤트 구독 (모듈 -> Host 통신용)
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
                case SpawnTowerCommand spawnCharCmd:
                    return HandleSpawnTower(spawnCharCmd);

                case MergeTowerCommand mergeCharCmd:
                    return HandleMergeTower(mergeCharCmd);

                case MoveTowerCommand moveCharCmd:
                    return HandleMoveTower(moveCharCmd);

                case StartWaveCommand startWaveCmd:
                    return HandleStartWave(startWaveCmd);

                
                case InjectMonstersCommand injectCmd:
                    return HandleInjectMonsters(injectCmd);

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

            // WaveModule 상태 동기화 및 웨이브 이벤트/보상 처리
            SyncWaveStateAndEmitWaveEvents();

            // 1. 타워/몬스터 ASC 및 AI 틱
            foreach (var tower in _state.Towers.Values)
            {
                tower.ASC.Tick(deltaTime);
                tower.AI?.Tick(Tick, deltaTime, tower, _state, _combatSystem, _tickEventBuffer);
            }

            foreach (var monster in _state.Monsters.Values)
            {
                monster.ASC.Tick(deltaTime);
                monster.AI?.Tick(Tick, deltaTime, monster, _state, _tickEventBuffer);
            }

            // 2. 투사체 이동/충돌 처리
            _combatSystem.TickProjectiles(Tick, deltaTime, _tickEventBuffer);

            // 3. 사망한 몬스터 처리
            ProcessDeadMonsters();

            // 게임 오버 체크: 몬스터가 너무 많이 쌓이면 패배합니다.
            if (_config.MaxMonsterStack > 0 && _state.Monsters.Count >= _config.MaxMonsterStack)
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
            _tempTowerSnapshots.Clear();
            _tempMonsterSnapshots.Clear();

            for (var i = 0; i < _state.Slots.Count; i++)
            {
                var slot = _state.Slots[i];
                _tempSlotSnapshots.Add(new SlotSnapshot(
                    slot.Index,
                    slot.TowerUid,
                    slot.TowerGrade
                ));
            }

            foreach (var tower in _state.Towers.Values)
            {
                _tempTowerSnapshots.Add(new TowerSnapshot(
                    tower.Uid,
                    tower.TowerId,
                    tower.TowerType,
                    tower.Grade,
                    tower.SlotIndex,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z,
                    tower.ASC.Get(AttributeId.AttackDamage),
                    tower.ASC.Get(AttributeId.AttackSpeed),
                    tower.ASC.Get(AttributeId.AttackRange),
                    tower.AttackType,
                    tower.ProjectileType,
                    tower.ProjectileSpeed,
                    tower.ThrowRadius
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
                    monster.Position.Z,
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
                towers: _tempTowerSnapshots.ToArray(),
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
                slotPositions.Add(new SlotPositionData(slot.Index, slot.Position.X, slot.Position.Y, slot.Position.Z));
            }

            var pathDataList = new List<PathData>();
            foreach (var path in mapModule.Paths)
            {
                var waypoints = new List<PathWaypointData>();
                foreach (var wp in path.Waypoints)
                {
                    waypoints.Add(new PathWaypointData(wp.X, wp.Y, wp.Z));
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

            if (fromSlot.TowerGrade != toSlot.TowerGrade)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeUnitResult.Fail(Tick, command.SenderUid, "같은 등급의 유닛만 머지할 수 있습니다."));
            }

            // 머지 실행
            var sourceUid1 = fromSlot.TowerUid;
            var sourceUid2 = toSlot.TowerUid;
            var newGrade = fromSlot.TowerGrade + 1;
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

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleSpawnTower(SpawnTowerCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
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
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "빈 슬롯이 없습니다."));
            }

            var slot = _state.GetSlot(slotIndex);
            if (slot == null || !slot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "해당 슬롯이 비어있지 않습니다."));
            }

            // 캐릭터 정의 조회
            var definition = _towerDatabase.GetDefinition(command.TowerId);
            if (definition == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "캐릭터 정의를 찾을 수 없습니다."));
            }

            // 캐릭터 생성
            var tower = _state.CreateTower(
                definition.TowerId,
                definition.TowerType,
                definition.InitialGrade,
                slotIndex,
                slot.Position,
                definition.AttackType,
                definition.ProjectileType,
                definition.ProjectileSpeed,
                definition.ThrowRadius,
                definition.OnMergeSourceEffectId,
                definition.OnMergeTargetEffectId
            );

            // 전투 스탯/능력 초기화
            ConfigureTowerCombat(tower, definition, definition.InitialGrade);

            // 슬롯에 캐릭터 배치
            slot.SetUnit(tower.Uid, tower.Grade);

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, slotIndex, true));

            var events = new List<MergeHostEvent>
            {
                new TowerSpawnedEvent(
                    Tick,
                    tower.Uid,
                    tower.TowerId,
                    tower.TowerType,
                    tower.Grade,
                    slotIndex,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z
                )
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                SpawnTowerResult.Ok(
                    Tick,
                    command.SenderUid,
                    tower.Uid,
                    tower.TowerId,
                    tower.TowerType,
                    tower.Grade,
                    slotIndex
                ),
                events
            );
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleMergeTower(MergeTowerCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(command.FromSlotIndex);
            var toSlot = _state.GetSlot(command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            // 캐릭터 찾기
            var sourceChar = _state.GetTowerBySlot(command.FromSlotIndex);
            var targetChar = _state.GetTowerBySlot(command.ToSlotIndex);

            if (sourceChar == null || targetChar == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "캐릭터를 찾을 수 없습니다."));
            }

            // 머지 가능 여부 확인
            if (!sourceChar.CanMergeWith(targetChar))
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "같은 타입과 등급의 캐릭터만 머지할 수 있습니다."));
            }

            // 새 등급 계산
            var newGrade = sourceChar.Grade + 1;

            // 새 캐릭터 ID 결정 (덱에서 랜덤)
            var newTowerId = _towerDatabase.GetRandomIdForGrade(newGrade);
            if (string.IsNullOrEmpty(newTowerId))
            {
                newTowerId = sourceChar.TowerId;
            }

            var newDefinition = _towerDatabase.GetDefinition(newTowerId);
            // 결과 캐릭터 생성
            var resultChar = _state.CreateTower(
                newTowerId,
                newDefinition?.TowerType ?? sourceChar.TowerType,
                newGrade,
                command.ToSlotIndex,
                toSlot.Position,
                newDefinition?.AttackType ?? sourceChar.AttackType,
                newDefinition?.ProjectileType ?? sourceChar.ProjectileType,
                newDefinition?.ProjectileSpeed ?? sourceChar.ProjectileSpeed,
                newDefinition?.ThrowRadius ?? sourceChar.ThrowRadius,
                newDefinition?.OnMergeSourceEffectId,
                newDefinition?.OnMergeTargetEffectId
            );
            // 전투 스탯/능력 초기화
            var fallbackDefinition = newDefinition ?? CreateFallbackDefinition(sourceChar, newGrade);
            ConfigureTowerCombat(resultChar, fallbackDefinition, newGrade);

            // 머지 이펙트 적용
            var effectResult = _effectSystem.ApplyMergeEffects(Tick, sourceChar, targetChar, resultChar);

            // 골드 처리
            if (effectResult.BonusGold > 0)
            {
                _state.AddPlayerGold(effectResult.BonusGold);
            }

            // 소스 캐릭터 제거
            _state.RemoveTower(sourceChar.Uid);
            fromSlot.Clear();

            // 타겟 캐릭터 제거
            _state.RemoveTower(targetChar.Uid);

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
                new TowerMergedEvent(
                    Tick,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.TowerId,
                    resultChar.TowerType,
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
                MergeTowerResult.Ok(
                    Tick,
                    command.SenderUid,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.TowerId,
                    resultChar.TowerType,
                    resultChar.Grade,
                    command.ToSlotIndex
                ),
                events
            );
        }

        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleMoveTower(MoveTowerCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveTowerResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(command.FromSlotIndex);
            var toSlot = _state.GetSlot(command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveTowerResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            if (fromSlot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveTowerResult.Fail(Tick, command.SenderUid, "이동할 캐릭터가 없습니다."));
            }

            if (!toSlot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveTowerResult.Fail(Tick, command.SenderUid, "대상 슬롯이 비어있지 않습니다."));
            }

            var tower = _state.GetTowerBySlot(command.FromSlotIndex);
            if (tower == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    MoveTowerResult.Fail(Tick, command.SenderUid, "캐릭터를 찾을 수 없습니다."));
            }

            // 캐릭터 이동
            tower.SlotIndex = command.ToSlotIndex;
            tower.Position = toSlot.Position;

            // 슬롯 상태 업데이트
            toSlot.SetUnit(fromSlot.TowerUid, fromSlot.TowerGrade);
            fromSlot.Clear();

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.FromSlotIndex, false));
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.ToSlotIndex, true));

            var events = new List<MergeHostEvent>
            {
                new TowerMovedEvent(
                    Tick,
                    tower.Uid,
                    command.FromSlotIndex,
                    command.ToSlotIndex,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z
                )
            };

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                MoveTowerResult.Ok(
                    Tick,
                    command.SenderUid,
                    tower.Uid,
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

            var waveModule = GetModule<WaveModule>();
            if (waveModule == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartWaveResult.Fail(Tick, command.SenderUid, "WaveModule이 등록되지 않았습니다."));
            }

            // 현재 구현은 "다음 웨이브 시작"만 지원합니다.
            if (command.WaveNumber > 0 && command.WaveNumber != waveModule.CurrentWaveNumber + 1)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartWaveResult.Fail(Tick, command.SenderUid, "특정 웨이브 번호 시작은 아직 지원하지 않습니다."));
            }

            var started = waveModule.StartWave(Tick);
            if (!started)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    StartWaveResult.Fail(Tick, command.SenderUid, "웨이브 시작에 실패했습니다."));
            }

            // WaveModule이 응답하는 상태를 조회해서 Result에 포함합니다.
            var status = new GetWaveStatusRequest(Tick);
            _innerEventBus.Publish(status);

            // View/스냅샷용 상태 동기화
            _state.SetCurrentWaveNumber(status.CurrentWaveNumber);
            _state.SetWavePhase(status.Phase);

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                StartWaveResult.Ok(Tick, command.SenderUid, status.CurrentWaveNumber, status.TotalMonsters));
        }


        private GameCommandOutcome<MergeCommandResult, MergeHostEvent> HandleInjectMonsters(InjectMonstersCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    InjectMonstersResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            if (command.Count <= 0)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    InjectMonstersResult.Ok(Tick, command.SenderUid, 0));
            }

            var path = _state.GetMonsterPath(command.PathIndex);
            if (path == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                    InjectMonstersResult.Fail(Tick, command.SenderUid, "유효하지 않은 경로입니다."));
            }

            var monsterId = string.IsNullOrWhiteSpace(command.MonsterId) ? "monster_default" : command.MonsterId;

            var events = new List<MergeHostEvent>(command.Count);
            var spawned = 0;

            // 상대 공격(garbage) 몬스터는 우선 보상/플레이어 데미지를 주지 않는 형태로 주입합니다.
            for (var i = 0; i < command.Count; i++)
            {
                var monster = _state.CreateMonster(
                    monsterId,
                    command.PathIndex,
                    path.GetStartPosition(),
                    damageToPlayer: 0,
                    goldReward: 0);
            monster.SetAI(_defaultMonsterAI);

                // 최소 동작용 ASC 세팅 (추후 몬스터 정의/스케일링 정책으로 교체)
                monster.ASC.Set(AttributeId.MaxHealth, 100f);
                monster.ASC.Set(AttributeId.Health, 100f);
                monster.ASC.Set(AttributeId.MoveSpeed, 2f);

                events.Add(new MonsterSpawnedEvent(
                    Tick,
                    monster.Uid,
                    monster.MonsterId,
                    monster.PathIndex,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.Position.Z,
                    monster.ASC.Get(AttributeId.MaxHealth)
                ));

                spawned++;
            }

            return new GameCommandOutcome<MergeCommandResult, MergeHostEvent>(
                InjectMonstersResult.Ok(Tick, command.SenderUid, spawned),
                events);
        }
        #endregion

        #region Tick Processing
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
                    monster.Position.Z,
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

        private void SyncWaveStateAndEmitWaveEvents()
        {
            var waveModule = GetModule<WaveModule>();
            if (waveModule == null)
            {
                return;
            }

            var currentWaveNumber = waveModule.CurrentWaveNumber;
            var currentPhase = waveModule.CurrentPhase;

            // 스냅샷에서 조회할 수 있도록 HostState에 반영합니다.
            if (_state.CurrentWaveNumber != currentWaveNumber)
            {
                _state.SetCurrentWaveNumber(currentWaveNumber);
            }

            if (_state.CurrentWavePhase != currentPhase)
            {
                _state.SetWavePhase(currentPhase);
            }

            // 변화가 없으면 종료
            if (_lastWaveNumber == currentWaveNumber && _lastWavePhase == currentPhase)
            {
                return;
            }

            // 웨이브 시작
            if (currentPhase == WavePhase.Spawning && (_lastWaveNumber != currentWaveNumber || _lastWavePhase != WavePhase.Spawning))
            {
                var status = new GetWaveStatusRequest(Tick);
                _innerEventBus.Publish(status);

                _tickEventBuffer.Add(new WaveStartedEvent(
                    Tick,
                    status.CurrentWaveNumber,
                    status.TotalMonsters
                ));
            }

            // 웨이브 완료
            if (currentPhase == WavePhase.Completed && _lastWavePhase != WavePhase.Completed)
            {
                var bonusGold = _config.WaveCompletionBonusGold;

                if (bonusGold != 0)
                {
                    _state.AddPlayerGold(bonusGold);

                    _tickEventBuffer.Add(new PlayerGoldChangedEvent(
                        Tick,
                        _state.PlayerGold,
                        bonusGold,
                        "WaveCompletion"
                    ));
                }

                _tickEventBuffer.Add(new WaveCompletedEvent(
                    Tick,
                    currentWaveNumber,
                    bonusGold
                ));
            }

            _lastWaveNumber = currentWaveNumber;
            _lastWavePhase = currentPhase;
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

        /// <summary>
        /// 타워 전투 스탯과 기본 공격 능력을 초기화합니다.
        /// </summary>
        private void ConfigureTowerCombat(MergeTower tower, TowerDefinition definition, int grade)
        {
            if (tower == null || definition == null)
            {
                return;
            }

            var gradeMultiplier = 1f + (grade - 1) * 0.2f;

            tower.ASC.Set(AttributeId.AttackDamage, definition.BaseAttackDamage * gradeMultiplier);
            tower.ASC.Set(AttributeId.AttackSpeed, definition.BaseAttackSpeed);
            tower.ASC.Set(AttributeId.AttackRange, definition.BaseAttackRange);

            var ability = BuildBaseAttackAbility(definition, definition.BaseAttackRange);
            tower.ASC.GiveAbility(ability);
            tower.SetAI(_defaultTowerAI);
        }

        private GameplayAbility BuildBaseAttackAbility(TowerDefinition definition, float range)
        {
            var ability = new GameplayAbility
            {
                AbilityTag = new FGameplayTag("Ability.BaseAttack"),
                DisplayName = "BaseAttack",
                CooldownEffect = new GameplayEffect
                {
                    DurationType = EffectDurationType.HasDuration,
                    Duration = 1f,
                    Period = 0f,
                    GrantedTags = BuildTagContainer("Cooldown.BaseAttack"),
                    DurationPolicy = BaseAttackCooldownDurationPlicy.Instance
                },
                ActivationBlockedTags = BuildTagContainer("Cooldown.BaseAttack"),
                TargetingStrategy = CreateTargetingStrategy(definition.TargetingType, range)
            };

            return ability;
        }

        private static GameplayTagContainer BuildTagContainer(string tag)
        {
            var container = new GameplayTagContainer();
            container.AddTag(new FGameplayTag(tag));
            return container;
        }

        private static ITargetingStrategy CreateTargetingStrategy(TowerTargetingType targetingType, float range)
        {
            return targetingType switch
            {
                TowerTargetingType.Nearest => TargetingStrategyFactory.Create(TargetingStrategyType.NearestEnemy, maxRange: range),
                TowerTargetingType.Random => TargetingStrategyFactory.Create(TargetingStrategyType.Random, maxRange: range),
                TowerTargetingType.LowestHp => TargetingStrategyFactory.Create(TargetingStrategyType.LowestHp, maxRange: range),
                TowerTargetingType.Area => TargetingStrategyFactory.Create(TargetingStrategyType.Area, radius: range),
                _ => TargetingStrategyFactory.Create(TargetingStrategyType.NearestEnemy, maxRange: range)
            };
        }

        private static TowerDefinition CreateFallbackDefinition(MergeTower source, int grade)
        {
            if (source == null)
            {
                return null;
            }

            var gradeMultiplier = 1f + (grade - 1) * 0.2f;

            return new TowerDefinition
            {
                TowerId = source.TowerId,
                TowerType = source.TowerType,
                InitialGrade = grade,
                BaseAttackDamage = source.ASC.Get(AttributeId.AttackDamage) / gradeMultiplier,
                BaseAttackSpeed = source.ASC.Get(AttributeId.AttackSpeed),
                BaseAttackRange = source.ASC.Get(AttributeId.AttackRange),
                AttackType = source.AttackType,
                ProjectileType = source.ProjectileType,
                ProjectileSpeed = source.ProjectileSpeed,
                ThrowRadius = source.ThrowRadius,
                TargetingType = TowerTargetingType.Nearest
            };
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
            if (module == null)
            {
                return;
            }

            if (_modulesInitialized)
            {
                GameHostLog.LogError($"[{GetType().Name}] 모듈이 이미 초기화된 상태에서는 AddModule을 호출할 수 없습니다. ({module.ModuleId})");
                return;
            }

            if (_moduleMap.ContainsKey(module.ModuleId))
            {
                return;
            }

            _modules.Add(module);
            _moduleMap[module.ModuleId] = module;
        }

        /// <summary>
        /// 설정과 함께 모듈을 추가합니다.
        /// </summary>
        public void AddModule<TModule, TConfig>(TModule module, TConfig config)
            where TModule : IHostModule<TConfig>
            where TConfig : class
        {
            if (module == null)
            {
                return;
            }

            if (_modulesInitialized)
            {
                GameHostLog.LogError($"[{GetType().Name}] 모듈이 이미 초기화된 상태에서는 AddModule을 호출할 수 없습니다. ({module.ModuleId})");
                return;
            }

            if (_moduleMap.ContainsKey(module.ModuleId))
            {
                return;
            }

            module.Configure(config);

            _modules.Add(module);
            _moduleMap[module.ModuleId] = module;
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
            monster.SetAI(_defaultMonsterAI);

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
                    monster.Position.Z,
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

}































