using System;
using System.Collections.Generic;
using MyProject.Common.Bootstrap;
using Noname.GameHost.GameEvent;
using UnityEngine;

namespace MyProject.Common.GameView
{
    /// <summary>
    /// 게임 뷰 베이스 클래스입니다.
    /// 구체적인 게임 뷰는 이 클래스를 상속해서 구현합니다.
    /// </summary>
    public abstract class GameViewManager : MonoBehaviour, IGameView, IManager
    {
        /// <summary>
        /// 모듈 프리팹 목록입니다.
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _modulePrefabs = new();

        /// <summary>
        /// 생성된 모듈 인스턴스 목록입니다.
        /// </summary>
        private readonly List<MonoBehaviour> _moduleInstances = new();

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        private readonly List<IViewModule> _modules = new();

        /// <summary>
        /// 초기화 완료 여부입니다.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Startup 완료 여부입니다.
        /// </summary>
        private bool _started;

        /// <summary>
        /// 등록된 모듈 목록입니다.
        /// </summary>
        public IReadOnlyList<IViewModule> Modules => _modules;

        /// <summary>
        /// 씬 스코프 이벤트 버스입니다.
        /// </summary>
        public GameEventBus.Scope SceneBus => GameEventBus.Scene;

        /// <summary>
        /// 모듈을 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            // 핵심 로직을 처리합니다.
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // 모듈 목록을 구성하고 각 모듈을 초기화합니다.
            BuildModuleList();
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Initialize(this);
            }

            StartupModule();
            OnInitialize();
        }

        /// <summary>
        /// 모듈 Startup을 호출합니다.
        /// </summary>
        protected void StartupModule()
        {
            // 핵심 로직을 처리합니다.
            if (_started)
            {
                return;
            }

            _started = true;
            // 각 모듈의 Startup을 호출합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Startup();
            }

            OnStartup();
        }

        /// <summary>
        /// 모듈 Shutdown을 호출합니다.
        /// </summary>
        protected void ShutdownModule()
        {
            // 핵심 로직을 처리합니다.
            if (!_started)
            {
                return;
            }

            _started = false;

            // 각 모듈의 Shutdown을 호출합니다.
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].Shutdown();
            }

            OnShutdown();
        }

        /// <summary>
        /// 지정한 타입의 모듈을 반환합니다.
        /// </summary>
        public T GetViewModule<T>() where T : class, IViewModule
        {
            for (var i = 0; i < _modules.Count; i++)
            {
                if (_modules[i] is T module)
                {
                    return module;
                }
            }

            return null;
        }

        public void Subscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext
        {
            GameEventBus.Subscribe(handler);
        }

        public void Unsubscribe<TEventContext>(Action<TEventContext> handler) where TEventContext : GameEventContext
        {
            GameEventBus.Unsubscribe(handler);
        }

        public void Publish<TEventContext>(TEventContext context) where TEventContext : GameEventContext
        {
            GameEventBus.Publish(context);
        }
        /// <summary>
        /// BuildModuleList 함수를 처리합니다.
        /// </summary>

        private void BuildModuleList()
        {
            // 핵심 로직을 처리합니다.
            _modules.Clear();
            _moduleInstances.Clear();

            if (_modulePrefabs.Count == 0)
            {
                // 하위 객체에서 모듈을 찾아 목록을 구성합니다.
                GetComponentsInChildren(true, _moduleInstances);
            }
            else
            {
                var parent = transform;
                for (var i = 0; i < _modulePrefabs.Count; i++)
                {
                    var prefab = _modulePrefabs[i];
                    if (prefab == null)
                    {
                        continue;
                    }

                    // 모듈 프리팹을 인스턴스로 생성합니다.
                    var instance = Instantiate(prefab, parent);
                    instance.transform.position = Vector3.zero;
                    instance.transform.rotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    
                    _moduleInstances.Add(instance);
                }
            }

            for (var i = 0; i < _moduleInstances.Count; i++)
            {
                var behaviour = _moduleInstances[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour is not IViewModule module)
                {
                    continue;
                }

                if (_modules.Contains(module))
                {
                    continue;
                }

                _modules.Add(module);
            }
        }
        /// <summary>
        /// Update 함수를 처리합니다.
        /// </summary>

        protected virtual void Update()
        {
            // 핵심 로직을 처리합니다.

        }
        /// <summary>
        /// OnDestroy 함수를 처리합니다.
        /// </summary>

        protected virtual void OnDestroy()
        {
            // 핵심 로직을 처리합니다.
            ShutdownModule();
        }

        /// <summary>
        /// 모듈 초기화 완료 후 호출됩니다.
        /// </summary>
        protected virtual void OnInitialize()
        {
            // 핵심 로직을 처리합니다.

        }

        /// <summary>
        /// 모듈 Startup 직후 호출됩니다.
        /// </summary>
        protected virtual void OnStartup()
        {
            // 핵심 로직을 처리합니다.
        }

        /// <summary>
        /// 모듈 Shutdown 직후 호출됩니다.
        /// </summary>
        protected virtual void OnShutdown()
        {
            // 핵심 로직을 처리합니다.
            
        }
    }
}


