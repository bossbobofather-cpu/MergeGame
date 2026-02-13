namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 모듈이 Host와 상호작용할 때 사용하는 컨텍스트 인터페이스입니다.
    /// </summary>
    public interface IHostContext
    {
        /// <summary>
        /// 현재 Host 틱입니다.
        /// </summary>
        long CurrentTick { get; }

        /// <summary>
        /// 현재 플레이어 수입니다.
        /// </summary>
        int PlayerCount { get; }

        /// <summary>
        /// 모듈 내부 이벤트 버스입니다.
        /// </summary>
        IInnerEventBus InnerEventBus { get; }

        /// <summary>
        /// 외부(GameHost) 이벤트를 발행합니다.
        /// </summary>
        /// <typeparam name="TEvent">이벤트 타입입니다.</typeparam>
        /// <param name="eventData">발행할 이벤트 데이터입니다.</param>
        void PublishEvent<TEvent>(TEvent eventData) where TEvent : GameEventBase;

        /// <summary>
        /// 타입으로 모듈을 조회합니다.
        /// </summary>
        /// <typeparam name="TModule">조회할 모듈 타입입니다.</typeparam>
        /// <returns>조회된 모듈 또는 null입니다.</returns>
        TModule GetModule<TModule>() where TModule : class, IHostModule;

        /// <summary>
        /// 모듈 ID로 모듈을 조회합니다.
        /// </summary>
        /// <param name="moduleId">조회할 모듈 ID입니다.</param>
        /// <returns>조회된 모듈 또는 null입니다.</returns>
        IHostModule GetModule(string moduleId);
    }
}
