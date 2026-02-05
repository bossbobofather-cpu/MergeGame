using System;
using System.Collections.Generic;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// 내부 이벤트 버스 인터페이스입니다.
    /// </summary>
    public interface IInnerEventBus
    {
        /// <summary>
        /// 이벤트를 구독합니다.
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// 이벤트 구독을 해제합니다.
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// 이벤트를 발행합니다.
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : IInnerEvent;

        /// <summary>
        /// 모든 구독을 해제합니다.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Host 내부 모듈 간 통신을 위한 이벤트 버스입니다.
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
                    GameHostLog.LogError($"[InnerEventBus] 이벤트 처리 오류 {typeof(TEvent).Name}: {ex}");
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }
    }
}
