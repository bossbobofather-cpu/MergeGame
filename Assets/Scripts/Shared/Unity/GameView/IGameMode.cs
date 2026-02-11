using System;
using System.Collections.Generic;
using Noname.GameHost.GameEvent;

namespace MyProject.Common.GameView
{
    /// <summary>
    /// 게임 모드가 제공하는 공용 인터페이스입니다.
    /// </summary>
    public interface IGameView
    {
        /// <summary>
        /// 현재 씬 스코프 이벤트 버스입니다.
        /// </summary>
        GameEventBus.Scope SceneBus { get; }

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        IReadOnlyList<IViewModule> Modules { get; }

        /// <summary>
        /// 지정한 타입의 모듈을 조회합니다.
        /// </summary>
        T GetViewModule<T>() where T : class, IViewModule;

        /// <summary>
        /// 이벤트 구독을 등록합니다.
        /// </summary>
        void Subscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext;

        /// <summary>
        /// 이벤트 구독을 해제합니다.
        /// </summary>
        void Unsubscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext;

        /// <summary>
        /// 이벤트를 발행합니다.
        /// </summary>
        void Publish<TEventContext>(TEventContext context) where TEventContext : GameEventContext;
    }
}


