namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// UI ?대깽??醫낅쪟?낅땲??
    /// </summary>
    public enum UIEventType
    {
        Button_Click,
    }

    /// <summary>
    /// UI ?대깽??而⑦뀓?ㅽ듃 踰좎씠?ㅼ엯?덈떎.
    /// </summary>
    public abstract class UIEventContext
    {
        /// <summary>
        /// UI ?대깽??醫낅쪟?낅땲??
        /// </summary>
        public UIEventType EventType { get; }

        /// <summary>
        /// ?대깽??諛쒖떊?먯엯?덈떎.
        /// </summary>
        public object Source { get; }
        /// <summary>
        /// UIEventContext 함수를 처리합니다.
        /// </summary>

        protected UIEventContext(UIEventType eventType, object source)
        {
            // 핵심 로직을 처리합니다.
            EventType = eventType;
            Source = source;
        }
    }
}
