using Mirror;
using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// MergeGame 전용 Mirror 네트워크 매니저입니다.
    /// 서버 시작/종료 시 서버 어댑터 수명주기를 함께 관리합니다.
    /// </summary>
    public class MergeGameNetworkManager : NetworkManager, IManager
    {
        [SerializeField] private MergeGameServerAdapter _serverAdapterPrefab;
        private MergeGameServerAdapter _serverAdapterInstance;

        /// <summary>
        /// 런타임 환경에 맞춰 UserId를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            User.InitializeFromRuntime();
        }

        /// <summary>
        /// 서버 시작 시 서버 어댑터를 생성하고 초기화합니다.
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            _serverAdapterInstance = Instantiate(_serverAdapterPrefab);
            Debug.Assert(_serverAdapterInstance != null, "Failed Instantiate MergeGameServerAdapter");
            if (_serverAdapterInstance != null)
            {
                _serverAdapterInstance.Initialize();
            }
        }

        /// <summary>
        /// 서버 종료 시 서버 어댑터를 정리합니다.
        /// </summary>
        public override void OnStopServer()
        {
            if (_serverAdapterInstance != null)
            {
                Destroy(_serverAdapterInstance.gameObject);
                _serverAdapterInstance = null;
            }

            base.OnStopServer();
        }

        /// <summary>
        /// Mirror 기본 플레이어 오브젝트 자동 생성을 사용하지 않습니다.
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // base.OnServerAddPlayer(conn);
        }
    }
}
