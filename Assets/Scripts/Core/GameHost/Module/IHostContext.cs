namespace Noname.GameHost.Module
{
    /// <summary>
    /// 모듈이 Host에 접근하기 위한 컨텍스트 인터페이스입니다.
    /// </summary>
    public interface IHostContext
    {
        /// <summary>
        /// 현재 틱입니다.
        /// </summary>
        long CurrentTick { get; }

        /// <summary>
        /// 플레이어 수입니다.
        /// </summary>
        int PlayerCount { get; }

        /// <summary>
        /// 내부 이벤트 버스입니다.
        /// </summary>
        IInnerEventBus InnerEventBus { get; }

        /// <summary>
        /// 외부 이벤트를 발행합니다.
        /// </summary>
        /// <typeparam name="TEvent">이벤트 타입</typeparam>
        /// <param name="eventData">이벤트 데이터</param>
        void PublishEvent<TEvent>(TEvent eventData) where TEvent : GameEventBase;

        /// <summary>
        /// 다른 모듈을 조회합니다.
        /// </summary>
        /// <typeparam name="TModule">모듈 타입</typeparam>
        /// <returns>모듈 인스턴스 또는 null</returns>
        TModule GetModule<TModule>() where TModule : class, IHostModule;

        /// <summary>
        /// 모듈 ID로 다른 모듈을 조회합니다.
        /// </summary>
        /// <param name="moduleId">모듈 ID</param>
        /// <returns>모듈 인스턴스 또는 null</returns>
        IHostModule GetModule(string moduleId);
    }
}
