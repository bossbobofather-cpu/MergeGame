using System.Collections.Generic;
using Mirror;
using MyProject.MergeGame;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Modules;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 서버 측 네트워크 어댑터입니다.
    /// - 플레이어별 MergeGameHost를 생성/관리합니다.
    /// - 클라이언트 CommandMsg를 Host 커맨드로 변환합니다.
    /// - Host Event/Snapshot을 EventMsg/SnapshotMsg로 전송합니다.
    /// </summary>
    public sealed class MergeGameServerAdapter : MonoBehaviour
    {
        private const int MaxSupportedPlayers = 2;

        [Header("Connection Mapping")]
        [SerializeField] private bool _reserveSlot0ForLocalHost = true;
        [SerializeField] private int _maxPlayers = 1;
        [SerializeField] private float _snapshotSendInterval = 0.1f;
        [SerializeField] private float _dummySpawnInterval = 1f;
        [SerializeField] private string _dummyTowerId = "unit_basic";

        /// <summary>
        /// Host(Server+LocalClient) 모드와 Dedicated Server 모드에 맞춰
        /// 연결 ID를 플레이어 인덱스로 매핑하는 방식을 설정합니다.
        /// </summary>
        public void Configure(bool reserveSlot0ForLocalHost)
        {
            _reserveSlot0ForLocalHost = reserveSlot0ForLocalHost;
        }

        /// <summary>
        /// 매치에 허용할 최대 플레이어 수를 설정합니다.
        /// 예: 싱글 테스트(1), 1:1 멀티(2)
        /// </summary>
        public void SetMaxPlayers(int maxPlayers)
        {
            _maxPlayers = Mathf.Clamp(maxPlayers, 1, MaxSupportedPlayers);
        }

        private readonly Dictionary<int, int> _playerIndexByConnectionId = new();
        private NetworkConnectionToClient[] _connections;
        private long[] _playerUids;
        private MergeGameHost[] _playerHosts;
        private bool[] _playerReady;
        private bool[] _playerStarted;
        private bool[] _playerGameOver;

        private bool _matchStarted;

        private float[] _dummySpawnTimers;
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

            var playerCount = Mathf.Clamp(_maxPlayers, 1, MaxSupportedPlayers);
            _connections = new NetworkConnectionToClient[playerCount];
            _playerUids = new long[playerCount];
            _playerHosts = new MergeGameHost[playerCount];
            _playerReady = new bool[playerCount];
            _playerStarted = new bool[playerCount];
            _playerGameOver = new bool[playerCount];
            _dummySpawnTimers = new float[playerCount];

            for (var i = 0; i < playerCount; i++)
            {
                _playerUids[i] = i + 1;
            }

            _initialized = true;

            NetworkServer.OnConnectedEvent += HandleConnected;
            NetworkServer.OnDisconnectedEvent += HandleDisconnected;
            NetworkServer.UnregisterHandler<CommandMsg>();
            NetworkServer.RegisterHandler<CommandMsg>(HandleCommandMsg, requireAuthentication: false);

            // 플레이어 보드별 Host를 생성하고 초기화합니다.
            for (var i = 0; i < _playerHosts.Length; i++)
            {
                var playerIndex = i;
                var host = BuildHost();
                host.ResultProduced += result => OnHostResult(playerIndex, result);
                host.EventRaised += evt => OnHostEvent(playerIndex, evt);
                host.StartSimulation();
                _playerHosts[playerIndex] = host;
            }

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
            NetworkServer.UnregisterHandler<CommandMsg>();

            if (_playerHosts != null)
            {
                for (var i = 0; i < _playerHosts.Length; i++)
                {
                    var host = _playerHosts[i];
                    if (host == null)
                    {
                        continue;
                    }

                    host.StopSimulation();
                    host.Dispose();
                    _playerHosts[i] = null;
                }
            }

            _initialized = false;
        }

        private void Update()
        {
            if (!_initialized || !NetworkServer.active || _playerHosts == null)
            {
                return;
            }

            // 서버 프레임에서 각 Host의 Result/Event를 메인 스레드로 디스패치합니다.
            for (var i = 0; i < _playerHosts.Length; i++)
            {
                _playerHosts[i]?.FlushEvents();
            }

            // 더미 테스트: 일정 주기로 기본 타워를 자동 생성합니다.
            for (var i = 0; i < _playerHosts.Length; i++)
            {
                TickDummySpawn(i, Time.deltaTime);
            }

            // 스냅샷 주기 전송
            _snapshotTimer += Time.deltaTime;
            if (_snapshotTimer >= _snapshotSendInterval)
            {
                _snapshotTimer -= _snapshotSendInterval;

                for (var i = 0; i < _playerHosts.Length; i++)
                {
                    SendSnapshotToPlayer(i);
                }
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
                Debug.LogWarning($"[MergeGameServerAdapter] 플레이어 슬롯이 가득 찼습니다. connId={conn.connectionId}");
                conn.Disconnect();
                return;
            }

            _playerIndexByConnectionId[conn.connectionId] = playerIndex;
            _connections[playerIndex] = conn;

            Debug.Log($"[MergeGameServerAdapter] 클라이언트 연결 성공. connId={conn.connectionId}, playerIndex={playerIndex}");

            _playerReady[playerIndex] = false;
            _playerStarted[playerIndex] = false;
            _playerGameOver[playerIndex] = false;

            SendLog(playerIndex, 0, "[서버] 준비(Ready) 커맨드를 보내주세요.");
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

                // 연결이 끊기면 Ready 게이트를 다시 열어둡니다.
                _matchStarted = false;

                Debug.Log($"[MergeGameServerAdapter] 클라이언트 연결 해제. connId={conn.connectionId}, playerIndex={playerIndex}");
            }
        }

        private int ResolvePlayerIndexForConnection(NetworkConnectionToClient conn)
        {
            // Host 모드에서는 localConnection(connectionId=0)을 playerIndex=0으로 예약합니다.
            if (_reserveSlot0ForLocalHost && conn.connectionId == 0)
            {
                return 0;
            }

            // 비어 있는 첫 슬롯을 배정합니다.
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

            // MVP에서는 준비/타워 생성/머지 커맨드만 처리합니다.
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
            SendLog(playerIndex, 0, "[준비] Ready 수신.");

            TryStartMatchIfReady();
        }

        private void TryStartMatchIfReady()
        {
            if (_matchStarted)
            {
                return;
            }

            for (var i = 0; i < _connections.Length; i++)
            {
                if (_connections[i] == null || !_playerReady[i])
                {
                    return;
                }
            }

            _matchStarted = true;

            // 매치 시작 직후 타이머를 초기화하고 게임 시작 커맨드를 보냅니다.
            for (var i = 0; i < _dummySpawnTimers.Length; i++)
            {
                _dummySpawnTimers[i] = 0f;
                SendLog(i, 0, "[매치] 준비 완료. 게임을 시작합니다.");
                GetHost(i)?.SendCommand(new StartMergeGameCommand(_playerUids[i]));
            }
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

            // 시작/종료 상태를 서버 내부 플래그에 반영합니다.
            if (evt is MergeGameStartedEvent)
            {
                _playerStarted[playerIndex] = true;
            }
            else if (evt is MergeGameOverEvent)
            {
                _playerGameOver[playerIndex] = true;
            }

            // 이동 이벤트는 빈도가 높아 로그에서 제외합니다.
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
            if (playerIndex < 0 || playerIndex >= _connections.Length)
            {
                return;
            }

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
                    line = $"[몬스터 생성] uid={e.MonsterUid} id={e.MonsterId} path={e.PathIndex} hp={e.MaxHealth:0}";
                    return true;

                case TowerSpawnedEvent e:
                    line = $"[타워 생성] uid={e.TowerUid} id={e.TowerId} grade={e.Grade} slot={e.SlotIndex}";
                    return true;

                case TowerAttackedEvent e:
                    line = $"[공격] attacker={e.AttackerUid} -> monster={e.TargetUid} dmg={e.Damage:0}";
                    return true;

                case MonsterDamagedEvent e:
                    line = $"[피해] monster={e.MonsterUid} dmg={e.Damage:0} hp={e.CurrentHealth:0}";
                    return true;

                case MonsterDiedEvent e:
                    line = $"[처치] monster={e.MonsterUid} gold=+{e.GoldReward}";
                    return true;

                case PlayerGoldChangedEvent e:
                    line = $"[골드] reason={e.Reason} delta={e.GoldDelta} current={e.CurrentGold}";
                    return true;

                case MergeGameOverEvent e:
                    line = $"[게임 종료] victory={e.IsVictory} score={e.FinalScore} maxGrade={e.MaxGradeReached}";
                    return true;
            }

            return false;
        }

        private MergeGameHost GetHost(int playerIndex)
        {
            if (_playerHosts == null || playerIndex < 0 || playerIndex >= _playerHosts.Length)
            {
                return null;
            }

            return _playerHosts[playerIndex];
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
                        waypoints: new List<Point3D>
                        {
                            new Point3D(-6f, 3f, 0f),
                            new Point3D(-2f, 3f, 0f),
                            new Point3D( 2f, 3f, 0f),
                            new Point3D( 6f, 3f, 0f)
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
            // 자동 진행용 단순 웨이브 설정입니다.
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
        /// 개발/테스트용 타워 데이터베이스입니다.
        /// 실서비스에서는 외부 데이터 소스(JSON/DB 등)로 교체할 수 있습니다.
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
                        AttackType = TowerAttackType.HitScan,
                        ProjectileType = ProjectileType.Direct,
                        ProjectileSpeed = 8f,
                        ThrowRadius = 1.5f,
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
