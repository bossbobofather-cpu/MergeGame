namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?몄뒪?몄뿉??諛쒖깮???대깽?몃? ?꾨떖?섎뒗 ?대깽?몄엯?덈떎.
    /// </summary>
    public class GameHostEventRaisedEvent : SceneGameEventContext
    {
        /// <summary>
        /// ?몄뒪???대깽???곗씠?곗엯?덈떎.
        /// </summary>
        public GameEventBase EventData { get; }
        /// <summary>
        /// GameHostEventRaisedEvent 함수를 처리합니다.
        /// </summary>

        public GameHostEventRaisedEvent(object source, GameEventBase eventData)
            : base(source)
        {
            // 핵심 로직을 처리합니다.
            EventData = eventData;
        }
    }
}
