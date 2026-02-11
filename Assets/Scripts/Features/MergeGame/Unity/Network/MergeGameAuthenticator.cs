using Mirror;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    public class MergeGameAuthenticator : NetworkAuthenticator
    {
        public override void OnStartServer()
        {
            // 인증 전 메시지이므로 requireAuthentication=false 필수
            NetworkServer.RegisterHandler<NetAuthenticateMessage>(OnAuthMessage, false);
        }

        public override void OnStartClient()
        {
            // 서버로부터 인증 성공/실패 응답을 받기 위한 핸들러
            NetworkClient.RegisterHandler<NetAuthResponseMessage>(OnAuthResponse, false);
        }

        void OnAuthMessage(NetworkConnectionToClient conn, NetAuthenticateMessage msg)
        {
            if (msg.UserId > 0)
            {
                conn.authenticationData = msg.UserId;

                // 클라
                // 1. 클라이언트에게 인증 성공을 먼저 알립니다. (더 안전한 순서)
                conn.Send(new NetAuthResponseMessage { Success = true, Message = "Success" });

                // 2. 그 다음 서버에서 인증 완료 처리를 합니다.
                ServerAccept(conn);
            }
            else
            {
                Debug.LogError($"Server Authentication failed");
                ServerReject(conn);
            }
        }

        void OnAuthResponse(NetAuthResponseMessage msg)
        {
            if (msg.Success)
            {
                // 인증 성공! 클라이언트 측 인증 완료 처리
                ClientAccept();
            }
            else
            {
                Debug.LogError($"Client Authentication failed: {msg.Message}");
                ClientReject();
            }
        }

        public override void OnClientAuthenticate()
        {
            // 이 시점엔 이미 transport 연결은 된 상태라 보통 isConnected 체크 불필요
            NetworkClient.Send(new NetAuthenticateMessage
            {
                UserId = User.UserId,
            });
        }
    }
}
