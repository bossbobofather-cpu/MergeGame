using MyProject.Common.GameMode;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈의 공통 베이스 클래스입니다.
    /// Mode와 Host 사이의 이벤트/스냅샷 연결 지점을 제공합니다.
    /// </summary>
    public abstract class MergeViewModuleBase : ModuleBase, IMergeViewModule
    {
        /// <summary>
        /// 현재 모듈이 연결된 MergeGameMode 인스턴스입니다.
        /// </summary>
        protected MergeGameMode GameView { get; private set; }

        /// <summary>
        /// 모듈 초기화 시 Mode를 MergeGameMode로 캐스팅해 보관합니다.
        /// </summary>
        protected override void OnInit()
        {
            GameView = Mode as MergeGameMode;
        }

        /// <summary>
        /// Host 이벤트 수신 콜백입니다.
        /// 필요한 모듈만 override하여 사용합니다.
        /// </summary>
        public virtual void OnHostEvent(MergeHostEvent evt)
        {
        }

        /// <summary>
        /// 스냅샷 갱신 콜백입니다.
        /// 필요한 모듈만 override하여 사용합니다.
        /// </summary>
        public virtual void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
        }
    }
}