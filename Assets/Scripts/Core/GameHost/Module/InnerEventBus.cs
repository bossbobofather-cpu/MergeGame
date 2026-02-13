using System;
using System.Collections.Generic;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// ?대? ?대깽??踰꾩뒪 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// </summary>
    public interface IInnerEventBus
    {
        /// <summary>
        /// ?대깽?몃? 援щ룆?⑸땲??
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// ?대깽??援щ룆???댁젣?⑸땲??
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// ?대깽?몃? 諛쒗뻾?⑸땲??
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : IInnerEvent;

        /// <summary>
        /// 紐⑤뱺 援щ룆???댁젣?⑸땲??
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Host ?대? 紐⑤뱢 媛??듭떊???꾪븳 ?대깽??踰꾩뒪?낅땲??
    /// </summary>
    public sealed class InnerEventBus : IInnerEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            if (handler == null) return;

            lock (_lock)
            {
                var type = typeof(TEvent);
                if (!_handlers.TryGetValue(type, out var list))
                {
                    list = new List<Delegate>();
                    _handlers[type] = list;
                }
                list.Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            if (handler == null) return;

            lock (_lock)
            {
                var type = typeof(TEvent);
                if (_handlers.TryGetValue(type, out var list))
                {
                    list.Remove(handler);
                }
            }
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : IInnerEvent
        {
            if (eventData == null) return;

            List<Delegate> handlersCopy;
            lock (_lock)
            {
                var type = typeof(TEvent);
                if (!_handlers.TryGetValue(type, out var list) || list.Count == 0)
                {
                    return;
                }
                handlersCopy = new List<Delegate>(list);
            }

            foreach (var handler in handlersCopy)
            {
                try
                {
                    ((Action<TEvent>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    GameHostLog.LogError($"[InnerEventBus] ?대깽??泥섎━ ?ㅻ쪟 {typeof(TEvent).Name}: {ex}");
                }
            }
        }
        /// <summary>
        /// Clear 함수를 처리합니다.
        /// </summary>

        public void Clear()
        {
            // 핵심 로직을 처리합니다.
            lock (_lock)
            {
                _handlers.Clear();
            }
        }
    }
}
