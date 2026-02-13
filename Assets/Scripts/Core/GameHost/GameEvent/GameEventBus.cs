using System;
using System.Collections.Generic;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?꾩뿭/???ㅼ퐫?꾨? ?④퍡 吏?먰븯???뺤쟻 ?대깽??踰꾩뒪?낅땲??
    /// Unity ?섍꼍 ?놁씠 Scene Handle(int) 湲곕컲?쇰줈 ?숈옉?⑸땲??
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
        /// ?꾩옱 Active Scene Handle???대떦?섎뒗 ?ㅼ퐫?꾨? 諛섑솚?⑸땲??
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
        /// ?대깽????낆뿉 留욌뒗 ?ㅼ퐫?꾩뿉 ?몃뱾?щ? ?깅줉?⑸땲??
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Subscribe(handler);
        }

        /// <summary>
        /// ?대깽????낆뿉 留욌뒗 ?ㅼ퐫?꾩뿉???몃뱾?щ? ?댁젣?⑸땲??
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            var scope = ResolveScope(typeof(TEvent));
            scope?.Unsubscribe(handler);
        }

        /// <summary>
        /// ?대깽????낆뿉 留욌뒗 ?ㅼ퐫?꾩뿉 ?대깽?몃? 諛쒗뻾?⑸땲??
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
        /// ForScene 함수를 처리합니다.
        /// </summary>

        public static Scope ForScene(int handle)
        {
            // 핵심 로직을 처리합니다.
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
        /// SetActiveScene 함수를 처리합니다.
        /// </summary>

        public static void SetActiveScene(int handle)
        {
            // 핵심 로직을 처리합니다.
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
        /// Active Scene Handle???꾩쭅 ?ㅼ젙?섏? ?딆븯???뚮쭔 ?ㅼ젙?⑸땲??
        /// </summary>
        public static bool TrySetActiveScene(int handle)
        {
            // 핵심 로직을 처리합니다.
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
        /// ClearGlobal 함수를 처리합니다.
        /// </summary>

        public static void ClearGlobal()
        {
            // 핵심 로직을 처리합니다.
            GlobalScope.Clear();
        }
        /// <summary>
        /// ResetAll 함수를 처리합니다.
        /// </summary>

        public static void ResetAll()
        {
            // 핵심 로직을 처리합니다.
            GlobalScope.Clear();
            SceneScopes.Clear();
            _activeSceneHandle = 0;
            _missingActiveSceneLogged = false;
            UnknownScopeLogged.Clear();
        }
        /// <summary>
        /// ClearScene 함수를 처리합니다.
        /// </summary>

        public static void ClearScene(int handle)
        {
            // 핵심 로직을 처리합니다.
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
        /// IsValidHandle 함수를 처리합니다.
        /// </summary>

        private static bool IsValidHandle(int handle)
        {
            // 핵심 로직을 처리합니다.
            return handle != 0;
        }
        /// <summary>
        /// LogMissingActiveScene 함수를 처리합니다.
        /// </summary>

        private static void LogMissingActiveScene()
        {
            // 핵심 로직을 처리합니다.
            if (_missingActiveSceneLogged)
            {
                return;
            }

            _missingActiveSceneLogged = true;
            GameHostLog.LogWarning("Active Scene???ㅼ젙?섏? ?딆븘 Scene Event Bus瑜??ъ슜?????놁뒿?덈떎.");
        }
        /// <summary>
        /// ResolveScope 함수를 처리합니다.
        /// </summary>

        private static Scope ResolveScope(Type eventType)
        {
            // 핵심 로직을 처리합니다.
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
        /// LogUnknownScope 함수를 처리합니다.
        /// </summary>

        private static void LogUnknownScope(Type eventType)
        {
            // 핵심 로직을 처리합니다.
            if (!UnknownScopeLogged.Add(eventType))
            {
                return;
            }

            GameHostLog.LogWarning(
                $"{eventType.Name} ?대깽?몄쓽 ?ㅼ퐫?꾧? ?뺤쓽?섏? ?딆븯?듬땲?? SceneGameEventContext ?먮뒗 GlobalGameEventContext瑜??ъ슜?섏꽭??");
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
            /// Clear 함수를 처리합니다.
            /// </summary>

            public void Clear()
            {
                // 핵심 로직을 처리합니다.
                _handlers.Clear();
            }
        }
    }
}
