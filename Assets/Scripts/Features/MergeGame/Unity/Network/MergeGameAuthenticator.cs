using Mirror;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// MergeGame 전용 Mirror 인증기입니다.
    /// </summary>
    public class MergeGameAuthenticator : NetworkAuthenticator
    {
        /// <summary>
        /// 서버 인증 핸들러를 등록합니다.
        /// </summary>
        public override void OnStartServer()
        {
            // 수동 인증 흐름을 사용하므로 requireAuthentication=false로 핸들러를 등록합니다.
            NetworkServer.RegisterHandler<NetAuthenticateMessage>(OnAuthMessage, false);
        }

        /// <summary>
        /// 클라이언트 인증 응답 핸들러를 등록합니다.
        /// </summary>
        public override void OnStartClient()
        {
            // 서버의 인증 성공/실패 응답을 받기 위한 핸들러를 등록합니다.
            NetworkClient.RegisterHandler<NetAuthResponseMessage>(OnAuthResponse, false);
        }

        /// <summary>
        /// 서버에서 인증 메시지를 처리합니다.
        /// </summary>
        private void OnAuthMessage(NetworkConnectionToClient conn, NetAuthenticateMessage msg)
        {
            // UserId가 0보다 크면 유효한 클라이언트로 간주합니다.
            if (msg.UserId > 0)
            {
                conn.authenticationData = msg.UserId;

                // 인증 성공 응답을 먼저 보낸 뒤 서버 인증 완료를 알립니다.
                conn.Send(new NetAuthResponseMessage { Success = true, Message = "Success" });
                ServerAccept(conn);
                return;
            }

            // 잘못된 UserId면 인증을 거부합니다.
            Debug.LogError($"Server Authentication failed. UserId={msg.UserId}");
            ServerReject(conn);
        }

        /// <summary>
        /// 클라이언트에서 인증 응답을 처리합니다.
        /// </summary>
        private void OnAuthResponse(NetAuthResponseMessage msg)
        {
            // 서버 응답 결과에 따라 클라이언트 인증 상태를 갱신합니다.
            if (msg.Success)
            {
                ClientAccept();
                return;
            }

            Debug.LogError($"Client Authentication failed: {msg.Message}");
            ClientReject();
        }

        /// <summary>
        /// 클라이언트 인증 요청을 전송합니다.
        /// </summary>
        public override void OnClientAuthenticate()
        {
            // 현재 런타임 UserId를 포함해 인증 요청을 서버로 보냅니다.
            NetworkClient.Send(new NetAuthenticateMessage
            {
                UserId = User.UserId,
            });
        }
    }
}
