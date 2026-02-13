using System;
using System.Collections.Generic;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public static class GameEventBus
    {
        private static readonly Scope GlobalScope = new();
        private static readonly Dictionary<int, Scope> SceneScopes = new();
        private static int _activeSceneHandle;
        private static bool _missingActiveSceneLogged;
        private static readonly HashSet<Type> UnknownScopeLogged = new();

        public static Scope Global => GlobalScope;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static Scope Scene
        {
            get
            {
                if (!IsValidHandle(_activeSceneHandle))
                {
                    LogMissingActiveScene();
                    return null;
                }

                return ForScene(_activeSceneHandle);
            }
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Subscribe(handler);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Unsubscribe(handler);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
        {
            if (context == null)
            {
                return;
            }

            var scope = ResolveScope(context.GetType());
            scope?.Publish(context);
        }
        /// <summary>
        /// ForScene 메서드입니다.
        /// </summary>

        public static Scope ForScene(int handle)
        {
            if (!IsValidHandle(handle))
            {
                LogMissingActiveScene();
                return null;
            }

            if (!SceneScopes.TryGetValue(handle, out var scope))
            {
                scope = new Scope();
                SceneScopes.Add(handle, scope);
            }

            return scope;
        }
        /// <summary>
        /// SetActiveScene 메서드입니다.
        /// </summary>

        public static void SetActiveScene(int handle)
        {
            if (!IsValidHandle(handle))
            {
                _activeSceneHandle = 0;
                return;
            }

            _activeSceneHandle = handle;
            _missingActiveSceneLogged = false;
            ForScene(handle);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static bool TrySetActiveScene(int handle)
        {
            if (IsValidHandle(_activeSceneHandle))
            {
                return false;
            }

            if (!IsValidHandle(handle))
            {
                return false;
            }

            SetActiveScene(handle);
            return true;
        }
        /// <summary>
        /// ClearGlobal 메서드입니다.
        /// </summary>

        public static void ClearGlobal()
        {
            GlobalScope.Clear();
        }
        /// <summary>
        /// ResetAll 메서드입니다.
        /// </summary>

        public static void ResetAll()
        {
            GlobalScope.Clear();
            SceneScopes.Clear();
            _activeSceneHandle = 0;
            _missingActiveSceneLogged = false;
            UnknownScopeLogged.Clear();
        }
        /// <summary>
        /// ClearScene 메서드입니다.
        /// </summary>

        public static void ClearScene(int handle)
        {
            if (!IsValidHandle(handle))
            {
                return;
            }

            if (SceneScopes.TryGetValue(handle, out var scope))
            {
                scope.Clear();
                SceneScopes.Remove(handle);
            }

            if (_activeSceneHandle == handle)
            {
                _activeSceneHandle = 0;
            }
        }
        /// <summary>
        /// IsValidHandle 메서드입니다.
        /// </summary>

        private static bool IsValidHandle(int handle)
        {
            return handle != 0;
        }
        /// <summary>
        /// LogMissingActiveScene 메서드입니다.
        /// </summary>

        private static void LogMissingActiveScene()
        {
            if (_missingActiveSceneLogged)
            {
                return;
            }

            _missingActiveSceneLogged = true;
            GameHostLog.LogWarning("Active Scene이 설정되지 않아 Scene Event Bus를 사용할 수 없습니다.");
        }
        /// <summary>
        /// ResolveScope 메서드입니다.
        /// </summary>

        private static Scope ResolveScope(Type eventType)
        {
            if (eventType == null)
            {
                return null;
            }

            if (typeof(GlobalGameEventContext).IsAssignableFrom(eventType))
            {
                return GlobalScope;
            }

            if (typeof(SceneGameEventContext).IsAssignableFrom(eventType))
            {
                return Scene;
            }

            LogUnknownScope(eventType);
            return Scene;
        }
        /// <summary>
        /// LogUnknownScope 메서드입니다.
        /// </summary>

        private static void LogUnknownScope(Type eventType)
        {
            if (!UnknownScopeLogged.Add(eventType))
            {
                return;
            }

            GameHostLog.LogWarning(
                $"{eventType.Name} 이벤트의 스코프가 정의되지 않았습니다. SceneGameEventContext 또는 GlobalGameEventContext를 사용하세요.");
        }

        public sealed class Scope : IEventBus<GameEventContext>
        {
            private readonly Dictionary<Type, List<Delegate>> _handlers = new();

            public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
            {
                if (handler == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers.Add(key, handlers);
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                }
            }

            public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
            {
                if (handler == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    return;
                }

                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _handlers.Remove(key);
                }
            }

            public void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
            {
                if (context == null)
                {
                    return;
                }

                var key = typeof(TEvent);
                if (!_handlers.TryGetValue(key, out var handlers))
                {
                    return;
                }

                var snapshot = handlers.ToArray();
                for (var i = 0; i < snapshot.Length; i++)
                {
                    if (snapshot[i] is Action<TEvent> handler)
                    {
                        handler(context);
                    }
                }
            }
            /// <summary>
            /// Clear 메서드입니다.
            /// </summary>

            public void Clear()
            {
                _handlers.Clear();
            }
        }
    }
}
