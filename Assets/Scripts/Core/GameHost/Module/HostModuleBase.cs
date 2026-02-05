using System;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 모듈의 기본 구현입니다.
    /// </summary>
    public abstract class HostModuleBase : IHostModule
    {
        private bool _disposed;

        /// <summary>
        /// Host 컨텍스트입니다.
        /// </summary>
        protected IHostContext Context { get; private set; }

        /// <summary>
        /// 내부 이벤트 버스입니다.
        /// </summary>
        protected IInnerEventBus InnerEventBus => Context?.InnerEventBus;

        /// <inheritdoc />
        public abstract string ModuleId { get; }

        /// <inheritdoc />
        public virtual int Priority => 0;

        /// <inheritdoc />
        public virtual bool IsRequired => false;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public void Initialize(IHostContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            Context = context ?? throw new ArgumentNullException(nameof(context));
            OnInitialize();
            IsInitialized = true;
        }

        /// <inheritdoc />
        public void Tick(long tick, float deltaTime)
        {
            if (!IsInitialized) return;
            OnTick(tick, deltaTime);
        }

        /// <inheritdoc />
        public void Startup()
        {
            if (!IsInitialized) return;
            OnStartup();
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            if (!IsInitialized) return;
            OnShutdown();
        }

        /// <summary>
        /// 초기화 시 호출됩니다.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 매 틱 호출됩니다.
        /// </summary>
        protected virtual void OnTick(long tick, float deltaTime) { }

        /// <summary>
        /// 시작 시 호출됩니다.
        /// </summary>
        protected virtual void OnStartup() { }

        /// <summary>
        /// 정지 시 호출됩니다.
        /// </summary>
        protected virtual void OnShutdown() { }

        /// <summary>
        /// 리소스 해제 시 호출됩니다.
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// 외부 이벤트를 발행합니다.
        /// </summary>
        protected void PublishEvent<TEvent>(TEvent eventData) where TEvent : GameEventBase
        {
            Context?.PublishEvent(eventData);
        }

        /// <summary>
        /// 내부 이벤트를 발행합니다.
        /// </summary>
        protected void PublishInnerEvent<TEvent>(TEvent eventData) where TEvent : IInnerEvent
        {
            InnerEventBus?.Publish(eventData);
        }

        /// <summary>
        /// 내부 이벤트를 구독합니다.
        /// </summary>
        protected void SubscribeInnerEvent<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            InnerEventBus?.Subscribe(handler);
        }

        /// <summary>
        /// 내부 이벤트 구독을 해제합니다.
        /// </summary>
        protected void UnsubscribeInnerEvent<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            InnerEventBus?.Unsubscribe(handler);
        }

        /// <summary>
        /// 다른 모듈을 조회합니다.
        /// </summary>
        protected TModule GetModule<TModule>() where TModule : class, IHostModule
        {
            return Context?.GetModule<TModule>();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            OnShutdown();
            OnDispose();
        }
    }

    /// <summary>
    /// 설정을 가진 Host 모듈의 기본 구현입니다.
    /// </summary>
    /// <typeparam name="TConfig">설정 타입</typeparam>
    public abstract class HostModuleBase<TConfig> : HostModuleBase, IHostModule<TConfig>
        where TConfig : class, new()
    {
        /// <inheritdoc />
        public TConfig Config { get; private set; } = new();

        /// <inheritdoc />
        public void Configure(TConfig config)
        {
            Config = config ?? new TConfig();
            OnConfigure(Config);
        }

        /// <summary>
        /// 설정 적용 시 호출됩니다.
        /// </summary>
        protected virtual void OnConfigure(TConfig config) { }
    }
}
