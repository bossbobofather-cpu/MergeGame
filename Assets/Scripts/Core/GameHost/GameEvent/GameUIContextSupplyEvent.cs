namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class GameUIContextSupplyEvent : SceneGameEventContext
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public UIEventContext UIEventCtx { get; }
        /// <summary>
        /// GameUIContextSupplyEvent 메서드입니다.
        /// </summary>

        public GameUIContextSupplyEvent(UIEventContext eventContext, object source)
            : base(source)
        {
            UIEventCtx = eventContext;
        }
    }
}
