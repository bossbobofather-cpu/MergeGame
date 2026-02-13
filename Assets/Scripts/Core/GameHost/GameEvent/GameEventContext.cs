namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameEventContext
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public object Source { get; }
        /// <summary>
        /// GameEventContext 메서드입니다.
        /// </summary>

        protected GameEventContext(object source)
        {
            Source = source;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class SceneGameEventContext : GameEventContext
    {
        protected SceneGameEventContext(object source)
            : base(source)
        {
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GlobalGameEventContext : GameEventContext
    {
        protected GlobalGameEventContext(object source)
            : base(source)
        {
        }
    }
}
