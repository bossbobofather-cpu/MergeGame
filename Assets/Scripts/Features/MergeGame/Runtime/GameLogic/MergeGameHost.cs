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
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 전용 호스트입니다.
    /// Command를 처리하고 Result/Event를 발행합니다.
    /// IHostContext를 구현하여 모듈들에게 컨텍스트를 제공합니다.
    /// </summary>
    public sealed class MergeGameHost
        : GameHostBase<MergeGameCommand, MergeCommandResult, MergeGameEvent, MergeHostSnapshot>,
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
        private readonly List<ProjectileSnapshot> _tempProjectileSnapshots = new();

        /// <summary>
        /// 이벤트 버퍼입니다.
        /// </summary>
        private readonly List<MergeGameEvent> _tickEventBuffer = new();

        /// <summary>
        /// 플레이어 수입니다.
        /// </summary>
        private int _playerCount;

        /// <summary>
        /// 플레이어별 스냅샷 캐시입니다 (Host 스레드에서 빌드, 외부에서 읽기).
        /// </summary>
        private volatile MergeHostSnapshot[] _playerSnapshots;

        /// <summary>
        /// SenderUid -> PlayerIndex 매핑입니다.
        /// </summary>
        private readonly Dictionary<long, int> _playerIndexByUid = new();
        private readonly Random _random = new();

        private int[] _killCountForOpponentInjection = Array.Empty<int>();
        private const int KillCountThresholdForOpponentInjection = 3;
        private const float BaseMonsterMaxHealth = 60f;

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

        long IHostContext.CurrentTick => Tick;
        int IHostContext.PlayerCount => _playerCount;
        IInnerEventBus IHostContext.InnerEventBus => _innerEventBus;

        void IHostContext.PublishEvent<TEvent>(TEvent eventData)
        {
            if (eventData is MergeGameEvent mergeEvent)
            {
                PublishEvent(mergeEvent);
            }
        }

        TModule IHostContext.GetModule<TModule>()
        {
            return _modules.OfType<TModule>().FirstOrDefault();
        }

        IHostModule IHostContext.GetModule(string moduleId)
        {
            return _moduleMap.TryGetValue(moduleId, out var module) ? module : null;
        }

        #endregion
        /// <summary>
        /// MergeGameHost 메서드입니다.
        /// </summary>

        public MergeGameHost(MergeHostConfig config, ITowerDatabase towerDatabase)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _towerDatabase = towerDatabase ?? throw new ArgumentNullException(nameof(towerDatabase));
            _state = new MergeHostState();

            _combatSystem = new MergeCombatSystem(_state);
            _effectSystem = new MergeEffectSystem();
            _defaultTowerAI = new TowerBaseAttackAI();
            _defaultMonsterAI = new MonsterPathMoveAI();

            // 내부 이벤트 구독 (모듈 -> Host 통신용)
            _innerEventBus.Subscribe<MonsterSpawnRequestInnerEvent>(OnMonsterSpawnRequest);
        }

        /// <summary>
        /// 플레이어 수를 초기화하고 상태를 준비합니다.
        /// </summary>
        public void InitializePlayers(int playerCount)
        {
            _playerCount = playerCount;
            _state.InitializePlayers(playerCount);
            _killCountForOpponentInjection = new int[playerCount];
        }

        /// <summary>
        /// 플레이어 UID를 인덱스에 등록합니다.
        /// </summary>
        public void RegisterPlayer(long uid, int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _playerCount)
            {
                return;
            }

            _playerIndexByUid[uid] = playerIndex;
        }

        #region GameHostBase Overrides
        /// <summary>
        /// HandleCommand 메서드입니다.
        /// </summary>

        protected override GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleCommand(MergeGameCommand command)
        {
            if (!_playerIndexByUid.TryGetValue(command.SenderUid, out var playerIndex))
            {
                return default;
            }

            switch (command)
            {
                case ReadyMergeGameCommand readyCommand:
                    return HandleReadyGame(readyCommand);

                case ExitMergeGameCommand endCommand:
                    return HandleEndGame(endCommand);

                case SpawnTowerCommand spawnCharCmd:
                    return HandleSpawnTower(playerIndex, spawnCharCmd);

                case MergeTowerCommand mergeCharCmd:
                    return HandleMergeTower(playerIndex, mergeCharCmd);

                case InjectMonstersCommand injectCmd:
                    return HandleInjectMonsters(playerIndex, injectCmd);

                default:
                    return default;
            }
        }
        /// <summary>
        /// OnTick 메서드입니다.
        /// </summary>

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
            for (int i = 0; i < _playerCount; i++)
            {
                if (_state.IsPlayerGameOver(i))
                {
                    continue;
                }

                // 1. 타워/몬스터 ASC 및 AI 틱
                foreach (var tower in _state.GetTowers(i).Values)
                {
                    tower.ASC.Tick(deltaTime);
                    tower.AI?.Tick(Tick, i, deltaTime, tower, _state, _combatSystem, _tickEventBuffer);
                }

                foreach (var monster in _state.GetMonsters(i).Values)
                {
                    monster.ASC.Tick(deltaTime);
                    monster.AI?.Tick(Tick, i, deltaTime, monster, _state, _tickEventBuffer);
                }

                // 2. 투사체 이동/충돌 처리
                _combatSystem.TickProjectiles(Tick, i, deltaTime, _tickEventBuffer);

                // 3. 사망한 몬스터 처리
                ProcessDeadMonsters(i);

                // 게임 오버 체크: 몬스터가 너무 많이 쌓이면 패배합니다.
                bool isGameOver = false;
                if (_config.MaxMonsterStack > 0 && _state.GetAliveMonsterCount(i) >= _config.MaxMonsterStack)
                {
                    isGameOver = true;
                }

                if (isGameOver)
                {
                    HandleGameOver(i, false);
                }
            }

            // 이벤트 발행
            foreach (var evt in _tickEventBuffer)
            {
                PublishEvent(evt);
            }
        }
        /// <summary>
        /// BuildSnapshotInternal 메서드입니다.
        /// </summary>

        protected override MergeHostSnapshot BuildSnapshotInternal()
        {
            var snapshots = new MergeHostSnapshot[_playerCount];
            for (int i = 0; i < _playerCount; i++)
            {
                snapshots[i] = BuildPlayerSnapshot(i);
            }
            _playerSnapshots = snapshots;
            return _playerCount > 0 ? snapshots[0] : null;
        }

        /// <summary>
        /// 플레이어별 최신 스냅샷을 반환합니다 (thread-safe).
        /// </summary>
        public MergeHostSnapshot GetPlayerSnapshot(int playerIndex)
        {
            var snapshots = _playerSnapshots;
            if (snapshots == null || playerIndex < 0 || playerIndex >= snapshots.Length)
                return null;
            return snapshots[playerIndex];
        }
        /// <summary>
        /// BuildPlayerSnapshot 메서드입니다.
        /// </summary>

        private MergeHostSnapshot BuildPlayerSnapshot(int playerIndex)
        {
            _tempSlotSnapshots.Clear();
            _tempTowerSnapshots.Clear();
            _tempMonsterSnapshots.Clear();
            _tempProjectileSnapshots.Clear();

            var slots = _state.GetSlots(playerIndex);
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                _tempSlotSnapshots.Add(new SlotSnapshot(
                    slot.Index,
                    slot.TowerUid,
                    slot.TowerGrade
                ));
            }

            foreach (var tower in _state.GetTowers(playerIndex).Values)
            {
                _tempTowerSnapshots.Add(new TowerSnapshot(
                    tower.Uid,
                    tower.TowerId,
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

            foreach (var monster in _state.GetMonsters(playerIndex).Values)
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
                    monster.ASC.Get(AttributeId.MaxHealth),
                    monster.IsInjectedByOpponent
                ));
            }

            // 투사체
            var activeProjectiles = _combatSystem.GetActiveProjectiles(playerIndex);
            for (var i = 0; i < activeProjectiles.Count; i++)
            {
                var p = activeProjectiles[i];
                _tempProjectileSnapshots.Add(new ProjectileSnapshot(
                    p.Uid,
                    p.Start.X, p.Start.Y, p.Start.Z,
                    p.Impact.X, p.Impact.Y, p.Impact.Z,
                    p.Progress,
                    p.ProjectileType,
                    p.IsLanded
                ));
            }

            var difficulty = GetModule<DifficultyModule>();

            return new MergeHostSnapshot(
                tick: Tick,
                playerIndex: playerIndex,
                sessionPhase: _state.SessionPhase,
                score: _state.GetScore(playerIndex),
                maxGrade: _state.GetMaxGrade(playerIndex),
                totalSlots: slots.Count,
                usedSlots: _state.GetUsedSlotCount(playerIndex),
                elapsedTime: _state.ElapsedTime,
                slots: _tempSlotSnapshots.ToArray(),
                playerGold: _state.GetPlayerGold(playerIndex),
                difficultyStep: difficulty == null ? 0 : difficulty.CurrentStep,
                spawnCount: difficulty == null ? 0 : difficulty.SpawnCount,
                healthMultiplier: difficulty == null ? 1f : difficulty.HealthMultiplier,
                spawnInterval: difficulty == null ? 1f : difficulty.SpawnInterval,
                maxMonsterStack: _config.MaxMonsterStack,
                towers: _tempTowerSnapshots.ToArray(),
                monsters: _tempMonsterSnapshots.ToArray(),
                projectiles: _tempProjectileSnapshots.ToArray()
            );
        }

        #endregion

        #region Command Handlers - Legacy
        /// <summary>
        /// HandleReadyGame 메서드입니다.
        /// </summary>

        private GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleReadyGame(ReadyMergeGameCommand command)
        {
            if (!_playerIndexByUid.TryGetValue(command.SenderUid, out var playerIndex))
            {
                return default;
            }

            if (_state.SessionPhase == MergeSessionPhase.GameOver)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    ReadyMergeGameResult.Fail(Tick, command.SenderUid, "게임이 이미 종료되어 있습니다."));
            }

            // MapModule에서 슬롯 데이터를 가져와서 State 초기화
            var mapModule = GetModule<MapModule>();
            if (mapModule == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    ReadyMergeGameResult.Fail(Tick, command.SenderUid, "MapModule이 등록되지 않았습니다."));
            }

            var events = new List<MergeGameEvent>();
            var initializedSlots = _state.GetSlots(playerIndex);
            var needsPlayerInit = initializedSlots == null || initializedSlots.Count == 0;

            // 플레이어별 초기화는 한번만 수행합니다.
            if (needsPlayerInit)
            {
                _state.InitializeSlots(playerIndex, mapModule.Slots);
                _state.SetPlayerGold(playerIndex, _config.PlayerStartGold);

                // MapModule에서 경로를 State에 등록
                foreach (var mapPath in mapModule.Paths)
                {
                    var monsterPath = new Models.MonsterPath(mapPath.PathIndex, mapPath.Waypoints);
                    _state.SetMonsterPath(playerIndex, mapPath.PathIndex, monsterPath);
                }

                // MapInitializedEvent 생성 (View용)
                var slotPositions = new List<SlotPositionData>();
                foreach (var slot in _state.GetSlots(playerIndex))
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

                events.Add(new MapInitializedEvent(Tick, playerIndex, mapModule.MapId, slotPositions, pathDataList));
            }

            // 세션 시작은 최초 1회만 수행합니다.
            if (_state.SessionPhase == MergeSessionPhase.None)
            {
                _state.SetSessionPhase(MergeSessionPhase.Playing);
                for (var i = 0; i < _playerCount; i++)
                {
                    events.Add(new GameStartedEvent(Tick, i));
                }
            }

            return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                ReadyMergeGameResult.Ok(Tick, command.SenderUid),
                events);
        }
        /// <summary>
        /// HandleEndGame 메서드입니다.
        /// </summary>

        private GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleEndGame(ExitMergeGameCommand command)
        {
            if (!_playerIndexByUid.TryGetValue(command.SenderUid, out var playerIndex))
            {
                return default;
            }

            if (_state.SessionPhase == MergeSessionPhase.None ||
                _state.SessionPhase == MergeSessionPhase.GameOver)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    ExitMergeGameResult.Fail(Tick, command.SenderUid, "게임이 시작되지 않았습니다."));
            }

            _state.SetPlayerGameOver(playerIndex, true);

            var events = new List<MergeGameEvent>
            {
                new GameOverEvent(Tick, playerIndex, true, _state.GetScore(playerIndex), _state.GetMaxGrade(playerIndex))
            };

            CheckGlobalGameOver();

            return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                ExitMergeGameResult.Ok(Tick, command.SenderUid),
                events);
        }

        #endregion

        #region Command Handlers
        /// <summary>
        /// HandleSpawnTower 메서드입니다.
        /// </summary>

        private GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleSpawnTower(int playerIndex, SpawnTowerCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var slotIndex = _state.FindEmptySlotIndex(playerIndex);
            if (slotIndex < 0)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "빈 슬롯이 없습니다."));
            }

            var slot = _state.GetSlot(playerIndex, slotIndex);
            if (slot == null || !slot.IsEmpty)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "해당 슬롯이 비어있지 않습니다."));
            }

            // 캐릭터 정의 조회 (3종 중 랜덤)
            var towerId = _towerDatabase.GetRandomIdForGrade(_config.InitialTowerGrade);
            var definition = _towerDatabase.GetDefinition(towerId);
            if (definition == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    SpawnTowerResult.Fail(Tick, command.SenderUid, "캐릭터 정의를 찾을 수 없습니다."));
            }

            // 캐릭터 생성
            var tower = _state.CreateTower(
                playerIndex,
                definition.TowerId,
                definition.InitialGrade,
                slotIndex,
                slot.Position,
                definition.AttackType,
                definition.ProjectileType,
                definition.ProjectileSpeed,
                definition.ThrowRadius,
                definition.TrapDelay,
                definition.OnMergeSourceEffects,
                definition.OnMergeTargetEffects
            );

            // 전투 스탯/능력 초기화
            ConfigureTowerCombat(tower, definition, definition.InitialGrade);

            // 슬롯에 타워 배치
            slot.SetTower(tower.Uid, tower.Grade);

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, slotIndex, true));

            var events = new List<MergeGameEvent>
            {
                new TowerSpawnedEvent(
                    Tick,
                    playerIndex,
                    tower.Uid,
                    tower.TowerId,
                    tower.Grade,
                    slotIndex,
                    slot.Position.X,
                    slot.Position.Y,
                    slot.Position.Z
                )
            };

            return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                SpawnTowerResult.Ok(
                    Tick,
                    command.SenderUid,
                    tower.Uid,
                    tower.TowerId,
                    tower.Grade,
                    slotIndex,
                    slot.Position.X,
                    slot.Position.Y,
                    slot.Position.Z
                ),
                events
            );
        }
        /// <summary>
        /// HandleMergeTower 메서드입니다.
        /// </summary>

        private GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleMergeTower(int playerIndex, MergeTowerCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            var fromSlot = _state.GetSlot(playerIndex, command.FromSlotIndex);
            var toSlot = _state.GetSlot(playerIndex, command.ToSlotIndex);

            if (fromSlot == null || toSlot == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "잘못된 슬롯 인덱스입니다."));
            }

            // 캐릭터 찾기
            var sourceChar = _state.GetTowerAtSlot(playerIndex, command.FromSlotIndex);
            var targetChar = _state.GetTowerAtSlot(playerIndex, command.ToSlotIndex);

            if (sourceChar == null || targetChar == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "캐릭터를 찾을 수 없습니다."));
            }

            // 머지 가능 여부 확인
            if (!sourceChar.CanMergeWith(targetChar))
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    MergeTowerResult.Fail(Tick, command.SenderUid, "같은 타입과 등급의 캐릭터만 머지할 수 있습니다. 최대 등급 타워는 머지할 수 없습니다."));
            }

            // 새 등급 계산
            var newGrade = sourceChar.Grade + 1;

            // 새 캐릭터 ID 결정 (덱에서 랜덤)
            var newTowerId = _towerDatabase.GetRandomIdForGrade(newGrade);
            var newDefinition = _towerDatabase.GetDefinition(newTowerId);
            // 결과 캐릭터 생성
            var resultChar = _state.CreateTower(
                playerIndex,
                newTowerId,
                newGrade,
                command.ToSlotIndex,
                toSlot.Position,
                newDefinition?.AttackType ?? sourceChar.AttackType,
                newDefinition?.ProjectileType ?? sourceChar.ProjectileType,
                newDefinition?.ProjectileSpeed ?? sourceChar.ProjectileSpeed,
                newDefinition?.ThrowRadius ?? sourceChar.ThrowRadius,
                newDefinition?.TrapDelay ?? sourceChar.TrapDelay,
                newDefinition?.OnMergeSourceEffects,
                newDefinition?.OnMergeTargetEffects
            );
            // 전투 스탯/능력 초기화
            var fallbackDefinition = newDefinition ?? CreateFallbackDefinition(sourceChar, newGrade);
            ConfigureTowerCombat(resultChar, fallbackDefinition, newGrade);

            // 머지 이펙트 적용
            var effectResult = _effectSystem.ApplyMergeEffects(Tick, playerIndex, sourceChar, targetChar, resultChar);

            // 골드 처리
            if (effectResult.BonusGold > 0)
            {
                _state.AddPlayerGold(playerIndex, effectResult.BonusGold);
            }

            // 소스 캐릭터 제거
            _state.RemoveTower(playerIndex, sourceChar.Uid);
            fromSlot.Clear();

            // 타겟 캐릭터 제거
            _state.RemoveTower(playerIndex, targetChar.Uid);

            // 결과 캐릭터 슬롯에 배치
            toSlot.SetTower(resultChar.Uid, resultChar.Grade);

            // 슬롯 상태 변경 알림 (내부 - MapModule용)
            _innerEventBus.Publish(new SlotStateChangedInnerEvent(Tick, command.FromSlotIndex, false));
            // toSlot은 여전히 점유 상태이므로 별도 알림 불필요

            // 점수 추가
            var scoreGained = CalculateMergeScore(newGrade);
            _state.AddScore(playerIndex, scoreGained);
            _state.UpdateMaxGrade(playerIndex, newGrade);

            var events = new List<MergeGameEvent>
            {
                new TowerMergedEvent(
                    Tick,
                    playerIndex,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.TowerId,
                    resultChar.Grade,
                    command.ToSlotIndex
                ),
                new ScoreChangedEvent(Tick, playerIndex, _state.GetScore(playerIndex), scoreGained)
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
                    playerIndex,
                    _state.GetPlayerGold(playerIndex),
                    effectResult.BonusGold
                ));
            }

            return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                MergeTowerResult.Ok(
                    Tick,
                    command.SenderUid,
                    sourceChar.Uid,
                    targetChar.Uid,
                    resultChar.Uid,
                    resultChar.TowerId,
                    resultChar.Grade,
                    command.ToSlotIndex
                ),
                events
            );
        }
        /// <summary>
        /// HandleInjectMonsters 메서드입니다.
        /// </summary>

        private GameCommandOutcome<MergeCommandResult, MergeGameEvent> HandleInjectMonsters(int playerIndex, InjectMonstersCommand command)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    InjectMonstersResult.Fail(Tick, command.SenderUid, "게임이 진행 중이 아닙니다."));
            }

            if (command.Count <= 0)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    InjectMonstersResult.Ok(Tick, command.SenderUid, 0));
            }

            // 대상 플레이어 결정
            var targetPlayerIndex = command.TargetPlayerIndex;
            if (targetPlayerIndex < 0)
            {
                // 대상이 지정되지 않은 경우, 다음 플레이어를 공격 대상으로 설정 (Round-Robin)
                // 플레이어가 1명이면 자기 자신
                targetPlayerIndex = _playerCount > 1 ? (playerIndex + 1) % _playerCount : playerIndex;
            }

            if (targetPlayerIndex < 0 || targetPlayerIndex >= _playerCount)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    InjectMonstersResult.Fail(Tick, command.SenderUid, $"유효하지 않은 대상 플레이어 인덱스입니다: {targetPlayerIndex}"));
            }

            var path = _state.GetMonsterPath(targetPlayerIndex, command.PathIndex);
            if (path == null)
            {
                return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                    InjectMonstersResult.Fail(Tick, command.SenderUid, "유효하지 않은 경로입니다."));
            }

            var targetSpawnPath = _state.GetMonsterPath(targetPlayerIndex, 0) ?? path;
            var targetSpawnPosition = targetSpawnPath.GetStartPosition();

            var monsterId = command.MonsterId;

            var events = new List<MergeGameEvent>(command.Count);
            var spawned = 0;

            // 주입 몬스터는 현재 난이도 스펙으로 생성합니다.
            var difficultyStep = GetCurrentDifficultyStep();
            var difficultyMultiplier = 1f + (difficultyStep - 1) * 0.1f;

            for (var i = 0; i < command.Count; i++)
            {
                var monster = _state.CreateMonster(
                    targetPlayerIndex,
                    monsterId,
                    command.PathIndex,
                    path.GetStartPosition(),
                    damageToPlayer: 10,
                    goldReward: 10 + difficultyStep,
                    isInjectedByOpponent: true);
                monster.SetAI(_defaultMonsterAI);

                monster.ASC.Set(AttributeId.MaxHealth, BaseMonsterMaxHealth * difficultyMultiplier);
                monster.ASC.Set(AttributeId.Health, BaseMonsterMaxHealth * difficultyMultiplier);
                monster.ASC.Set(AttributeId.MoveSpeed, 2f);

                events.Add(CreateMonsterSpawnedEvent(targetPlayerIndex, monster));

                spawned++;
            }

            if (spawned > 0)
            {
                events.Add(CreateMonsterInjectionTriggeredEvent(
                    sourcePlayerIndex: playerIndex,
                    targetPlayerIndex: targetPlayerIndex,
                    monsterId: monsterId,
                    injectedCount: spawned,
                    sourceMonsterUid: 0,
                    hasSourcePosition: false,
                    sourceX: 0f,
                    sourceY: 0f,
                    sourceZ: 0f,
                    targetSpawnX: targetSpawnPosition.X,
                    targetSpawnY: targetSpawnPosition.Y,
                    targetSpawnZ: targetSpawnPosition.Z));
            }

            return new GameCommandOutcome<MergeCommandResult, MergeGameEvent>(
                InjectMonstersResult.Ok(Tick, command.SenderUid, spawned),
                events);
        }
        #endregion

        #region Tick Processing
        /// <summary>
        /// ProcessDeadMonsters 메서드입니다.
        /// </summary>
        private void ProcessDeadMonsters(int playerIndex)
        {
            var deadMonsterUids = new List<long>();

            foreach (var monster in _state.GetMonsters(playerIndex).Values)
            {
                if (!monster.IsAlive)
                {
                    deadMonsterUids.Add(monster.Uid);
                }
            }

            foreach (var uid in deadMonsterUids)
            {
                var monster = _state.GetMonster(playerIndex, uid);
                if (monster == null)
                {
                    continue;
                }

                // 골드 보상
                if (monster.GoldReward > 0)
                {
                    _state.AddPlayerGold(playerIndex, monster.GoldReward);

                    _tickEventBuffer.Add(new PlayerGoldChangedEvent(
                        Tick,
                        playerIndex,
                        _state.GetPlayerGold(playerIndex),
                        monster.GoldReward
                    ));
                }

                // 사망 이벤트 (외부 - View/Client용)
                _tickEventBuffer.Add(new MonsterDiedEvent(
                    Tick,
                    playerIndex,
                    monster.Uid,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.Position.Z,
                    monster.GoldReward,
                    0
                ));

                // 사망 알림 (내부 - DifficultyModule용)
                _innerEventBus.Publish(new MonsterDiedInnerEvent(
                    Tick,
                    playerIndex,
                    monster.Uid
                ));

                TryInjectOpponentMonsterByKill(playerIndex, monster.Uid, monster.MonsterId, monster.PathIndex, monster.Position.X, monster.Position.Y, monster.Position.Z);

                _state.RemoveMonster(playerIndex, uid);
            }
        }
        /// <summary>
        /// HandleGameOver 메서드입니다.
        /// </summary>

        private void HandleGameOver(int playerIndex, bool isVictory)
        {
            if (_state.IsPlayerGameOver(playerIndex))
            {
                return;
            }

            _state.SetPlayerGameOver(playerIndex, true);

            _tickEventBuffer.Add(new GameOverEvent(
                Tick,
                playerIndex,
                isVictory,
                _state.GetScore(playerIndex),
                _state.GetMaxGrade(playerIndex)
            ));

            CheckGlobalGameOver();
        }
        /// <summary>
        /// CheckGlobalGameOver 메서드입니다.
        /// </summary>

        private void CheckGlobalGameOver()
        {
            int activePlayerCount = 0;
            int lastActivePlayerIndex = -1;

            for (int i = 0; i < _playerCount; i++)
            {
                if (!_state.IsPlayerGameOver(i))
                {
                    activePlayerCount++;
                    lastActivePlayerIndex = i;
                }
            }

            if (activePlayerCount == 0)
            {
                _state.SetSessionPhase(MergeSessionPhase.GameOver);
            }
            else if (_playerCount > 1 && activePlayerCount == 1)
            {
                HandleGameOver(lastActivePlayerIndex, true);
            }
        }

        #endregion

        #region Helpers
        /// <summary>
        /// CalculateMergeScore 메서드입니다.
        /// </summary>

        private int CalculateMergeScore(int resultGrade)
        {
            // 등급이 높을수록 더 많은 점수
            return resultGrade * _config.ScorePerGrade;
        }
        /// <summary>
        /// GetCurrentDifficultyStep 메서드입니다.
        /// </summary>

        private int GetCurrentDifficultyStep()
        {
            var difficulty = GetModule<DifficultyModule>();
            return difficulty?.CurrentStep ?? 0;
        }
        /// <summary>
        /// CreateMonsterSpawnedEvent 메서드입니다.
        /// </summary>

        private MonsterSpawnedEvent CreateMonsterSpawnedEvent(int playerIndex, MergeMonster monster)
        {
            return new MonsterSpawnedEvent(
                Tick,
                playerIndex,
                monster.Uid,
                monster.MonsterId,
                monster.PathIndex,
                monster.Position.X,
                monster.Position.Y,
                monster.Position.Z,
                monster.ASC.Get(AttributeId.MaxHealth)
            );
        }

        private MonsterInjectionTriggeredEvent CreateMonsterInjectionTriggeredEvent(
            int sourcePlayerIndex,
            int targetPlayerIndex,
            long monsterId,
            int injectedCount,
            long sourceMonsterUid,
            bool hasSourcePosition,
            float sourceX,
            float sourceY,
            float sourceZ,
            float targetSpawnX,
            float targetSpawnY,
            float targetSpawnZ)
        {
            return new MonsterInjectionTriggeredEvent(
                Tick,
                sourcePlayerIndex,
                targetPlayerIndex,
                monsterId,
                injectedCount,
                sourceMonsterUid,
                hasSourcePosition,
                sourceX,
                sourceY,
                sourceZ,
                targetSpawnX,
                targetSpawnY,
                targetSpawnZ);
        }
        /// <summary>
        /// TryInjectOpponentMonsterByKill 메서드입니다.
        /// </summary>

        private void TryInjectOpponentMonsterByKill(int killerPlayerIndex, long sourceMonsterUid, long monsterId, int preferredPathIndex, float sourceX, float sourceY, float sourceZ)
        {
            if (_state.SessionPhase != MergeSessionPhase.Playing)
            {
                return;
            }

            if (_playerCount <= 1)
            {
                return;
            }

            if (killerPlayerIndex < 0 || killerPlayerIndex >= _killCountForOpponentInjection.Length)
            {
                return;
            }

            if (_state.IsPlayerGameOver(killerPlayerIndex))
            {
                return;
            }

            _killCountForOpponentInjection[killerPlayerIndex]++;

            while (_killCountForOpponentInjection[killerPlayerIndex] >= KillCountThresholdForOpponentInjection)
            {
                if (!TryPickRandomOpponentPlayerIndex(killerPlayerIndex, out var targetPlayerIndex))
                {
                    return;
                }

                var path = _state.GetMonsterPath(targetPlayerIndex, preferredPathIndex)
                           ?? _state.GetMonsterPath(targetPlayerIndex, 0);
                if (path == null)
                {
                    return;
                }

                var targetSpawnPath = _state.GetMonsterPath(targetPlayerIndex, 0) ?? path;
                var targetSpawnPosition = targetSpawnPath.GetStartPosition();

                var difficultyStep = GetCurrentDifficultyStep();
                var difficultyMultiplier = 1f + (difficultyStep - 1) * 0.1f;

                var injectedMonster = _state.CreateMonster(
                    targetPlayerIndex,
                    monsterId,
                    path.PathIndex,
                    path.GetStartPosition(),
                    damageToPlayer: 10,
                    goldReward: 10 + difficultyStep,
                    isInjectedByOpponent: true);
                injectedMonster.SetAI(_defaultMonsterAI);

                injectedMonster.ASC.Set(AttributeId.MaxHealth, BaseMonsterMaxHealth * difficultyMultiplier);
                injectedMonster.ASC.Set(AttributeId.Health, BaseMonsterMaxHealth * difficultyMultiplier);
                injectedMonster.ASC.Set(AttributeId.MoveSpeed, 2f);

                _tickEventBuffer.Add(CreateMonsterSpawnedEvent(targetPlayerIndex, injectedMonster));
                _tickEventBuffer.Add(CreateMonsterInjectionTriggeredEvent(
                    sourcePlayerIndex: killerPlayerIndex,
                    targetPlayerIndex: targetPlayerIndex,
                    monsterId: monsterId,
                    injectedCount: 1,
                    sourceMonsterUid: sourceMonsterUid,
                    hasSourcePosition: true,
                    sourceX: sourceX,
                    sourceY: sourceY,
                    sourceZ: sourceZ,
                    targetSpawnX: targetSpawnPosition.X,
                    targetSpawnY: targetSpawnPosition.Y,
                    targetSpawnZ: targetSpawnPosition.Z));

                _killCountForOpponentInjection[killerPlayerIndex] -= KillCountThresholdForOpponentInjection;
            }
        }
        /// <summary>
        /// TryPickRandomOpponentPlayerIndex 메서드입니다.
        /// </summary>

        private bool TryPickRandomOpponentPlayerIndex(int sourcePlayerIndex, out int targetPlayerIndex)
        {
            targetPlayerIndex = -1;
            if (_playerCount <= 1)
            {
                return false;
            }

            var candidates = new List<int>(_playerCount - 1);
            for (var i = 0; i < _playerCount; i++)
            {
                if (i == sourcePlayerIndex || _state.IsPlayerGameOver(i))
                {
                    continue;
                }

                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            targetPlayerIndex = candidates[_random.Next(candidates.Count)];
            return true;
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
        /// <summary>
        /// BuildBaseAttackAbility 메서드입니다.
        /// </summary>

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
                TargetingStrategy = CreateTargetingStrategy(definition.TargetingType, range),
                AppliedEffects = new List<GameplayEffect>
                {
                    new GameplayEffect
                    {
                        EffectTag = new FGameplayTag("Effect.NormalDamage"),
                        DisplayName = "NormalDamage",
                        DurationType = EffectDurationType.Instant,
                        Modifiers = new List<AttributeModifier>
                        {
                            new AttributeModifier
                            {
                                ValueMode = AttributeModifierValueMode.Calculated,
                                AttributeId = AttributeId.Health,
                                Operation = AttributeModifierOperationType.Add,
                                CalculatorType = AttributeCalculatorType.DamageBySourceAttackDamage
                            }
                        }
                    }
                }
            };

            return ability;
        }
        /// <summary>
        /// BuildTagContainer 메서드입니다.
        /// </summary>

        private static GameplayTagContainer BuildTagContainer(string tag)
        {
            var container = new GameplayTagContainer();
            container.AddTag(new FGameplayTag(tag));
            return container;
        }
        /// <summary>
        /// CreateTargetingStrategy 메서드입니다.
        /// </summary>

        private static ITargetingStrategy CreateTargetingStrategy(TowerTargetingType targetingType, float range)
        {
            return targetingType switch
            {
                TowerTargetingType.Nearest => TargetingStrategyFactory.Create(TargetingStrategyType.NearestEnemy, maxRange: range),
                TowerTargetingType.Random => TargetingStrategyFactory.Create(TargetingStrategyType.Random, maxRange: range),
                TowerTargetingType.LowestHp => TargetingStrategyFactory.Create(TargetingStrategyType.LowestHp, maxRange: range),
                TowerTargetingType.Area => TargetingStrategyFactory.Create(TargetingStrategyType.Area, radius: range),
                TowerTargetingType.None => null,
                _ => TargetingStrategyFactory.Create(TargetingStrategyType.NearestEnemy, maxRange: range)
            };
        }
        /// <summary>
        /// CreateFallbackDefinition 메서드입니다.
        /// </summary>

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
                InitialGrade = grade,
                BaseAttackDamage = source.ASC.Get(AttributeId.AttackDamage) / gradeMultiplier,
                BaseAttackSpeed = source.ASC.Get(AttributeId.AttackSpeed),
                BaseAttackRange = source.ASC.Get(AttributeId.AttackRange),
                AttackType = source.AttackType,
                ProjectileType = source.ProjectileType,
                ProjectileSpeed = source.ProjectileSpeed,
                ThrowRadius = source.ThrowRadius,
                TrapDelay = source.TrapDelay,
                OnMergeSourceEffects = source.OnMergeSourceEffects != null ? new List<GameplayEffect>(source.OnMergeSourceEffects) : new List<GameplayEffect>(),
                OnMergeTargetEffects = source.OnMergeTargetEffects != null ? new List<GameplayEffect>(source.OnMergeTargetEffects) : new List<GameplayEffect>(),
                TargetingType = source.ProjectileType == ProjectileType.Throw ? TowerTargetingType.None : TowerTargetingType.Nearest
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// 몬스터 경로를 설정합니다.
        /// </summary>
        public void SetMonsterPath(int playerIndex, int pathIndex, MonsterPath path)
        {
            _state.SetMonsterPath(playerIndex, pathIndex, path);
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
            int playerIndex = evt.PlayerIndex;
            var pathIndex = evt.PathIndex;
            var path = _state.GetMonsterPath(playerIndex, pathIndex);
            if (path == null)
            {
                return;
            }

            var monster = _state.CreateMonster(
                playerIndex,
                evt.MonsterId,
                pathIndex,
                path.GetStartPosition(),
                damageToPlayer: 10,
                goldReward: 10 + evt.DifficultyStep
            );
            monster.SetAI(_defaultMonsterAI);

            // ASC 초기화 (난이도에 따라 스케일링)
            var difficultyMultiplier = 1f + (evt.DifficultyStep - 1) * 0.1f;
            monster.ASC.Set(AttributeId.MaxHealth, BaseMonsterMaxHealth * difficultyMultiplier);
            monster.ASC.Set(AttributeId.Health, BaseMonsterMaxHealth * difficultyMultiplier);
            monster.ASC.Set(AttributeId.MoveSpeed, 2f);

            PublishEvent(CreateMonsterSpawnedEvent(playerIndex, monster));

            evt.Handled = true;
        }

        #endregion
        /// <summary>
        /// Dispose 메서드입니다.
        /// </summary>

        public override void Dispose()
        {
            DisposeModules();
            _innerEventBus.Unsubscribe<MonsterSpawnRequestInnerEvent>(OnMonsterSpawnRequest);
            base.Dispose();
            _state?.Dispose();
        }
    }

}










