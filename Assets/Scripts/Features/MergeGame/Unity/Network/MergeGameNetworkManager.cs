using System;
using Mirror;
using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    public class MergeGameNetworkManager : NetworkManager, IManager
    {
        [SerializeField] private long _userId = 1;
        [SerializeField] MergeGameServerAdapter _serverAdapterPrefab;
        MergeGameServerAdapter _serverAdapterInstance;

        public void Initialize()
        {
            User.UserId = _userId;
        }

        //서버만 호출 된다.
        public override void OnStartServer()
        {
            base.OnStartServer();

            _serverAdapterInstance = GameObject.Instantiate(_serverAdapterPrefab);
            Debug.Assert(_serverAdapterInstance != null, "Failed Instantiate MergeGameServerAdapter");
            if(_serverAdapterInstance) _serverAdapterInstance.Initialize();
        }

        //서버가 중지될 때 호출 됩니다.
        public override void OnStopServer()
        {
            if (_serverAdapterInstance != null)
            {
                // 어댑터 인스턴스를 파괴하여 리소스 누수를 방지합니다.
                // 어댑터의 OnDestroy에서 이벤트 구독 해제 등이 처리됩니다.
                Destroy(_serverAdapterInstance.gameObject);
                _serverAdapterInstance = null;
            }
            
            base.OnStopServer();
        }

        /// <summary>
        /// 클라이언트가 플레이어 추가를 요청할 때 호출됩니다.
        /// 현재 아키텍처에서는 Mirror의 자동 플레이어 생성을 사용하지 않으므로,
        /// 기본 동작을 막기 위해 이 메소드를 비워둡니다.
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // base.OnServerAddPlayer(conn); // 기본 프리팹 생성 로직을 호출하지 않음
        }
    }
}