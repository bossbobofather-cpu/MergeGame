using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈 공통 인터페이스입니다.
    /// 네트워크 연결 상태와 Host 메시지를 모듈에 전달합니다.
    /// </summary>
    public interface IMergeViewModule
    {
        /// <summary>
        /// 서버 연결 완료 시 호출됩니다.
        /// </summary>
        void OnConnectedEvent();

        /// <summary>
        /// 서버 연결 해제 시 호출됩니다.
        /// </summary>
        void OnDisconnectedEvent();

        /// <summary>
        /// Host 이벤트 수신 시 호출됩니다.
        /// </summary>
        void OnEventMsg(MergeGameEvent evt);

        /// <summary>
        /// 커맨드 결과 수신 시 호출됩니다.
        /// </summary>
        void OnCommandResultMsg(MergeCommandResult result);

        /// <summary>
        /// 스냅샷 수신 시 호출됩니다.
        /// </summary>
        void OnSnapshotMsg(MergeHostSnapshot snapshot);
    }
}
