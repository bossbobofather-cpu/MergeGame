using System;
using System.Collections.Generic;
using Mirror;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Modules;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 서버 측 네트워크 어댑터입니다.
    /// 
    /// 책임:
    /// - 클라이언트 CommandMsg 수신 -> Host 커맨드로 변환/주입
    /// - Host의 Event/Snapshot -> 클라이언트로 전송
    /// - 프로토타입용 자동 시뮬레이션(더미 캐릭터 스폰)
    /// 
    /// 주의:
    /// Host.FlushEvents()는 서버에서 1곳(여기)에서만 호출해야 합니다.
    /// (클라/뷰가 같은 Host를 FlushEvents 하면 이벤트를 선점할 수 있습니다.)
    /// </summary>
    public sealed class MergeGameServerAdapter : MonoBehaviour
    {
        [Header("Match")]
        [SerializeField] private int _killsPerGarbage = 10;

        [Header("Snapshot")]
        [SerializeField] private float _snapshotSendInterval = 1f;

        [Header("Dev Simulation")]
        [SerializeField] private float _dummySpawnInterval = 1f;
        [SerializeField] private string _dummyTowerId = "unit_basic";

        [Header("Mode")]
        [SerializeField] private bool _reserveSlot0ForLocalHost = true;

        /// <summary>
        /// Host(Server+LocalClient)인지, Dedicated Server인지에 따라 플레이어 슬롯 매핑이 달라집니다.
        /// - Host 모드: localConnection(connectionId=0)을 playerIndex=0으로 고정합니다.
        /// - ServerOnly: 먼저 접속한 클라이언트가 playerIndex=0을 차지합니다.
        /// </summary>
        public void Configure(bool reserveSlot0ForLocalHost)
        {
            _reserveSlot0ForLocalHost = reserveSlot0ForLocalHost;
        }


        private readonly Dictionary<int, int> _playerIndexByConnectionId = new();
        private readonly NetworkConnectionToClient[] _connections = new NetworkConnectionToClient[2];

        private readonly long[] _playerUids = { 1, 2 };

        private MergeGameHost _hostA;
        private MergeGameHost _hostB;
        private MergeGameMatchHost _matchHost;

        private readonly bool[] _playerReady = new bool[2];

        private readonly bool[] _playerStarted = new bool[2];
        private readonly bool[] _playerGameOver = new bool[2];

        private bool _matchStarted;

        private readonly float[] _dummySpawnTimers = new float[2];
        private float _snapshotTimer;

        private bool _initialized;

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (!NetworkServer.active)
            {
                Debug.LogWarning("[MergeGameServerAdapter] NetworkServer가 활성화되지 않았습니다.");
                return;
            }

            _initialized = true;

            NetworkServer.OnConnectedEvent += HandleConnected;
            NetworkServer.OnDisconnectedEvent += HandleDisconnected;
            NetworkServer.RegisterHandler<CommandMsg>(HandleCommandMsg, requireAuthentication: false);

            // Host 생성/조립
            _hostA = BuildHost();
            _hostB = BuildHost();

            _hostA.ResultProduced += result => OnHostResult(0, result);
            _hostB.ResultProduced += result => OnHostResult(1, result);

            _hostA.EventRaised += evt => OnHostEvent(0, evt);
            _hostB.EventRaised += evt => OnHostEvent(1, evt);

            _matchHost = new MergeGameMatchHost(_hostA, _hostB, killsPerGarbage: _killsPerGarbage);

            _hostA.StartSimulation();
            _hostB.StartSimulation();

            Debug.Log("[MergeGameServerAdapter] ServerAdapter initialized.");
        }

        private void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            NetworkServer.OnConnectedEvent -= HandleConnected;
            NetworkServer.OnDisconnectedEvent -= HandleDisconnected;

            _matchHost?.Dispose();

            if (_hostA != null)
            {
                _hostA.StopSimulation();
                _hostA.Dispose();
            }

            if (_hostB != null)
            {
                _hostB.StopSimulation();
                _hostB.Dispose();
            }

            _hostA = null;
            _hostB = null;
        }

        private void Update()
        {
            if (!_initialized || !NetworkServer.active)
            {
                return;
            }

            // Host -> ServerAdapter/MatchHost로 이벤트 디스패치
            _hostA?.FlushEvents();
            _hostB?.FlushEvents();

            // 프로토타입: 1초마다 더미 캐릭터 자동 스폰
            TickDummySpawn(0, Time.deltaTime);
            TickDummySpawn(1, Time.deltaTime);

            // 스냅샷 전송
            _snapshotTimer += Time.deltaTime;
            if (_snapshotTimer >= _snapshotSendInterval)
            {
                _snapshotTimer -= _snapshotSendInterval;

                SendSnapshotToPlayer(0);
                SendSnapshotToPlayer(1);
            }
        }

        private void TickDummySpawn(int playerIndex, float deltaSeconds)
        {
            if (!_playerStarted[playerIndex] || _playerGameOver[playerIndex])
            {
                return;
            }

            _dummySpawnTimers[playerIndex] += deltaSeconds;
            if (_dummySpawnTimers[playerIndex] < _dummySpawnInterval)
            {
                return;
            }

            _dummySpawnTimers[playerIndex] -= _dummySpawnInterval;

            var host = GetHost(playerIndex);
            if (host == null)
            {
                return;
            }

            host.SendCommand(new SpawnTowerCommand(
                senderUid: _playerUids[playerIndex],
                towerId: _dummyTowerId,
                slotIndex: -1
            ));
        }

        private void SendSnapshotToPlayer(int playerIndex)
        {
            var conn = _connections[playerIndex];
            if (conn == null)
            {
                return;
            }

            var host = GetHost(playerIndex);
            if (host == null)
            {
                return;
            }

            var snapshot = host.GetLatestSnapshot();
            if (snapshot == null)
            {
                return;
            }

            var monsters = snapshot.Monsters;
            var towers = snapshot.Towers;
            var usedSlots = snapshot.UsedSlots;

            var p0 = 0f;
            var p1 = 0f;

            if (monsters != null)
            {
                if (monsters.Count > 0) p0 = monsters[0].PathProgress;
                if (monsters.Count > 1) p1 = monsters[1].PathProgress;
            }

            var msg = new SnapshotMsg
            {
                PlayerIndex = playerIndex,
                Tick = snapshot.Tick,
                SessionPhase = (int)snapshot.SessionPhase,
                WaveNumber = snapshot.CurrentWaveNumber,
                WavePhase = (int)snapshot.WavePhase,
                MonsterCount = monsters?.Count ?? 0,
                TowerCount = towers?.Count ?? 0,
                UsedSlotCount = usedSlots,
                SampleMonsterProgress0 = p0,
                SampleMonsterProgress1 = p1,
            };

            conn.Send(msg);
        }

        private void HandleConnected(NetworkConnectionToClient conn)
        {
            var playerIndex = ResolvePlayerIndexForConnection(conn);
            if (playerIndex < 0)
            {
                Debug.LogWarning($"[MergeGameServerAdapter] 플레이어 슬롯이 꽉 찼습니다. connId={conn.connectionId}");
                conn.Disconnect();
                return;
            }

            _playerIndexByConnectionId[conn.connectionId] = playerIndex;
            _connections[playerIndex] = conn;

            Debug.Log($"[MergeGameServerAdapter] Client connected: connId={conn.connectionId} -> playerIndex={playerIndex}");


            _playerReady[playerIndex] = false;
            _playerStarted[playerIndex] = false;
            _playerGameOver[playerIndex] = false;

            SendLog(playerIndex, 0, "[연결] 준비(Ready) 커맨드를 기다립니다.");
        }

        private void HandleDisconnected(NetworkConnectionToClient conn)
        {
            if (_playerIndexByConnectionId.TryGetValue(conn.connectionId, out var playerIndex))
            {
                _playerIndexByConnectionId.Remove(conn.connectionId);

                if (playerIndex >= 0 && playerIndex < _connections.Length && _connections[playerIndex] == conn)
                {
                    _connections[playerIndex] = null;
                }

                _playerReady[playerIndex] = false;
                _playerStarted[playerIndex] = false;
                _playerGameOver[playerIndex] = false;
                _dummySpawnTimers[playerIndex] = 0f;

                // 프로토타입: 연결이 끊기면 다시 매치 시작 가능 상태로 되돌립니다.
                _matchStarted = false;

                Debug.Log($"[MergeGameServerAdapter] Client disconnected: connId={conn.connectionId} (playerIndex={playerIndex})");
            }
        }

        private int ResolvePlayerIndexForConnection(NetworkConnectionToClient conn)
        {
            // Host 모드(local client 포함)에서는 localConnection(connectionId=0)을 playerIndex=0으로 고정합니다.
            if (_reserveSlot0ForLocalHost && conn.connectionId == 0)
            {
                return 0;
            }

            // 그 외에는 빈 슬롯(0..N) 중 첫 번째를 배정합니다.
            for (var i = 0; i < _connections.Length; i++)
            {
                if (_connections[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        private void HandleCommandMsg(NetworkConnectionToClient conn, CommandMsg msg)
        {
            if (!_playerIndexByConnectionId.TryGetValue(conn.connectionId, out var playerIndex))
            {
                return;
            }

            var host = GetHost(playerIndex);
            if (host == null)
            {
                return;
            }

            // MVP: 필요한 커맨드만 지원
            switch (msg.CommandType)
            {
                case MergeNetCommandType.StartGame:
                case MergeNetCommandType.Ready:
                    HandleReady(playerIndex);
                    break;

                case MergeNetCommandType.SpawnTower:
                    host.SendCommand(new SpawnTowerCommand(
                        senderUid: _playerUids[playerIndex],
                        towerId: string.IsNullOrEmpty(msg.Str0) ? _dummyTowerId : msg.Str0,
                        slotIndex: msg.Int0
                    ));
                    break;

                case MergeNetCommandType.MergeTower:
                    host.SendCommand(new MergeTowerCommand(
                        senderUid: _playerUids[playerIndex],
                        fromSlotIndex: msg.Int0,
                        toSlotIndex: msg.Int1
                    ));
                    break;
            }
        }

        private void HandleReady(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _playerReady.Length)
            {
                return;
            }

            if (_matchStarted)
            {
                SendLog(playerIndex, 0, "[준비] 이미 게임이 시작되었습니다.");
                return;
            }

            if (_playerReady[playerIndex])
            {
                return;
            }

            _playerReady[playerIndex] = true;
            SendLog(playerIndex, 0, "[준비] Ready 완료.");

            TryStartMatchIfReady();
        }

        private void TryStartMatchIfReady()
        {
            if (_matchStarted)
            {
                return;
            }

            // 2명 연결 + 2명 준비가 완료되어야 게임을 시작합니다.
            if (_connections[0] == null || _connections[1] == null)
            {
                return;
            }

            if (!_playerReady[0] || !_playerReady[1])
            {
                return;
            }

            _matchStarted = true;

            // 시작과 동시에 더미 스폰 타이머를 리셋합니다.
            _dummySpawnTimers[0] = 0f;
            _dummySpawnTimers[1] = 0f;

            SendLog(0, 0, "[매치] 양쪽 준비 완료. 게임을 시작합니다.");
            SendLog(1, 0, "[매치] 양쪽 준비 완료. 게임을 시작합니다.");

            GetHost(0)?.SendCommand(new StartMergeGameCommand(_playerUids[0]));
            GetHost(1)?.SendCommand(new StartMergeGameCommand(_playerUids[1]));
        }
        private void OnHostResult(int playerIndex, MergeCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.Success)
            {
                return;
            }

            SendLog(playerIndex, result.Tick, $"[커맨드 실패] {result.GetType().Name}: {result.ErrorMessage}");
        }

        private void OnHostEvent(int playerIndex, MergeHostEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            // 서버 내부 상태 업데이트
            if (evt is MergeGameStartedEvent)
            {
                _playerStarted[playerIndex] = true;
            }
            else if (evt is MergeGameOverEvent)
            {
                _playerGameOver[playerIndex] = true;
            }

            // 너무 자주 발생하는 이벤트는 스냅샷으로 대체
            if (evt is MonsterMovedEvent)
            {
                return;
            }

            if (!TryFormatEvent(evt, out var line))
            {
                return;
            }

            SendLog(playerIndex, evt.Tick, line);
        }

        private void SendLog(int playerIndex, long tick, string text)
        {
            var conn = _connections[playerIndex];
            if (conn == null)
            {
                return;
            }

            conn.Send(new EventMsg
            {
                PlayerIndex = playerIndex,
                Tick = tick,
                EventType = MergeNetEventType.Log,
                Text = text
            });
        }

        private static bool TryFormatEvent(MergeHostEvent evt, out string line)
        {
            line = null;

            switch (evt)
            {
                case MapInitializedEvent e:
                    line = $"[맵 초기화] map={e.MapId} slots={e.SlotPositions.Count} paths={e.Paths.Count}";
                    return true;

                case MergeGameStartedEvent e:
                    line = $"[게임 시작] slots={e.SlotCount}";
                    return true;

                case WaveStartedEvent e:
                    line = $"[웨이브 시작] wave={e.WaveNumber} total={e.TotalMonsterCount}";
                    return true;

                case MonsterSpawnedEvent e:
                    line = $"[몬스터 스폰] uid={e.MonsterUid} id={e.MonsterId} path={e.PathIndex} hp={e.MaxHealth:0}";
                    return true;

                case TowerSpawnedEvent e:
                    line = $"[캐릭터 스폰] uid={e.TowerUid} id={e.TowerId} grade={e.Grade} slot={e.SlotIndex}";
                    return true;

                case TowerAttackedEvent e:
                    line = $"[공격] attacker={e.AttackerUid} -> monster={e.TargetUid} dmg={e.Damage:0}";
                    return true;

                case MonsterDamagedEvent e:
                    line = $"[피격] monster={e.MonsterUid} dmg={e.Damage:0} hp={e.CurrentHealth:0}";
                    return true;

                case MonsterDiedEvent e:
                    line = $"[처치] monster={e.MonsterUid} gold=+{e.GoldReward}";
                    return true;

                case PlayerGoldChangedEvent e:
                    line = $"[골드] {e.Reason} delta={e.GoldDelta} current={e.CurrentGold}";
                    return true;

                case MergeGameOverEvent e:
                    line = $"[게임 종료] victory={e.IsVictory} score={e.FinalScore} maxGrade={e.MaxGradeReached}";
                    return true;
            }

            return false;
        }

        private MergeGameHost GetHost(int playerIndex)
        {
            return playerIndex == 0 ? _hostA : _hostB;
        }

        private static MergeGameHost BuildHost()
        {
            var hostConfig = new MergeHostConfig()
                .WithMaxMonsterStack(100)
                .WithWaveSettings(spawnInterval: 0.2f, completionBonus: 0);

            var host = new MergeGameHost(hostConfig, new DevTowerDatabase());

            host.AddModule(new MapModule(), BuildMapConfig());
            host.AddModule(new RuleModule(), BuildRuleConfig(hostConfig));
            host.AddModule(new WaveModule(), BuildWaveConfig(hostConfig));

            host.InitializeModules();

            return host;
        }

        private static MapModuleConfig BuildMapConfig()
        {
            return new MapModuleConfig
            {
                MapId = 1,
                SlotDefinitions = MapModuleConfig.CreateGridSlotDefinitions(rows: 4, columns: 4, slotWidth: 1.5f, slotHeight: 1.5f),
                PathDefinitions = new List<PathDefinition>
                {
                    new PathDefinition(
                        pathIndex: 0,
                        waypoints: new List<Point2D>
                        {
                            new Point2D(-6f, 3f),
                            new Point2D(-2f, 3f),
                            new Point2D( 2f, 3f),
                            new Point2D( 6f, 3f)
                        }
                    )
                }
            };
        }

        private static RuleModuleConfig BuildRuleConfig(MergeHostConfig hostConfig)
        {
            return new RuleModuleConfig
            {
                PlayerMaxHp = hostConfig.PlayerMaxHp,
                PlayerStartGold = hostConfig.PlayerStartGold,
                ScorePerGrade = hostConfig.ScorePerGrade,
                InitialUnitGrade = hostConfig.InitialUnitGrade,
                MaxUnitGrade = hostConfig.MaxUnitGrade,
                WaveCompletionBonusGold = hostConfig.WaveCompletionBonusGold,
            };
        }

        private static WaveModuleConfig BuildWaveConfig(MergeHostConfig hostConfig)
        {
            // 프로토타입: 1웨이브에서 계속 스폰(수량을 크게)해서 스택 패배 조건을 확인할 수 있게 합니다.
            return new WaveModuleConfig
            {
                AutoStartWaves = true,
                WaveStartDelay = 0f,
                WaveIntervalDelay = 0f,
                DefaultSpawnInterval = hostConfig.WaveSpawnInterval,
                MaxWaveCount = 1,
                BaseMonsterCount = 1000,
                MonstersPerWaveIncrease = 0,
            };
        }

        /// <summary>
        /// 개발용 하드코딩 캐릭터 DB입니다.
        /// </summary>
        private sealed class DevTowerDatabase : ITowerDatabase
        {
            private readonly Dictionary<string, TowerDefinition> _definitions = new()
            {
                {
                    "unit_basic",
                    new TowerDefinition
                    {
                        TowerId = "unit_basic",
                        TowerType = "basic",
                        InitialGrade = 1,
                        BaseAttackDamage = 10f,
                        BaseAttackSpeed = 1f,
                        BaseAttackRange = 10f,
                    }
                },
            };

            public TowerDefinition GetDefinition(string towerId)
            {
                if (string.IsNullOrEmpty(towerId))
                {
                    return null;
                }

                return _definitions.TryGetValue(towerId, out var definition) ? definition : null;
            }

            public string GetRandomIdForGrade(int grade)
            {
                return "unit_basic";
            }
        }
    }
}


