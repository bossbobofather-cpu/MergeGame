using System;
using System.Reflection;
using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame 네트워크 부트스트래퍼입니다.
    /// 싱글/멀티를 구분하지 않고 동일한 네트워크 경로(Mirror)로 시작합니다.
    /// </summary>
    public sealed class MergeGameBootstrapper : BootstrapperBase
    {
        private const string TransportTypeName = "Mirror.Transport, Mirror";
        private const string TelepathyTransportTypeName = "Mirror.TelepathyTransport, Mirror.Transports";
        private const string PortTransportTypeName = "Mirror.PortTransport, Mirror.Transports";

        private const string NetworkServerTypeName = "Mirror.NetworkServer, Mirror";
        private const string NetworkClientTypeName = "Mirror.NetworkClient, Mirror";
        private const string HostModeTypeName = "Mirror.HostMode, Mirror";

        private const string ServerAdapterTypeName = "MyProject.MergeGame.Unity.Network.MergeGameServerAdapter, Noname.MergeGame.Unity.Network";
        private const string ClientAdapterTypeName = "MyProject.MergeGame.Unity.Network.MergeGameClientAdapter, Noname.MergeGame.Unity.Network";
        private const string LogViewTypeName = "MyProject.MergeGame.Unity.Network.MergeGameNetworkLogView, Noname.MergeGame.Unity.Network";

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
        [Tooltip("Host 모드: local(호스트) 1명 + remote(maxConnections). 기본 0이면 싱글과 동일한 1인 실행입니다.")]
        [SerializeField] private int _maxConnections = 0;
        [SerializeField] private bool _autoStart = true;

        [Header("Components")]
        [SerializeField] private MonoBehaviour _transport;
        [SerializeField] private MonoBehaviour _serverAdapter;
        [SerializeField] private MonoBehaviour _clientAdapter;
        [SerializeField] private MonoBehaviour _logView;

        private bool _started;

        protected override void OnInit()
        {
            base.OnInit();

            Application.runInBackground = true;
            EnsureTransport();

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
            var transportType = Type.GetType(TransportTypeName);
            var telepathyTransportType = Type.GetType(TelepathyTransportTypeName);
            var portTransportType = Type.GetType(PortTransportTypeName);

            if (transportType == null || telepathyTransportType == null)
            {
                Debug.LogError("[MergeGameBootstrapper] Mirror Transport 타입을 찾지 못했습니다.");
                return;
            }

            if (_transport == null)
            {
                var existing = GetComponent(transportType);
                if (existing != null)
                {
                    _transport = existing as MonoBehaviour;
                }
            }

            if (_transport == null)
            {
                _transport = gameObject.AddComponent(telepathyTransportType) as MonoBehaviour;
            }

            if (_transport != null && portTransportType != null && portTransportType.IsInstanceOfType(_transport))
            {
                var portProperty = portTransportType.GetProperty("Port", BindingFlags.Public | BindingFlags.Instance);
                portProperty?.SetValue(_transport, (ushort)_port);
            }

            var activeProperty = transportType.GetProperty("active", BindingFlags.Public | BindingFlags.Static);
            activeProperty?.SetValue(null, _transport);
        }

        private void EnsureServerAdapter()
        {
            _serverAdapter = EnsureComponent(_serverAdapter, ServerAdapterTypeName);
        }

        private void EnsureClientAdapter()
        {
            _clientAdapter = EnsureComponent(_clientAdapter, ClientAdapterTypeName);
        }

        private void EnsureLogView()
        {
            _logView = EnsureComponent(_logView, LogViewTypeName);
        }

        private MonoBehaviour EnsureComponent(MonoBehaviour current, string typeName)
        {
            if (current != null)
            {
                return current;
            }

            var type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogError($"[MergeGameBootstrapper] 타입을 찾지 못했습니다: {typeName}");
                return null;
            }

            var existing = GetComponent(type);
            if (existing != null)
            {
                return existing as MonoBehaviour;
            }

            return gameObject.AddComponent(type) as MonoBehaviour;
        }

        private void StartAsHost()
        {
            var remoteCapacity = Mathf.Max(0, _maxConnections);
            var maxPlayers = Mathf.Clamp(remoteCapacity + 1, 1, 2);

            var networkServerType = Type.GetType(NetworkServerTypeName);
            var networkClientType = Type.GetType(NetworkClientTypeName);
            var hostModeType = Type.GetType(HostModeTypeName);

            networkServerType?.GetMethod("Listen", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { Mathf.Max(1, remoteCapacity + 1) });

            EnsureServerAdapter();
            InvokeInstanceMethod(_serverAdapter, "Configure", true);
            InvokeInstanceMethod(_serverAdapter, "SetMaxPlayers", maxPlayers);
            InvokeInstanceMethod(_serverAdapter, "Initialize");

            EnsureClientAdapter();
            InvokeInstanceMethod(_clientAdapter, "Initialize");

            networkClientType?.GetMethod("ConnectHost", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            hostModeType?.GetMethod("InvokeOnConnected", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);

            Debug.Log($"[MergeGameBootstrapper] Started as HOST (players={maxPlayers}, remoteCapacity={remoteCapacity})");
        }

        private void StartAsServerOnly()
        {
            var connectionCapacity = Mathf.Max(1, _maxConnections);
            var maxPlayers = Mathf.Clamp(connectionCapacity, 1, 2);

            var networkServerType = Type.GetType(NetworkServerTypeName);
            networkServerType?.GetMethod("Listen", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { connectionCapacity });

            EnsureServerAdapter();
            InvokeInstanceMethod(_serverAdapter, "Configure", false);
            InvokeInstanceMethod(_serverAdapter, "SetMaxPlayers", maxPlayers);
            InvokeInstanceMethod(_serverAdapter, "Initialize");

            Debug.Log($"[MergeGameBootstrapper] Started as SERVER (players={maxPlayers}, connectionCapacity={connectionCapacity})");
        }

        private void StartAsClient()
        {
            var networkClientType = Type.GetType(NetworkClientTypeName);

            EnsureClientAdapter();
            InvokeInstanceMethod(_clientAdapter, "Initialize");

            networkClientType?.GetMethod("Connect", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null)?.Invoke(null, new object[] { _address });

            Debug.Log($"[MergeGameBootstrapper] Started as CLIENT (addr={_address}, port={_port})");
        }

        private static void InvokeInstanceMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null)
            {
                return;
            }

            var type = instance.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            method?.Invoke(instance, args);
        }

        private static bool TryDetectParrelSyncClone(out bool isClone)
        {
            isClone = false;

            try
            {
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
            var networkClientType = Type.GetType(NetworkClientTypeName);
            var networkServerType = Type.GetType(NetworkServerTypeName);

            var clientActiveProperty = networkClientType?.GetProperty("active", BindingFlags.Public | BindingFlags.Static);
            var serverActiveProperty = networkServerType?.GetProperty("active", BindingFlags.Public | BindingFlags.Static);

            var isClientActive = clientActiveProperty?.GetValue(null) is bool b1 && b1;
            var isServerActive = serverActiveProperty?.GetValue(null) is bool b2 && b2;

            if (isClientActive)
            {
                networkClientType?.GetMethod("Disconnect", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
                networkClientType?.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }

            if (isServerActive)
            {
                networkServerType?.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
        }
    }
}
