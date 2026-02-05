namespace Noname.GameHost.Module
{
    /// <summary>
    /// 내부 이벤트 마커 인터페이스입니다.
    /// Host 내부 모듈 간 통신에 사용됩니다.
    /// </summary>
    public interface IInnerEvent
    {
    }

    /// <summary>
    /// 내부 이벤트 기본 클래스입니다.
    /// </summary>
    public abstract class InnerEventBase : IInnerEvent
    {
        /// <summary>
        /// 이벤트가 발생한 틱입니다.
        /// </summary>
        public long Tick { get; }

        protected InnerEventBase(long tick)
        {
            Tick = tick;
        }
    }
}
