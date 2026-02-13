namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public enum UIEventType
    {
        Button_Click,
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class UIEventContext
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public UIEventType EventType { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public object Source { get; }
        /// <summary>
        /// UIEventContext 메서드입니다.
        /// </summary>

        protected UIEventContext(UIEventType eventType, object source)
        {
            EventType = eventType;
            Source = source;
        }
    }
}
