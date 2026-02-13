using System;
using System.Collections.Generic;
using Mirror;
using MyProject.MergeGame;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Modules;
using MyProject.MergeGame.Snapshots;
using Noname.GameAbilitySystem;
using Noname.GameHost;
using UnityEngine;
using Time = UnityEngine.Time;

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

        /// <summary>
        /// Host(Server+LocalClient) 모드와 Dedicated Server 모드에 맞춰
        /// 연결 ID를 플레이어 인덱스로 매핑하는 방식을 설정합니다.
        /// </summary>
        public void Configure(bool reserveSlot0ForLocalHost)
        {
            // 핵심 로직을 처리합니다.
            _reserveSlot0ForLocalHost = reserveSlot0ForLocalHost;
        }

        /// <summary>
        /// 매치에 허용할 최대 플레이어 수를 설정합니다.
        /// 예: 싱글 테스트(1), 1:1 멀티(2)
        /// </summary>
        public void SetMaxPlayers(int maxPlayers)
        {
            // 핵심 로직을 처리합니다.
            _maxPlayers = Mathf.Clamp(maxPlayers, 1, MaxSupportedPlayers);
        }

        private readonly Dictionary<int, (NetworkConnectionToClient, MergeGamePlayer)> _playerAndConnectionByConnId = new();
        private MergeGameHost _host;

        private bool _matchStarted;
        private float _snapshotTimer;
        private readonly HashSet<long> _readyResultAckedUids = new();

        private bool _initialized;
        /// <summary>
        /// Initialize 함수를 처리합니다.
        /// </summary>

        public void Initialize()
        {
            // 핵심 로직을 처리합니다.
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

            _initialized = true;

            // NetworkServer.OnConnectedEvent는 연결 직후 호출되므로 인증 데이터가 아직 없을 수 있습니다.
            // Authenticator가 있다면 인증 완료 이벤트를 구독해야 합니다.
            if (NetworkManager.singleton.authenticator != null)
            {
                NetworkManager.singleton.authenticator.OnServerAuthenticated.AddListener(HandleConnected);
            }
            else
            {
                NetworkServer.OnConnectedEvent += HandleConnected;
            }
            
            NetworkServer.OnDisconnectedEvent += HandleDisconnected;
            NetworkServer.UnregisterHandler<NetCommandMessage>();
            NetworkServer.RegisterHandler<NetCommandMessage>(HandleCommandMsg);

            // 단일 Host를 생성하고 초기화합니다.
            _host = BuildHost();
            _host.InitializePlayers(playerCount);

            _host.ResultProduced += OnHostResult;
            _host.EventRaised += OnHostEvent;
            _host.StartSimulation();

            Debug.Log("[MergeGameServerAdapter] ServerAdapter initialized.");
        }
        /// <summary>
        /// OnDestroy 함수를 처리합니다.
        /// </summary>

        private void OnDestroy()
        {
            // 핵심 로직을 처리합니다.
            if (!_initialized)
            {
                return;
            }

            if (NetworkManager.singleton != null && NetworkManager.singleton.authenticator != null)
            {
                NetworkManager.singleton.authenticator.OnServerAuthenticated.RemoveListener(HandleConnected);
            }

            NetworkServer.OnConnectedEvent -= HandleConnected;
            NetworkServer.OnDisconnectedEvent -= HandleDisconnected;
            NetworkServer.UnregisterHandler<NetCommandMessage>();

            if (_host != null)
            {
                _host.ResultProduced -= OnHostResult;
                _host.EventRaised -= OnHostEvent;
                _host.StopSimulation();
                _host.Dispose();
                _host = null;
            }

            _initialized = false;
        }
        /// <summary>
        /// Update 함수를 처리합니다.
        /// </summary>

        private void Update()
        {
            // 핵심 로직을 처리합니다.
            if (!_initialized || !NetworkServer.active || _host == null)
            {
                return;
            }

            // 서버 프레임에서 Host의 Result/Event를 메인 스레드로 디스패치합니다.
            _host.FlushEvents();
            // 스냅샷 주기 전송
            _snapshotTimer += Time.deltaTime;
            if (_snapshotTimer >= _snapshotSendInterval)
            {
                _snapshotTimer -= _snapshotSendInterval;
                BroadcastSnapshotsToAllClients();
            }
        }
        /// <summary>
        /// BroadcastSnapshotsToAllClients 함수를 처리합니다.
        /// </summary>

        private void BroadcastSnapshotsToAllClients()
        {
            // 핵심 로직을 처리합니다.
            var maxPlayers = Mathf.Clamp(_maxPlayers, 1, MaxSupportedPlayers);
            for (var playerIndex = 0; playerIndex < maxPlayers; playerIndex++)
            {
                var snapshot = _host.GetPlayerSnapshot(playerIndex);
                if (snapshot == null)
                {
                    continue;
                }

                var pooled = ByteSerializer.SerializePooled(snapshot);
                var msg = new NetSnapshotMessage
                {
                    PlayerIndex = playerIndex,
                    Payload = pooled.Segment,
                };

                foreach (var playerAndConnection in _playerAndConnectionByConnId)
                {
                    var conn = playerAndConnection.Value.Item1;
                    if (conn == null)
                    {
                        continue;
                    }

                    conn.Send(msg);
                }

                pooled.Dispose();
            }
        }
        /// <summary>
        /// HandleConnected 함수를 처리합니다.
        /// </summary>

        private void HandleConnected(NetworkConnectionToClient conn)
        {
            // 핵심 로직을 처리합니다.
            if (conn == null)
            {
                return;
            }

            if (_playerAndConnectionByConnId.ContainsKey(conn.connectionId))
            {
                Debug.LogWarning($"[MergeGameServerAdapter] 이미 등록된 연결입니다. connId={conn.connectionId}");
                return;
            }

            if (!TryResolveUserId(conn.authenticationData, out var uid))
            {
                Debug.LogWarning($"[MergeGameServerAdapter] 유효하지 않은 인증 정보(UserId) 입니다. connId={conn.connectionId}");
                conn.Disconnect();
                return;
            }

            if (uid < 0)
            {
                Debug.LogWarning($"[MergeGameServerAdapter] 유효하지 않은 인증 정보(UserId) 입니다. connId={conn.connectionId}, uid={uid}");
                conn.Disconnect();
                return;
            }

            var playerIndex = ResolvePlayerIndexForConnection(conn);
            if (playerIndex < 0)
            {
                Debug.LogWarning($"[MergeGameServerAdapter] 플레이어 슬롯이 가득 찼습니다. connId={conn.connectionId}");
                conn.Disconnect();
                return;
            }

            var player = new MergeGamePlayer(uid, playerIndex);
            _readyResultAckedUids.Remove(uid);
            _playerAndConnectionByConnId.Add(conn.connectionId, (conn, player));

            Debug.Log($"[MergeGameServerAdapter] 클라이언트 연결 성공. connId={conn.connectionId}, playerIndex={playerIndex}");

            //클라이언트 연결되면 호스트에 등록
            _host?.RegisterPlayer(uid, playerIndex);

            // 클라이언트에게 자신에게 할당된 플레이어 인덱스를 알려줍니다.
            ConnectedInfoEvent connectedEvent = new(0, playerIndex);
            var pooled = ByteSerializer.SerializePooled(connectedEvent);
            conn.Send(new NetEventMessage
            {
                PlayerIndex = playerIndex,
                Tick = 0,
                EventType = MergeNetEventType.ConnectedInfo,
                Payload = pooled.Segment,
            });

            pooled.Dispose();
        }
        /// <summary>
        /// TryResolveUserId 함수를 처리합니다.
        /// </summary>

        private static bool TryResolveUserId(object authenticationData, out long uid)
        {
            // 핵심 로직을 처리합니다.
            uid = 0;
            if (authenticationData == null)
            {
                return false;
            }

            switch (authenticationData)
            {
                case long longValue:
                    uid = longValue;
                    return true;
                case int intValue:
                    uid = intValue;
                    return true;
                case string strValue:
                    return long.TryParse(strValue, out uid);
                default:
                    return false;
            }
        }
        /// <summary>
        /// HandleDisconnected 함수를 처리합니다.
        /// </summary>

        private void HandleDisconnected(NetworkConnectionToClient conn)
        {
            // 핵심 로직을 처리합니다.
            var connId = conn.connectionId;

            _playerAndConnectionByConnId.TryGetValue(connId, out var playerAndConnection);
            var player = playerAndConnection.Item2;
            if (player != null)
            {
                _readyResultAckedUids.Remove(player.Uid);
                player.Dispose();
            }

            _playerAndConnectionByConnId.Remove(conn.connectionId);

            // 모든 플레이어가 나간 경우에만 매치 상태를 초기화합니다.
            if (_playerAndConnectionByConnId.Count == 0)
            {
                _matchStarted = false;
                _readyResultAckedUids.Clear();
            }
        }
        /// <summary>
        /// ResolvePlayerIndexForConnection 함수를 처리합니다.
        /// </summary>

        private int ResolvePlayerIndexForConnection(NetworkConnectionToClient conn)
        {
            // 핵심 로직을 처리합니다.
            var maxPlayers = Mathf.Clamp(_maxPlayers, 1, MaxSupportedPlayers);

            // Host 모드에서는 localConnection(connectionId=0)을 playerIndex=0으로 예약합니다.
            if (_reserveSlot0ForLocalHost && conn.connectionId == 0)
            {
                return maxPlayers > 0 ? 0 : -1;
            }

            for (var index = 0; index < maxPlayers; index++)
            {
                if (_reserveSlot0ForLocalHost && index == 0)
                {
                    continue;
                }

                var inUse = false;
                foreach (var kv in _playerAndConnectionByConnId)
                {
                    var existingPlayer = kv.Value.Item2;
                    if (existingPlayer != null && existingPlayer.Index == index)
                    {
                        inUse = true;
                        break;
                    }
                }

                if (!inUse)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// 클라이언트로 부터 받은 커맨드를 호스트에 전달 한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="payload"></param>
        private void HandleCommandMsg(NetworkConnectionToClient conn, NetCommandMessage msg)
        {
            // 핵심 로직을 처리합니다.
            if (_host == null) return;

            _playerAndConnectionByConnId.TryGetValue(conn.connectionId, out var playerAndConnection);
            var player = playerAndConnection.Item2;
            if (player == null) return;

            switch (msg.CommandType)
            {
                case MergeNetCommandType.ReadyGame:
                    HandleCommandMsg_ReadyGame(player, msg.Payload);
                    break;
                case MergeNetCommandType.ExitGame:
                {
                    var cmd = ExitMergeGameCommand.ReadFrom(msg.Payload);
                    _host.SendCommand(cmd);
                    break;
                }
                case MergeNetCommandType.SpawnTower:
                {
                    var cmd = SpawnTowerCommand.ReadFrom(msg.Payload);
                    _host.SendCommand(cmd);
                    break;
                }
                case MergeNetCommandType.MergeTower:
                {
                    var cmd = MergeTowerCommand.ReadFrom(msg.Payload);
                    _host.SendCommand(cmd);
                    break;
                }
                case MergeNetCommandType.InjectMonsters:
                {
                    var cmd = InjectMonstersCommand.ReadFrom(msg.Payload);
                    _host.SendCommand(cmd);
                    break;
                }
            }
        }
        /// <summary>
        /// HandleCommandMsg_ReadyGame 함수를 처리합니다.
        /// </summary>


        private void HandleCommandMsg_ReadyGame(MergeGamePlayer player, ArraySegment<byte> payload)
        {
            // 핵심 로직을 처리합니다.
            if (player == null)
            {
                return;
            }

            if (player.State == MergeGamePlayerState.Started)
            {
                return;
            }

            if (player.State != MergeGamePlayerState.Ready)
            {
                player.SetState(MergeGamePlayerState.Ready);
            }

            SendReadyAcceptedResultOnce(player);
            TryStartMatchIfReady();
        }
        /// <summary>
        /// TryStartMatchIfReady 함수를 처리합니다.
        /// </summary>

        private void TryStartMatchIfReady()
        {
            // 핵심 로직을 처리합니다.
            if (_matchStarted || _host == null)
            {
                return;
            }

            var requiredPlayers = Mathf.Clamp(_maxPlayers, 1, MaxSupportedPlayers);
            var connectedPlayers = new List<MergeGamePlayer>();

            foreach (var pair in _playerAndConnectionByConnId)
            {
                var conn = pair.Value.Item1;
                var player = pair.Value.Item2;
                if (conn == null || player == null)
                {
                    continue;
                }

                connectedPlayers.Add(player);
            }

            if (connectedPlayers.Count < requiredPlayers)
            {
                return;
            }

            for (var i = 0; i < connectedPlayers.Count; i++)
            {
                if (connectedPlayers[i].State != MergeGamePlayerState.Ready)
                {
                    return;
                }
            }

            _matchStarted = true;

            for (var i = 0; i < connectedPlayers.Count; i++)
            {
                var player = connectedPlayers[i];
                var cmd = new ReadyMergeGameCommand(player.Uid);
                _host.SendCommand(cmd);
                player.SetState(MergeGamePlayerState.Started);
            }
        }
        /// <summary>
        /// SendReadyAcceptedResultOnce 함수를 처리합니다.
        /// </summary>

        private void SendReadyAcceptedResultOnce(MergeGamePlayer player)
        {
            // 핵심 로직을 처리합니다.
            if (player == null || _host == null)
            {
                return;
            }

            if (!_readyResultAckedUids.Add(player.Uid))
            {
                return;
            }

            var readyResult = ReadyMergeGameResult.Ok(_host.Tick, player.Uid);
            SendResultToPlayer(readyResult, MergeNetCommandType.ReadyGame);
        }

        /// <summary>
        /// 호스트로부터 받은 결과를 클라이언트에 전송한다.
        /// </summary>
        /// <param name="result"></param>
        private void OnHostResult(MergeCommandResult result)
        {
            // 핵심 로직을 처리합니다.
            if (result == null)
            {
                return;
            }

            switch (result)
            {
                case ReadyMergeGameResult readyResult:
                    if (readyResult.Success && _readyResultAckedUids.Contains(readyResult.SenderUid))
                    {
                        break;
                    }

                    SendResultToPlayer(readyResult, MergeNetCommandType.ReadyGame);
                    break;
                case SpawnTowerResult spawnResult:
                    SendResultToPlayer(spawnResult, MergeNetCommandType.SpawnTower);
                    break;
                case MergeTowerResult mergeResult:
                    SendResultToPlayer(mergeResult, MergeNetCommandType.MergeTower);
                    break;
                case ExitMergeGameResult endResult:
                    SendResultToPlayer(endResult, MergeNetCommandType.ExitGame);
                    break;
                case InjectMonstersResult injectResult:
                    SendResultToPlayer(injectResult, MergeNetCommandType.InjectMonsters);
                    break;
            }
        }
        /// <summary>
        /// OnHostEvent 함수를 처리합니다.
        /// </summary>

        private void OnHostEvent(MergeGameEvent mergeGameEvent)
        {
            // 핵심 로직을 처리합니다.
            if (mergeGameEvent == null)
            {
                return;
            }

            MergeNetEventType eventType = mergeGameEvent switch
            {
                GameStartedEvent => MergeNetEventType.GameStarted,
                GameOverEvent => MergeNetEventType.GameOver,
                MapInitializedEvent => MergeNetEventType.MapInitialized,
                TowerSpawnedEvent => MergeNetEventType.TowerSpawned,
                TowerMergedEvent => MergeNetEventType.TowerMerged,
                TowerRemovedEvent => MergeNetEventType.TowerRemoved,
                TowerAttackedEvent => MergeNetEventType.TowerAttacked,
                EffectTriggeredEvent => MergeNetEventType.EffectTriggered,
                MonsterSpawnedEvent => MergeNetEventType.MonsterSpawned,
                MonsterDamagedEvent => MergeNetEventType.MonsterDamaged,
                MonsterDiedEvent => MergeNetEventType.MonsterDied,
                MonsterMovedEvent => MergeNetEventType.MonsterMoved,
                MonsterInjectionTriggeredEvent => MergeNetEventType.MonsterInjected,
                DifficultyStepChangedEvent => MergeNetEventType.DifficultyStepChangedEvent,
                ScoreChangedEvent => MergeNetEventType.ScoreChanged,
                PlayerGoldChangedEvent => MergeNetEventType.PlayerGoldChanged,
                _ => MergeNetEventType.None,
            };

            if (eventType == MergeNetEventType.None) return;

            BroadcastEvent(mergeGameEvent, eventType);
        }
        /// <summary>
        /// BroadcastEvent 함수를 처리합니다.
        /// </summary>

        private void BroadcastEvent(MergeGameEvent evt, MergeNetEventType eventType)
        {
            // 핵심 로직을 처리합니다.
            var pooled = ByteSerializer.SerializePooled(evt);
            var msg = new NetEventMessage
            {
                PlayerIndex = evt.PlayerIndex,
                Tick = evt.Tick,
                EventType = eventType,
                Payload = pooled.Segment,
            };

            foreach (var kv in _playerAndConnectionByConnId)
            {
                var conn = kv.Value.Item1;
                if (conn == null) continue;

                conn.Send(msg);
            }

            pooled.Dispose();
        }
        /// <summary>
        /// SendResultToPlayer 함수를 처리합니다.
        /// </summary>

        private void SendResultToPlayer(MergeCommandResult result, MergeNetCommandType commandType)
        {
            // 핵심 로직을 처리합니다.
            if (false == TryGetFindPlayerAndConnectionByUid(result.SenderUid, out var playerAndConnection))
            {
                return;
            }

            var pooled = ByteSerializer.SerializePooled(result);
            playerAndConnection.conn.Send(new NetCommandResultMessage
            {
                SenderUid = result.SenderUid,
                CommandType = commandType,
                Payload = pooled.Segment,
            });

            pooled.Dispose();
        }
        /// <summary>
        /// out 함수를 처리합니다.
        /// </summary>

        private bool TryGetFindPlayerAndConnectionByUid(long uid, out (NetworkConnectionToClient conn, MergeGamePlayer player) out_PlayerAndConnection)
        {
            // 핵심 로직을 처리합니다.
            foreach (var playerAndConnection in _playerAndConnectionByConnId)
            {
                var player = playerAndConnection.Value.Item2;
                if (player == null) continue;

                if (player.Uid != uid) continue;

                out_PlayerAndConnection = playerAndConnection.Value;
                return true;
            }

            out_PlayerAndConnection = default;
            return false;
        }
        /// <summary>
        /// BuildHost 함수를 처리합니다.
        /// </summary>

        private static MergeGameHost BuildHost()
        {
            // 핵심 로직을 처리합니다.
            var hostConfig = new MergeHostConfig()
                .WithMaxMonsterStack(100);

            var host = new MergeGameHost(hostConfig, new DevTowerDatabase());

            host.AddModule(new MapModule(), BuildMapConfig());
            host.AddModule(new RuleModule(), BuildRuleConfig(hostConfig));
            host.AddModule(new DifficultyModule(), BuildDifficultyConfig());

            host.InitializeModules();

            return host;
        }

        /// <summary>
        /// 맵 정보. 슬롯 개수,위치, 몬스터 경로 개수 위치 등
        /// 지금은 하드코딩으로. (이후에 맵 데이타를 추출해서 가져다 쓰도록 해야 함)
        /// </summary>
        /// <returns></returns>
        private static MapModuleConfig BuildMapConfig()
        {
            // 핵심 로직을 처리합니다.
            return new MapModuleConfig
            {
                MapId = DevHelperSet.DevIdHelper.DEV_DEFAULT_MAP_ID,

                SlotDefinitions = new List<SlotDefinition>
                {
                    new SlotDefinition(0, -5, 0, -5),
                    new SlotDefinition(1, 0, 0, -5),
                    new SlotDefinition(2, 5, 0, -5),
                    new SlotDefinition(3, -5, 0, 0),
                    new SlotDefinition(4, 0, 0, 0),
                    new SlotDefinition(5, 5, 0, 0),
                    new SlotDefinition(6, -5, 0, 5),
                    new SlotDefinition(7, 0, 0, 5),
                    new SlotDefinition(8, 5, 0, 5),
                },

                PathDefinitions = new List<PathDefinition>
                {
                    new PathDefinition(
                        pathIndex: 0,
                        waypoints: new List<Point3D>
                        {
                            new Point3D(-9f, 0f, -9f),
                            new Point3D(-9f, 0f, 9f),
                            new Point3D( 9f, 0f, 9f),
                            new Point3D( 9f, 0f, -9f),
                            new Point3D( -9f, 0f, -9f)
                        }
                    )
                }
            };
        }
        /// <summary>
        /// BuildRuleConfig 함수를 처리합니다.
        /// </summary>

        private static RuleModuleConfig BuildRuleConfig(MergeHostConfig hostConfig)
        {
            // 핵심 로직을 처리합니다.
            return new RuleModuleConfig
            {
                PlayerStartGold = hostConfig.PlayerStartGold,
                ScorePerGrade = hostConfig.ScorePerGrade,
                InitialUnitGrade = hostConfig.InitialTowerGrade,
                MaxUnitGrade = hostConfig.MaxTowerGrade,
            };
        }
        /// <summary>
        /// BuildDifficultyConfig 함수를 처리합니다.
        /// </summary>

        private static DifficultyModuleConfig BuildDifficultyConfig()
        {
            // 자동 진행용 단순 웨이브 설정입니다.
            return new DifficultyModuleConfig
            {

            };
        }

        /// <summary>
        /// 개발/테스트용 타워 데이터베이스입니다.
        /// 실서비스에서는 외부 데이터 소스(JSON/DB 등)로 교체할 수 있습니다.
        /// </summary>
        private sealed class DevTowerDatabase : ITowerDatabase
        {
            private static readonly long[] _towerIds =
            {
                DevHelperSet.DevIdHelper.DEV_TOWER_ID_RED,
                DevHelperSet.DevIdHelper.DEV_TOWER_ID_GREEN,
                DevHelperSet.DevIdHelper.DEV_TOWER_ID_BLUE
            };

            private readonly System.Random _random = new();

            private readonly Dictionary<long, TowerDefinition> _definitions = new()
            {
                {
                    DevHelperSet.DevIdHelper.DEV_TOWER_ID_RED,
                    new TowerDefinition
                    {
                        TowerId = DevHelperSet.DevIdHelper.DEV_TOWER_ID_RED,
                        InitialGrade = 1,
                        BaseAttackDamage = 10f,
                        BaseAttackSpeed = 1f,
                        BaseAttackRange = 10f,
                        AttackType = TowerAttackType.HitScan,
                        TargetingType = TowerTargetingType.Nearest,
                    }
                },
                {
                    DevHelperSet.DevIdHelper.DEV_TOWER_ID_GREEN,
                    new TowerDefinition
                    {
                        TowerId = DevHelperSet.DevIdHelper.DEV_TOWER_ID_GREEN,
                        InitialGrade = 1,
                        BaseAttackDamage = 15f,
                        BaseAttackSpeed = 0.7f,
                        BaseAttackRange = 12f,
                        AttackType = TowerAttackType.Projectile,
                        ProjectileType = ProjectileType.Direct,
                        ProjectileSpeed = 15f,
                        TargetingType = TowerTargetingType.Nearest,
                    }
                },
                {
                    DevHelperSet.DevIdHelper.DEV_TOWER_ID_BLUE,
                    new TowerDefinition
                    {
                        TowerId = DevHelperSet.DevIdHelper.DEV_TOWER_ID_BLUE,
                        InitialGrade = 1,
                        BaseAttackDamage = 20f,
                        BaseAttackSpeed = 0.5f,
                        BaseAttackRange = 8f,
                        AttackType = TowerAttackType.Projectile,
                        ProjectileType = ProjectileType.Throw,
                        ProjectileSpeed = 20f,
                        ThrowRadius = 2.5f,
                        TrapDelay = 0.5f,
                        TargetingType = TowerTargetingType.None,
                    }
                },
            };
            /// <summary>
            /// GetDefinition 함수를 처리합니다.
            /// </summary>

            public TowerDefinition GetDefinition(long towerId)
            {
                // 핵심 로직을 처리합니다.
                return _definitions.TryGetValue(towerId, out var definition) ? definition : null;
            }
            /// <summary>
            /// GetRandomIdForGrade 함수를 처리합니다.
            /// </summary>

            public long GetRandomIdForGrade(int grade)
            {
                // 핵심 로직을 처리합니다.
                return _towerIds[_random.Next(_towerIds.Length)];
            }
        }
    }
}


