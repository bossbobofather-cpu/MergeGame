using System;
using System.Collections.Generic;

namespace Noname.GameHost.Module
{
    /// <summary>
    /// Host 내부 이벤트 버스 인터페이스입니다.
    /// </summary>
    public interface IInnerEventBus
    {
        /// <summary>
        /// 이벤트 핸들러를 등록합니다.
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// 이벤트 핸들러를 해제합니다.
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent;

        /// <summary>
        /// 이벤트를 발행합니다.
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : IInnerEvent;

        /// <summary>
        /// 등록된 모든 핸들러를 제거합니다.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 단일 스레드/락 기반 내부 이벤트 버스 구현입니다.
    /// </summary>
    public sealed class InnerEventBus : IInnerEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IInnerEvent
        {
            if (handler == null)
            {
                return;
            }

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
            if (handler == null)
            {
                return;
            }

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
            if (eventData == null)
            {
                return;
            }

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

        /// <summary>
        /// 등록된 핸들러를 모두 초기화합니다.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }
    }
}
