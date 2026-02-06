using System;
using System.Reflection;
using Mirror;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// Mirror 네트워크 부트스트래퍼입니다.
    /// - Host 모드: Server + Local Client
    /// - Client 모드: Remote Client
    /// 
    /// GameHost 로직은 서버에서만 실행하고,
    /// 클라이언트는 Snapshot/Event를 받아서 로그(뷰)만 처리합니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MergeGameNetworkBootstrapper : MonoBehaviour
    {
        public enum RunMode
        {
            Host,
            ServerOnly,
            Client,
        }

        [Header("Network")]
        [SerializeField] private RunMode _runMode = RunMode.Host;
        [SerializeField] private bool _autoSelectRunModeByParrelSync = true;
        [SerializeField] private RunMode _cloneRunMode = RunMode.Client;
        [SerializeField] private string _address = "localhost";
        [SerializeField] private ushort _port = 7777;
        [Tooltip("Host 모드: local(호스트) 1명 + remote(maxConnections)")]
        [SerializeField] private int _maxConnections = 1;
        [SerializeField] private bool _autoStart = true;

        [Header("Components")]
        [SerializeField] private Transport _transport;
        [SerializeField] private MergeGameServerAdapter _serverAdapter;
        [SerializeField] private MergeGameClientAdapter _clientAdapter;
        [SerializeField] private MergeGameNetworkLogView _logView;

        private bool _started;

        private void Awake()
        {
            // 에디터에서 포커스가 빠져도 호스트가 멈추지 않게 합니다.
            Application.runInBackground = true;

            EnsureTransport();
        }

        private void Start()
        {
            if (_autoStart)
            {
                StartNetwork();
            }
        }

        public void StartNetwork()
        {
            if (_started)
            {
                return;
            }

            // ParrelSync(클론 실행) 환경에서는 Host/Client를 자동으로 분기합니다.
            // - 원본(메인) 인스턴스: Host
            // - 클론 인스턴스: Client
            // ParrelSync가 프로젝트에 없으면 아무 동작도 하지 않습니다.
            if (_autoSelectRunModeByParrelSync && TryDetectParrelSyncClone(out var isClone) && isClone)
            {
                _runMode = _cloneRunMode;
            }

            _started = true;

            if (_runMode == RunMode.Host)
            {
                StartAsHost();
                EnsureLogView();
            }
            else if (_runMode == RunMode.ServerOnly)
            {
                StartAsServerOnly();
            }
            else
            {
                StartAsClient();
                EnsureLogView();
            }
        }

        private void EnsureTransport()
        {
            if (_transport == null)
            {
                _transport = GetComponent<Transport>();
            }

            if (_transport == null)
            {
                _transport = gameObject.AddComponent<TelepathyTransport>();
            }

            if (_transport is PortTransport portTransport)
            {
                portTransport.Port = _port;
            }

            Transport.active = _transport;
        }

        private void EnsureClientAdapter()
        {
            if (_clientAdapter == null)
            {
                _clientAdapter = GetComponent<MergeGameClientAdapter>();
            }

            if (_clientAdapter == null)
            {
                _clientAdapter = gameObject.AddComponent<MergeGameClientAdapter>();
            }
        }

        private void EnsureServerAdapter()
        {
            if (_serverAdapter == null)
            {
                _serverAdapter = GetComponent<MergeGameServerAdapter>();
            }

            if (_serverAdapter == null)
            {
                _serverAdapter = gameObject.AddComponent<MergeGameServerAdapter>();
            }
        }

        private void EnsureLogView()
        {
            if (_logView == null)
            {
                _logView = GetComponent<MergeGameNetworkLogView>();
            }

            if (_logView == null)
            {
                _logView = gameObject.AddComponent<MergeGameNetworkLogView>();
            }
        }

        private void StartAsHost()
        {
            // 1) Server 시작
            NetworkServer.Listen(_maxConnections + 1);

            // 2) ServerAdapter 준비 (연결 이벤트 핸들러 등록)
            EnsureServerAdapter();
            _serverAdapter.Configure(reserveSlot0ForLocalHost: true);
            _serverAdapter.Initialize();

            // 3) ClientAdapter 준비 (Connected 이벤트를 놓치지 않도록 ConnectHost 전에 등록)
            EnsureClientAdapter();
            _clientAdapter.Initialize();

            // 4) Local Client 연결
            NetworkClient.ConnectHost();

            // 5) HostMode 연결 이벤트 트리거
            HostMode.InvokeOnConnected();

            Debug.Log("[MergeGameNetworkBootstrapper] Started as HOST (Server + Local Client)");
        }

        private void StartAsServerOnly()
        {
            // Dedicated Server 모드: Server만 실행하고 Local Client는 붙이지 않습니다.
            NetworkServer.Listen(_maxConnections);

            EnsureServerAdapter();
            _serverAdapter.Configure(reserveSlot0ForLocalHost: false);
            _serverAdapter.Initialize();

            Debug.Log("[MergeGameNetworkBootstrapper] Started as SERVER (Dedicated)");
        }

        private void StartAsClient()
        {
            // 1) ClientAdapter 준비 (Connected 이벤트를 놓치지 않도록 Connect 전에 등록)
            EnsureClientAdapter();
            _clientAdapter.Initialize();

            // 2) Remote Client 연결
            NetworkClient.Connect(_address);

            Debug.Log($"[MergeGameNetworkBootstrapper] Started as CLIENT (addr={_address}, port={_port})");
        }

        private static bool TryDetectParrelSyncClone(out bool isClone)
        {
            isClone = false;

            try
            {
                // ParrelSync가 설치된 경우: ParrelSync.ClonesManager.IsClone()
                // (asmdef 유무/어셈블리명 차이를 피하기 위해 리플렉션으로 검색)
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Type clonesManagerType = null;

                for (var i = 0; i < assemblies.Length; i++)
                {
                    clonesManagerType = assemblies[i].GetType("ParrelSync.ClonesManager", throwOnError: false);
                    if (clonesManagerType != null)
                    {
                        break;
                    }
                }

                if (clonesManagerType == null)
                {
                    return false;
                }

                var method = clonesManagerType.GetMethod("IsClone", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    return false;
                }

                var value = method.Invoke(null, null);
                if (value is bool b)
                {
                    isClone = b;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void OnDestroy()
        {
            // 네트워크 종료
            if (NetworkClient.active)
            {
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
            }

            if (NetworkServer.active)
            {
                NetworkServer.Shutdown();
            }
        }
    }
}


