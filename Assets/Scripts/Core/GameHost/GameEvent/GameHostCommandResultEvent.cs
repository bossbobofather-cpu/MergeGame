namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public class GameHostCommandResultEvent : SceneGameEventContext
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameCommandResultBase Result { get; }
        /// <summary>
        /// GameHostCommandResultEvent 메서드입니다.
        /// </summary>

        public GameHostCommandResultEvent(object source, GameCommandResultBase result)
            : base(source)
        {
            Result = result;
        }
    }
}
