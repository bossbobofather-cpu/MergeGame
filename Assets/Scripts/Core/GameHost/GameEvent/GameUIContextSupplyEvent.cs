namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// UI 而⑦뀓?ㅽ듃瑜??꾨떖?섎뒗 ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class GameUIContextSupplyEvent : SceneGameEventContext
    {
        /// <summary>
        /// ?꾨떖??UI 而⑦뀓?ㅽ듃?낅땲??
        /// </summary>
        public UIEventContext UIEventCtx { get; }
        /// <summary>
        /// GameUIContextSupplyEvent 함수를 처리합니다.
        /// </summary>

        public GameUIContextSupplyEvent(UIEventContext eventContext, object source)
            : base(source)
        {
            // 핵심 로직을 처리합니다.
            UIEventCtx = eventContext;
        }
    }
}
