namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 寃뚯엫 ?대깽?몄쓽 湲곕낯 而⑦뀓?ㅽ듃?낅땲??
    /// </summary>
    public abstract class GameEventContext
    {
        /// <summary>
        /// ?대깽??諛쒖떊?먯엯?덈떎.
        /// </summary>
        public object Source { get; }
        /// <summary>
        /// GameEventContext 함수를 처리합니다.
        /// </summary>

        protected GameEventContext(object source)
        {
            // 핵심 로직을 처리합니다.
            Source = source;
        }
    }

    /// <summary>
    /// ???ㅼ퐫?꾩뿉??泥섎━?섎뒗 ?대깽??而⑦뀓?ㅽ듃?낅땲??
    /// </summary>
    public abstract class SceneGameEventContext : GameEventContext
    {
        protected SceneGameEventContext(object source)
            : base(source)
        {
        }
    }

    /// <summary>
    /// ?꾩뿭 ?ㅼ퐫?꾩뿉??泥섎━?섎뒗 ?대깽??而⑦뀓?ㅽ듃?낅땲??
    /// </summary>
    public abstract class GlobalGameEventContext : GameEventContext
    {
        protected GlobalGameEventContext(object source)
            : base(source)
        {
        }
    }
}
