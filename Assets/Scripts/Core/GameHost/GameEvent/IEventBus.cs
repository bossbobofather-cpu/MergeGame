using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public interface IEventBus<TEventBase>
        where TEventBase : class
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : TEventBase;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : TEventBase;
        void Publish<TEvent>(TEvent context) where TEvent : TEventBase;
        void Clear();
    }
}
