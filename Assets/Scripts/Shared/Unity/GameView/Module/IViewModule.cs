namespace MyProject.Common.GameView
{
    /// <summary>
    /// 게임 모듈의 공통 동작을 정의하는 인터페이스입니다.
    /// </summary>
    public interface IViewModule
    {
        /// <summary>
        /// 모드와 연결되는 초기화 단계입니다.
        /// </summary>
        void Initialize(IGameView view);

        /// <summary>
        /// 모드가 활성화될 때 호출됩니다.
        /// </summary>
        void Startup();

        /// <summary>
        /// 모드가 비활성화될 때 호출됩니다.
        /// </summary>
        void Shutdown();
    }
}


