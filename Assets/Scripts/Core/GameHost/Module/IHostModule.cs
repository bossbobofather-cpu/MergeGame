using System;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 모듈의 공통 인터페이스입니다.
    /// </summary>
    public interface IHostModule : IDisposable
    {
        /// <summary>
        /// 모듈 ID입니다.
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// 모듈 우선순위입니다. 낮을수록 먼저 실행됩니다.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 필수 모듈 여부입니다.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// 모듈이 초기화되었는지 여부입니다.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 모듈을 초기화합니다.
        /// </summary>
        /// <param name="context">Host 컨텍스트</param>
        void Initialize(IHostContext context);

        /// <summary>
        /// 매 틱 호출됩니다.
        /// </summary>
        /// <param name="tick">현재 틱</param>
        /// <param name="deltaTime">델타 타임</param>
        void Tick(long tick, float deltaTime);

        /// <summary>
        /// 모듈을 시작합니다.
        /// </summary>
        void Startup();

        /// <summary>
        /// 모듈을 정지합니다.
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// 설정을 가진 Host 모듈 인터페이스입니다.
    /// </summary>
    /// <typeparam name="TConfig">설정 타입</typeparam>
    public interface IHostModule<TConfig> : IHostModule
        where TConfig : class
    {
        /// <summary>
        /// 모듈 설정입니다.
        /// </summary>
        TConfig Config { get; }

        /// <summary>
        /// 설정을 적용합니다.
        /// </summary>
        void Configure(TConfig config);
    }
}
