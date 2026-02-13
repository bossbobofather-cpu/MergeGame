using System;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 모듈의 공통 수명주기 인터페이스입니다.
    /// </summary>
    public interface IHostModule : IDisposable
    {
        /// <summary>
        /// 모듈 식별자입니다.
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// 틱 실행 우선순위입니다.
        /// 낮을수록 먼저 실행됩니다.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 필수 모듈 여부입니다.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// 초기화 완료 여부입니다.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 모듈을 초기화합니다.
        /// </summary>
        /// <param name="context">Host 컨텍스트입니다.</param>
        void Initialize(IHostContext context);

        /// <summary>
        /// 틱 단위 업데이트를 수행합니다.
        /// </summary>
        /// <param name="tick">현재 틱입니다.</param>
        /// <param name="deltaTime">이전 틱 대비 경과 시간(초)입니다.</param>
        void Tick(long tick, float deltaTime);

        /// <summary>
        /// 모듈 시작 시점에 호출됩니다.
        /// </summary>
        void Startup();

        /// <summary>
        /// 모듈 종료 시점에 호출됩니다.
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// 설정 객체를 받는 Host 모듈 인터페이스입니다.
    /// </summary>
    /// <typeparam name="TConfig">모듈 설정 타입입니다.</typeparam>
    public interface IHostModule<TConfig> : IHostModule
        where TConfig : class
    {
        /// <summary>
        /// 현재 모듈 설정입니다.
        /// </summary>
        TConfig Config { get; }

        /// <summary>
        /// 모듈 설정을 주입합니다.
        /// </summary>
        void Configure(TConfig config);
    }
}
