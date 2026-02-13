namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?몄뒪??而ㅻ㎤??泥섎━ 寃곌낵瑜??꾨떖?섎뒗 ?대깽?몄엯?덈떎.
    /// </summary>
    public class GameHostCommandResultEvent : SceneGameEventContext
    {
        /// <summary>
        /// 而ㅻ㎤??寃곌낵?낅땲??
        /// </summary>
        public GameCommandResultBase Result { get; }
        /// <summary>
        /// GameHostCommandResultEvent 함수를 처리합니다.
        /// </summary>

        public GameHostCommandResultEvent(object source, GameCommandResultBase result)
            : base(source)
        {
            // 핵심 로직을 처리합니다.
            Result = result;
        }
    }
}
