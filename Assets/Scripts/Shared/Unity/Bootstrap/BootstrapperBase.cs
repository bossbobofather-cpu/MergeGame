using System.Collections.Generic;
using Noname.GameHost.GameEvent;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 게임 부트스트랩 공통 베이스입니다.
    /// 매니저 생성과 전역 이벤트 버스 초기화를 담당합니다.
    /// </summary>
    public abstract class BootstrapperBase : MonoBehaviour
    {
        /// <summary>
        /// 씬 시작 시 생성할 매니저 프리팹 목록입니다.
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _managerPrefabs = new();

        private static bool _initialized;
        private bool _didInit;

        /// <summary>
        /// 도메인 리로드 시 정적 상태를 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _initialized = false;
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            GameEventBus.ResetAll();
        }

        /// <summary>
        /// 씬 로드 직후 전역 이벤트 구독을 등록합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Application.quitting += OnQuit;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            GameEventBus.TrySetActiveScene(SceneManager.GetActiveScene().handle);
        }

        /// <summary>
        /// 부트스트랩 진입점입니다.
        /// </summary>
        private void Start()
        {
            if (_didInit)
            {
                return;
            }

            _didInit = true;
            OnInit();
        }

        /// <summary>
        /// 매니저를 생성/초기화합니다.
        /// </summary>
        protected virtual void OnInit()
        {
            for (var i = 0; i < _managerPrefabs.Count; i++)
            {
                var manager = CreateManager(_managerPrefabs[i]);
                if (manager != null)
                {
                    manager.Initialize();
                }
            }
        }

        /// <summary>
        /// 씬 언로드 시 씬 스코프 이벤트 버스를 정리합니다.
        /// </summary>
        private static void OnSceneUnloaded(Scene scene)
        {
            GameEventBus.ClearScene(scene.handle);
        }

        /// <summary>
        /// 활성 씬 변경 시 이벤트 버스 활성 씬 핸들을 갱신합니다.
        /// </summary>
        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (oldScene.handle == newScene.handle)
            {
                return;
            }

            GameEventBus.SetActiveScene(newScene.handle);
        }

        /// <summary>
        /// 앱 종료 시 정적 이벤트를 해제합니다.
        /// </summary>
        private static void OnQuit()
        {
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            GameEventBus.ResetAll();
        }

        /// <summary>
        /// 매니저 프리팹을 생성하거나 기존 인스턴스를 반환합니다.
        /// </summary>
        protected IManager CreateManager(MonoBehaviour prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (prefab is not IManager)
            {
                Debug.LogWarning($"IManager를 구현하지 않은 매니저 프리팹입니다: {prefab.name}");
                return null;
            }

            var managerType = prefab.GetType();
            var existing = FindFirstObjectByType(managerType);
            if (existing != null)
            {
                return existing as IManager;
            }

            var instance = Instantiate(prefab, transform);
            return instance as IManager;
        }
    }
}
