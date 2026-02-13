using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?대깽??踰꾩뒪 怨듭슜 ?명꽣?섏씠?ㅼ엯?덈떎.
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
