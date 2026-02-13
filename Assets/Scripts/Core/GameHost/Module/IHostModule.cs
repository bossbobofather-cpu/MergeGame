using System;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 紐⑤뱢??怨듯넻 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// </summary>
    public interface IHostModule : IDisposable
    {
        /// <summary>
        /// 紐⑤뱢 ID?낅땲??
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// 紐⑤뱢 ?곗꽑?쒖쐞?낅땲?? ??쓣?섎줉 癒쇱? ?ㅽ뻾?⑸땲??
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// ?꾩닔 紐⑤뱢 ?щ??낅땲??
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// 紐⑤뱢??珥덇린?붾릺?덈뒗吏 ?щ??낅땲??
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 紐⑤뱢??珥덇린?뷀빀?덈떎.
        /// </summary>
        /// <param name="context">Host 而⑦뀓?ㅽ듃</param>
        void Initialize(IHostContext context);

        /// <summary>
        /// 留????몄텧?⑸땲??
        /// </summary>
        /// <param name="tick">?꾩옱 ??/param>
        /// <param name="deltaTime">?명? ???/param>
        void Tick(long tick, float deltaTime);

        /// <summary>
        /// 紐⑤뱢???쒖옉?⑸땲??
        /// </summary>
        void Startup();

        /// <summary>
        /// 紐⑤뱢???뺤??⑸땲??
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// ?ㅼ젙??媛吏?Host 紐⑤뱢 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// </summary>
    /// <typeparam name="TConfig">?ㅼ젙 ???/typeparam>
    public interface IHostModule<TConfig> : IHostModule
        where TConfig : class
    {
        /// <summary>
        /// 紐⑤뱢 ?ㅼ젙?낅땲??
        /// </summary>
        TConfig Config { get; }

        /// <summary>
        /// ?ㅼ젙???곸슜?⑸땲??
        /// </summary>
        void Configure(TConfig config);
    }
}
