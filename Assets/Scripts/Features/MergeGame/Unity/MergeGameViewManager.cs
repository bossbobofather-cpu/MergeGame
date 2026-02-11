using System;
using Mirror;
using MyProject.Common.Bootstrap;
using MyProject.Common.GameView;
using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using MyProject.MergeGame.Unity.Network;
using Noname.GameHost;
using Noname.GameHost.GameEvent;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View Manager 입니다.
    /// - 서버에서 전달된 Snapshot/Event를 수신합니다.
    /// - 입력은 CommandMsg로 서버에 전송합니다.
    /// </summary>
    public class MergeGameViewManager : GameViewManager
    {
        private readonly Color _logColor = new Color(0f, 0f, 0f, 0.6f);
        private readonly Color _errorColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
        private readonly Color _startColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        private readonly Color _spawnColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        private readonly Color _mergeColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        private readonly Color _scoreColor = new Color(0.4f, 0.8f, 0.4f, 0.6f);
        private readonly Color _gameOverColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);
        private bool _readySent;
        private int _assignedPlayerIndex = -1;

        /// <summary>
        /// 서버에서 할당받은 플레이어 인덱스입니다. 아직 할당되지 않은 경우 -1입니다.
        /// </summary>
        public int AssignedPlayerIndex => _assignedPlayerIndex;

        /// <summary>
        /// 로컬 유저 ID입니다.
        /// </summary>
        public long LocalUserId => User.UserId;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            NetworkClient.UnregisterHandler<NetCommandResultMessage>();
            NetworkClient.RegisterHandler<NetCommandResultMessage>(OnCommandResultMsg);
            NetworkClient.UnregisterHandler<NetEventMessage>();
            NetworkClient.RegisterHandler<NetEventMessage>(OnEventMsg);
            NetworkClient.UnregisterHandler<NetSnapshotMessage>();
            NetworkClient.RegisterHandler<NetSnapshotMessage>(OnSnapshotMsg);

            // Authenticator가 있으면 인증 완료 이벤트를 구독하고, 없으면 기존 연결 이벤트를 사용합니다.
            // 이것이 인증 흐름을 올바르게 처리하는 방법입니다.
            if (NetworkManager.singleton.authenticator != null)
            {
                NetworkManager.singleton.authenticator.OnClientAuthenticated.AddListener(HandleConnected);
            }
            else
            {
                // Authenticator가 없는 경우를 위한 폴백
                NetworkClient.OnConnectedEvent += HandleConnected;
            }
            
            NetworkClient.OnDisconnectedEvent += HandleDisconnected;
        }

        protected override void OnDestroy()
        {
            if (NetworkManager.singleton != null && NetworkManager.singleton.authenticator != null)
            {
                NetworkManager.singleton.authenticator.OnClientAuthenticated.RemoveListener(HandleConnected);
            }

            // Authenticator 없는 폴백 경로도 항상 정리합니다.
            NetworkClient.OnConnectedEvent -= HandleConnected;
            NetworkClient.OnDisconnectedEvent -= HandleDisconnected;

            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// 서버로 커맨드를 전송합니다.
        /// </summary>
        public void SendCommand(NetCommandMessage msg)
        {
            if (!NetworkClient.isConnected)
            {
                Debug.LogWarning("[MergeGameView] 서버에 연결되어 있지 않습니다.");
                return;
            }

            NetworkClient.Send(msg);
        }

        /// <summary>
        /// 게임 준비(Ready) 커맨드를 서버로 전송합니다.
        /// (이미 전송했다면 무시합니다.)
        /// </summary>
        public void SendReady()
        {
            if (_readySent)
            {
                return;
            }

            if (!NetworkClient.isConnected)
            {
                return;
            }

            _readySent = true;

            ReadyMergeGameCommand cmd = new ReadyMergeGameCommand(User.UserId);
            var pooled = ByteSerializer.SerializePooled(cmd);
            SendCommand(new NetCommandMessage 
            {
                SenderUid = User.UserId,
                CommandType = MergeNetCommandType.ReadyGame,
                Payload = pooled.Segment,
            });

            pooled.Dispose();
        }        
        private void HandleConnected()
        {
            Debug.Log("[MergeGameView] Connected");

            foreach(var module in Modules)
            {
                var mergeViewMoudle = module as IMergeViewModule; 
                if(mergeViewMoudle == null) continue;

                mergeViewMoudle.OnConnectedEvent();
            }
        }
        private void HandleDisconnected()
        {
            Debug.Log("[MergeGameView] Disconnected");

            _readySent = false;
            _assignedPlayerIndex = -1;

            foreach(var module in Modules)
            {
                var mergeViewMoudle = module as IMergeViewModule;
                if(mergeViewMoudle == null) continue;

                mergeViewMoudle.OnDisconnectedEvent();
            }
        }

        /// <summary>
        /// MergeGameCommand를 직렬화하여 서버로 전송합니다.
        /// </summary>
        public void SendCommand(MergeGameCommand command, MergeNetCommandType commandType)
        {
            if (!NetworkClient.isConnected)
            {
                Debug.LogWarning("[MergeGameView] 서버에 연결되어 있지 않습니다.");
                return;
            }

            var pooled = ByteSerializer.SerializePooled(command);
            NetworkClient.Send(new NetCommandMessage
            {
                SenderUid = LocalUserId,
                CommandType = commandType,
                Payload = pooled.Segment,
            });

            pooled.Dispose();
        }

        /// <summary>
        /// 서버로 부터 전달 받은 커맨드의 결과
        /// </summary>
        private void OnCommandResultMsg(NetCommandResultMessage msg)
        {
            MergeCommandResult result = msg.CommandType switch
            {
                MergeNetCommandType.ReadyGame => ReadyMergeGameResult.ReadFrom(msg.Payload),
                MergeNetCommandType.SpawnTower => SpawnTowerResult.ReadFrom(msg.Payload),
                MergeNetCommandType.MergeTower => MergeTowerResult.ReadFrom(msg.Payload),
                MergeNetCommandType.ExitGame => ExitMergeGameResult.ReadFrom(msg.Payload),
                MergeNetCommandType.InjectMonsters => InjectMonstersResult.ReadFrom(msg.Payload),
                _ => null,
            };

            if (result == null) return;

            foreach(var module in Modules)
            {
                var mergeViewMoudle = module as IMergeViewModule;
                if(mergeViewMoudle == null) continue;

                mergeViewMoudle.OnCommandResultMsg(result);
            }
        }

        /// <summary>
        /// 서버로 부터 전달 받은 이벤트
        /// </summary>
        private void OnEventMsg(NetEventMessage msg)
        {
            MergeGameEvent evt = DeserializeEvent(msg);
            if (evt == null) return;

            if (msg.EventType == MergeNetEventType.PlayerAssigned || msg.EventType == MergeNetEventType.ConnectedInfo)
            {
                _assignedPlayerIndex = evt.PlayerIndex;
            }

            foreach(var module in Modules)
            {
                var mergeViewMoudle = module as IMergeViewModule;
                if(mergeViewMoudle == null) continue;

                mergeViewMoudle.OnEventMsg(evt);
            }
        }

        private static MergeGameEvent DeserializeEvent(NetEventMessage msg)
        {
            return msg.EventType switch
            {
                MergeNetEventType.PlayerAssigned => ConnectedInfoEvent.ReadFrom(msg.Payload),
                MergeNetEventType.ConnectedInfo => ConnectedInfoEvent.ReadFrom(msg.Payload),
                MergeNetEventType.GameStarted => GameStartedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.GameOver => GameOverEvent.ReadFrom(msg.Payload),
                MergeNetEventType.MapInitialized => MapInitializedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.TowerSpawned => TowerSpawnedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.TowerMerged => TowerMergedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.TowerRemoved => TowerRemovedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.TowerAttacked => TowerAttackedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.EffectTriggered => EffectTriggeredEvent.ReadFrom(msg.Payload),
                MergeNetEventType.MonsterSpawned => MonsterSpawnedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.MonsterDamaged => MonsterDamagedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.MonsterDied => MonsterDiedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.MonsterMoved => MonsterMovedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.DifficultyStepChangedEvent => DifficultyStepChangedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.ScoreChanged => ScoreChangedEvent.ReadFrom(msg.Payload),
                MergeNetEventType.PlayerGoldChanged => PlayerGoldChangedEvent.ReadFrom(msg.Payload),
                _ => null,
            };
        }

        /// <summary>
        /// 서버로 부터 전달 받은 스냅샷
        /// </summary>
        private void OnSnapshotMsg(NetSnapshotMessage msg)
        {
            var snapshot = MergeHostSnapshot.ReadFrom(msg.Payload);
            if (snapshot == null) return;

            foreach(var module in Modules)
            {
                var mergeViewMoudle = module as IMergeViewModule;
                if(mergeViewMoudle == null) continue;

                mergeViewMoudle.OnSnapshotMsg(snapshot);
            }
        }
    }
}

