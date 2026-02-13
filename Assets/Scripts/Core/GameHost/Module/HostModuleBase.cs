using System;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 紐⑤뱢??湲곕낯 援ы쁽?낅땲??
    /// </summary>
    public abstract class HostModuleBase : IHostModule
    {
        private bool _disposed;

        /// <summary>
        /// Host 而⑦뀓?ㅽ듃?낅땲??
        /// </summary>
        protected IHostContext Context { get; private set; }

        /// <summary>
        /// ?대? ?대깽??踰꾩뒪?낅땲??
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
            // 핵심 로직을 처리합니다.
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
            // 핵심 로직을 처리합니다.
            if (!IsInitialized) return;
            OnTick(tick, deltaTime);
        }

        /// <inheritdoc />
        public void Startup()
        {
            // 핵심 로직을 처리합니다.
            if (!IsInitialized) return;
            OnStartup();
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            // 핵심 로직을 처리합니다.
            if (!IsInitialized) return;
            OnShutdown();
        }

        /// <summary>
        /// 珥덇린?????몄텧?⑸땲??
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 留????몄텧?⑸땲??
        /// </summary>
        protected virtual void OnTick(long tick, float deltaTime) { }

        /// <summary>
        /// ?쒖옉 ???몄텧?⑸땲??
        /// </summary>
        protected virtual void OnStartup() { }

        /// <summary>
        /// ?뺤? ???몄텧?⑸땲??
        /// </summary>
        protected virtual void OnShutdown() { }

        /// <summary>
        /// 由ъ냼???댁젣 ???몄텧?⑸땲??
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// ?몃? ?대깽?몃? 諛쒗뻾?⑸땲??
        /// </summary>
        protected void PublishEvent<TEvent>(TEvent eventData) where TEvent : GameEventBase
        {
            Context?.PublishEvent(eventData);
        }

        /// <summary>
        /// ?대? ?대깽?몃? 諛쒗뻾?⑸땲??
        /// </summary>
        protected void PublishInnerEvent<TEvent>(TEvent eventData) where TEvent : IInnerEvent
        {
            InnerEventBus?.Publish(eventData);
        }

        /// <summary>
        /// ?대? ?대깽?몃? 援щ룆?⑸땲??
        /// </summary>
        protected void SubscribeInnerEvent<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            InnerEventBus?.Subscribe(handler);
        }

        /// <summary>
        /// ?대? ?대깽??援щ룆???댁젣?⑸땲??
        /// </summary>
        protected void UnsubscribeInnerEvent<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            InnerEventBus?.Unsubscribe(handler);
        }

        /// <summary>
        /// ?ㅻⅨ 紐⑤뱢??議고쉶?⑸땲??
        /// </summary>
        protected TModule GetModule<TModule>() where TModule : class, IHostModule
        {
            return Context?.GetModule<TModule>();
        }
        /// <summary>
        /// Dispose 함수를 처리합니다.
        /// </summary>

        public void Dispose()
        {
            // 핵심 로직을 처리합니다.
            if (_disposed) return;
            _disposed = true;

            OnShutdown();
            OnDispose();
        }
    }

    /// <summary>
    /// ?ㅼ젙??媛吏?Host 紐⑤뱢??湲곕낯 援ы쁽?낅땲??
    /// </summary>
    /// <typeparam name="TConfig">?ㅼ젙 ???/typeparam>
    public abstract class HostModuleBase<TConfig> : HostModuleBase, IHostModule<TConfig>
        where TConfig : class, new()
    {
        /// <inheritdoc />
        public TConfig Config { get; private set; } = new();

        /// <inheritdoc />
        public void Configure(TConfig config)
        {
            // 핵심 로직을 처리합니다.
            Config = config ?? new TConfig();
            OnConfigure(Config);
        }

        /// <summary>
        /// ?ㅼ젙 ?곸슜 ???몄텧?⑸땲??
        /// </summary>
        protected virtual void OnConfigure(TConfig config) { }
    }
}
