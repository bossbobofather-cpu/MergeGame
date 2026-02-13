namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 내부 모듈 간 통신에 사용하는 이벤트 마커 인터페이스입니다.
    /// </summary>
    public interface IInnerEvent
    {
    }

    /// <summary>
    /// 내부 이벤트 기본 타입입니다.
    /// </summary>
    public abstract class InnerEventBase : IInnerEvent
    {
        /// <summary>
        /// 이벤트가 생성된 틱입니다.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// 내부 이벤트를 생성합니다.
        /// </summary>
        protected InnerEventBase(long tick)
        {
            Tick = tick;
        }
    }
}
