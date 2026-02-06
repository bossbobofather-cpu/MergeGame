using MyProject.MergeGame;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 네트워크로 수신한 Event/Snapshot을 로그로 출력하는 View입니다.
    /// (현재 단계에서는 UI/렌더링 대신 로그만 남기는 것을 목표로 합니다.)
    /// </summary>
    public sealed class MergeGameNetworkLogView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MergeGameClientAdapter _clientAdapter;

        [Header("Options")]
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _logSnapshots = true;

        [Tooltip("Snapshot 로그 최소 출력 간격(초)")]
        [SerializeField] private float _minSnapshotLogInterval = 0.5f;

        private bool _subscribed;
        private float _snapshotCooldown;
        private bool _hasPendingSnapshot;
        private SnapshotMsg _pendingSnapshot;

        private void Awake()
        {
            EnsureClientAdapter();
            TrySubscribe();
        }

        private void OnEnable()
        {
            EnsureClientAdapter();
            TrySubscribe();
        }

        private void Start()
        {
            // AddComponent 순서에 따라 Awake 시점에 어댑터를 못 찾는 경우가 있어 Start에서 한 번 더 시도합니다.
            EnsureClientAdapter();
            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!_logSnapshots)
            {
                return;
            }

            if (_snapshotCooldown > 0f)
            {
                _snapshotCooldown -= Time.deltaTime;
            }

            if (_hasPendingSnapshot && _snapshotCooldown <= 0f)
            {
                _hasPendingSnapshot = false;
                _snapshotCooldown = Mathf.Max(0f, _minSnapshotLogInterval);

                var msg = _pendingSnapshot;

                // Enum으로 캐스팅해서 읽기 쉽게 출력합니다.
                var sessionPhase = (MergeSessionPhase)msg.SessionPhase;
                var wavePhase = (WavePhase)msg.WavePhase;

                Debug.Log(
                    $"[P{msg.PlayerIndex}] [스냅샷] tick={msg.Tick} session={sessionPhase} wave={msg.WaveNumber}({wavePhase}) " +
                    $"monsters={msg.MonsterCount} chars={msg.TowerCount} usedSlots={msg.UsedSlotCount} " +
                    $"p0={msg.SampleMonsterProgress0:0.00} p1={msg.SampleMonsterProgress1:0.00}"
                );
            }
        }

        private void EnsureClientAdapter()
        {
            if (_clientAdapter != null)
            {
                return;
            }

            _clientAdapter = GetComponent<MergeGameClientAdapter>();
        }

        private void TrySubscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (_clientAdapter == null)
            {
                return;
            }

            _subscribed = true;

            _clientAdapter.Connected += HandleConnected;
            _clientAdapter.Disconnected += HandleDisconnected;
            _clientAdapter.EventReceived += HandleEvent;
            _clientAdapter.SnapshotReceived += HandleSnapshot;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;

            if (_clientAdapter != null)
            {
                _clientAdapter.Connected -= HandleConnected;
                _clientAdapter.Disconnected -= HandleDisconnected;
                _clientAdapter.EventReceived -= HandleEvent;
                _clientAdapter.SnapshotReceived -= HandleSnapshot;
            }
        }

        private void HandleConnected()
        {
            Debug.Log("[MergeGameNetworkLogView] Connected");
        }

        private void HandleDisconnected()
        {
            Debug.Log("[MergeGameNetworkLogView] Disconnected");
        }

        private void HandleEvent(EventMsg msg)
        {
            if (!_logEvents)
            {
                return;
            }

            if (string.IsNullOrEmpty(msg.Text))
            {
                return;
            }

            Debug.Log($"[P{msg.PlayerIndex}] (t={msg.Tick}) {msg.Text}");
        }

        private void HandleSnapshot(SnapshotMsg msg)
        {
            if (!_logSnapshots)
            {
                return;
            }

            _pendingSnapshot = msg;
            _hasPendingSnapshot = true;
        }
    }
}


