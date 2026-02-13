namespace Noname.GameHost.Module
{
    /// <summary>
    /// 紐⑤뱢??Host???묎렐?섍린 ?꾪븳 而⑦뀓?ㅽ듃 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// </summary>
    public interface IHostContext
    {
        /// <summary>
        /// ?꾩옱 ?깆엯?덈떎.
        /// </summary>
        long CurrentTick { get; }

        /// <summary>
        /// ?뚮젅?댁뼱 ?섏엯?덈떎.
        /// </summary>
        int PlayerCount { get; }

        /// <summary>
        /// ?대? ?대깽??踰꾩뒪?낅땲??
        /// </summary>
        IInnerEventBus InnerEventBus { get; }

        /// <summary>
        /// ?몃? ?대깽?몃? 諛쒗뻾?⑸땲??
        /// </summary>
        /// <typeparam name="TEvent">?대깽?????/typeparam>
        /// <param name="eventData">?대깽???곗씠??/param>
        void PublishEvent<TEvent>(TEvent eventData) where TEvent : GameEventBase;

        /// <summary>
        /// ?ㅻⅨ 紐⑤뱢??議고쉶?⑸땲??
        /// </summary>
        /// <typeparam name="TModule">紐⑤뱢 ???/typeparam>
        /// <returns>紐⑤뱢 ?몄뒪?댁뒪 ?먮뒗 null</returns>
        TModule GetModule<TModule>() where TModule : class, IHostModule;

        /// <summary>
        /// 紐⑤뱢 ID濡??ㅻⅨ 紐⑤뱢??議고쉶?⑸땲??
        /// </summary>
        /// <param name="moduleId">紐⑤뱢 ID</param>
        /// <returns>紐⑤뱢 ?몄뒪?댁뒪 ?먮뒗 null</returns>
        IHostModule GetModule(string moduleId);
    }
}
