using MyProject.Common.GameView;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 모듈의 공통 베이스 클래스입니다.
    /// </summary>
    public abstract class MergeViewModuleBase : ViewModuleBase, IMergeViewModule
    {
        /// <summary>
        /// 현재 모듈이 연결된 MergeGameView 인스턴스입니다.
        /// </summary>
        protected MergeGameViewManager GameView { get; private set; }

        /// <summary>
        /// 모듈 초기화 시 View를 MergeGameView로 캐스팅해 보관합니다.
        /// </summary>
        protected override void OnInit()
        {
            // 핵심 로직을 처리합니다.
            GameView = View as MergeGameViewManager;
        }
        /// <summary>
        /// OnEventMsg 함수를 처리합니다.
        /// </summary>

        public virtual void OnEventMsg(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
        }
        /// <summary>
        /// OnSnapshotMsg 함수를 처리합니다.
        /// </summary>

        public virtual void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            // 핵심 로직을 처리합니다.
        }
        /// <summary>
        /// OnCommandResultMsg 함수를 처리합니다.
        /// </summary>

        public virtual void OnCommandResultMsg(MergeCommandResult result)
        {
            // 핵심 로직을 처리합니다.
        }
        /// <summary>
        /// OnConnectedEvent 함수를 처리합니다.
        /// </summary>

        public virtual void OnConnectedEvent()
        {
            // 핵심 로직을 처리합니다.
        }
        /// <summary>
        /// OnDisconnectedEvent 함수를 처리합니다.
        /// </summary>

        public virtual void OnDisconnectedEvent()
        {
            // 핵심 로직을 처리합니다.
        }

        /// <summary>
        /// 로컬 플레이어가 할당되었는지 여부입니다.
        /// </summary>
        protected bool HasAssignedPlayer =>
            GameView != null && GameView.AssignedPlayerIndex >= 0;

        /// <summary>
        /// 입력된 플레이어 인덱스가 로컬 플레이어와 같은지 검사합니다.
        /// </summary>
        protected bool IsMyPlayer(int playerIndex)
        {
            // 핵심 로직을 처리합니다.
            return HasAssignedPlayer && GameView.AssignedPlayerIndex == playerIndex;
        }

        /// <summary>
        /// 이벤트가 로컬 플레이어용인지 검사합니다.
        /// </summary>
        protected bool IsMyEvent(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
            return evt != null && IsMyPlayer(evt.PlayerIndex);
        }

        /// <summary>
        /// 스냅샷이 로컬 플레이어용인지 검사합니다.
        /// </summary>
        protected bool IsMySnapshot(MergeHostSnapshot snapshot)
        {
            // 핵심 로직을 처리합니다.
            return snapshot != null && IsMyPlayer(snapshot.PlayerIndex);
        }
    }
}
