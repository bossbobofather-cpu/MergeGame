namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public class GameHostEventRaisedEvent : SceneGameEventContext
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameEventBase EventData { get; }
        /// <summary>
        /// GameHostEventRaisedEvent 메서드입니다.
        /// </summary>

        public GameHostEventRaisedEvent(object source, GameEventBase eventData)
            : base(source)
        {
            EventData = eventData;
        }
    }
}
