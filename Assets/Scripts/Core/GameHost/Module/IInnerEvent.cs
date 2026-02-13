namespace Noname.GameHost.Module
{
    /// <summary>
    /// ?대? ?대깽??留덉빱 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// Host ?대? 紐⑤뱢 媛??듭떊???ъ슜?⑸땲??
    /// </summary>
    public interface IInnerEvent
    {
    }

    /// <summary>
    /// ?대? ?대깽??湲곕낯 ?대옒?ㅼ엯?덈떎.
    /// </summary>
    public abstract class InnerEventBase : IInnerEvent
    {
        /// <summary>
        /// ?대깽?멸? 諛쒖깮???깆엯?덈떎.
        /// </summary>
        public long Tick { get; }
        /// <summary>
        /// InnerEventBase 함수를 처리합니다.
        /// </summary>

        protected InnerEventBase(long tick)
        {
            // 핵심 로직을 처리합니다.
            Tick = tick;
        }
    }
}
