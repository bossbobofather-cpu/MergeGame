using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈 인터페이스입니다.
    /// 서버 연결 및 해제, 호스트 이벤트와 스냅샷을 수신할 수 있습니다.
    /// </summary>
    public interface IMergeViewModule
    {
        /// <summary>
        /// 서버 연결 시 호출 됩니다.
        /// </summary>
        void OnConnectedEvent();

        /// <summary>
        /// 서버 연결 해제 시 호출됩니다.
        /// </summary>
        void OnDisconnectedEvent();

        /// <summary>
        /// 호스트 이벤트 수신 시 호출됩니다.
        /// </summary>
        void OnEventMsg(MergeGameEvent evt);

        /// <summary>
        /// 커맨드 결과 수신 시 호출됩니다.
        /// </summary>
        void OnCommandResultMsg(MergeCommandResult result);

        /// <summary>
        /// 스냅샷 갱신 시 호출됩니다.
        /// </summary>
        void OnSnapshotMsg(MergeHostSnapshot snapshot);
    }
}