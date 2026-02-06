using MyProject.Common.GameMode;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈의 베이스 클래스입니다.
    /// ModuleBase를 상속하고 IMergeViewModule을 구현합니다.
    /// </summary>
    public abstract class MergeViewModuleBase : ModuleBase, IMergeViewModule
    {
        /// <summary>
        /// 현재 연결된 MergeGameView입니다.
        /// </summary>
        protected MergeGameView GameView { get; private set; }

        protected override void OnInit()
        {
            GameView = Mode as MergeGameView;
        }

        /// <summary>
        /// Host 이벤트를 수신했을 때 호출됩니다.
        /// 필요한 이벤트만 override하여 처리합니다.
        /// </summary>
        public virtual void OnHostEvent(MergeHostEvent evt)
        {
        }

        /// <summary>
        /// 스냅샷이 갱신되었을 때 호출됩니다.
        /// </summary>
        public virtual void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
        }
    }
}
