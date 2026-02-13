using UnityEngine;

namespace MyProject.Common.GameView
{
    /// <summary>
    /// 공통 모듈 베이스 클래스입니다.
    /// </summary>
    public abstract class ViewModuleBase : MonoBehaviour, IViewModule
    {
        private IGameView _view;

        /// <summary>
        /// 연결된 게임 뷰입니다.
        /// </summary>
        public IGameView View => _view;

        /// <summary>
        /// 모듈 식별 키입니다.
        /// </summary>
        public virtual string ModuleKey => GetType().Name;

        /// <summary>
        /// 초기화 단계입니다.
        /// </summary>
        public void Initialize(IGameView view)
        {
            // 핵심 로직을 처리합니다.
            _view = view;
            OnInit();
        }

        /// <summary>
        /// 모드가 활성화될 때 호출됩니다.
        /// </summary>
        public void Startup()
        {
            // 핵심 로직을 처리합니다.
            OnStartup();
        }

        /// <summary>
        /// 모드가 비활성화될 때 호출됩니다.
        /// </summary>
        public void Shutdown()
        {
            // 핵심 로직을 처리합니다.
            OnShutdown();
        }

        /// <summary>
        /// 초기 구독 등록 등 준비 처리를 수행합니다.
        /// </summary>
        protected virtual void OnInit()
        {
            // 핵심 로직을 처리합니다.
        }

        /// <summary>
        /// 모듈 시작 시점 로직을 수행합니다.
        /// </summary>
        protected virtual void OnStartup()
        {
            // 핵심 로직을 처리합니다.
        }

        /// <summary>
        /// 모듈 종료 시점 정리 로직을 수행합니다.
        /// </summary>
        protected virtual void OnShutdown()
        {
            // 핵심 로직을 처리합니다.
        }
    }
}


