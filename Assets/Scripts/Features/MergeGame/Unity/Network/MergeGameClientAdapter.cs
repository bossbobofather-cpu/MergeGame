using System;
using Mirror;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 클라이언트 측 네트워크 어댑터입니다.
    /// - 서버에서 전달된 Snapshot/Event를 수신합니다.
    /// - 입력은 CommandMsg로 서버에 전송합니다.
    /// </summary>
    public sealed class MergeGameClientAdapter : MonoBehaviour
    {
        [Header("Auto")]
        [SerializeField] private bool _autoSendReadyOnConnected = true;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<EventMsg> EventReceived;
        public event Action<SnapshotMsg> SnapshotReceived;

        private bool _initialized;
        private bool _readySent;

        /// <summary>
        /// 클라이언트 어댑터를 초기화합니다.
        /// (핸들러 등록은 Connect 이전에 호출하는 것을 권장합니다.)
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // 인증 흐름을 아직 구성하지 않았으므로 requireAuthentication=false
            NetworkClient.RegisterHandler<EventMsg>(OnEventMsg, requireAuthentication: false);
            NetworkClient.RegisterHandler<SnapshotMsg>(OnSnapshotMsg, requireAuthentication: false);

            NetworkClient.OnConnectedEvent += HandleConnected;
            NetworkClient.OnDisconnectedEvent += HandleDisconnected;
        }

        private void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            NetworkClient.OnConnectedEvent -= HandleConnected;
            NetworkClient.OnDisconnectedEvent -= HandleDisconnected;
        }

        /// <summary>
        /// 서버로 커맨드를 전송합니다.
        /// </summary>
        public void SendCommand(CommandMsg msg)
        {
            if (!NetworkClient.isConnected)
            {
                Debug.LogWarning("[MergeGameClientAdapter] 서버에 연결되어 있지 않습니다.");
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

            SendCommand(new CommandMsg
            {
                CommandType = MergeNetCommandType.Ready,
                PlayerIndex = 0,
                SenderUid = 0,
                Int0 = 0,
                Int1 = 0,
                Str0 = null,
            });
        }

        private void HandleConnected()
        {
            Debug.Log("[MergeGameClientAdapter] Connected");

            if (_autoSendReadyOnConnected)
            {
                SendReady();
            }

            Connected?.Invoke();
        }

        private void HandleDisconnected()
        {
            Debug.Log("[MergeGameClientAdapter] Disconnected");
            _readySent = false;
            Disconnected?.Invoke();
        }

        private void OnEventMsg(EventMsg msg)
        {
            EventReceived?.Invoke(msg);
        }

        private void OnSnapshotMsg(SnapshotMsg msg)
        {
            SnapshotReceived?.Invoke(msg);
        }
    }
}
