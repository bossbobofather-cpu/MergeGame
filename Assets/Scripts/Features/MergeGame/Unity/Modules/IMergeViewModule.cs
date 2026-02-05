using MyProject.Common.GameMode;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈 인터페이스입니다.
    /// 호스트 이벤트와 스냅샷을 수신할 수 있습니다.
    /// </summary>
    public interface IMergeViewModule : IModule
    {
        /// <summary>
        /// 호스트 이벤트 수신 시 호출됩니다.
        /// </summary>
        void OnHostEvent(MergeHostEvent evt);

        /// <summary>
        /// 스냅샷 갱신 시 호출됩니다.
        /// </summary>
        void OnSnapshotUpdated(MergeHostSnapshot snapshot);
    }
}
