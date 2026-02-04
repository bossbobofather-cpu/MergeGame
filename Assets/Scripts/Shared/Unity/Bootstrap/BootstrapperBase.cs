using System.Collections.Generic;
using Noname.GameHost.GameEvent;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 게임 부트스트래퍼의 기본 동작을 정의하는 베이스입니다.
    /// 런타임 이벤트 바인딩과 매니저 생성 흐름을 담당합니다.
    /// </summary>
    public abstract class BootstrapperBase : MonoBehaviour
    {
        /// <summary>
        /// 초기화 시 생성할 매니저 프리팹 목록입니다.
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _managerPrefabs = new();

        private static bool _initialized;
        private bool _didInit;

        /// <summary>
        /// 정적 상태와 이벤트 바인딩을 초기화합니다.
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
        /// 런타임 시작 시 씬 이벤트를 등록합니다.
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

        private void Start()
        {
            if (_didInit)
            {
                return;
            }

            // 씬 로드 직후 한 번만 초기화를 실행합니다.
            _didInit = true;
            OnInit();
        }

        /// <summary>
        /// 자식 클래스 초기화 지점입니다.
        /// </summary>
        protected virtual void OnInit()
        {
            for (var i = 0; i < _managerPrefabs.Count; i++)
            {
                // 등록된 매니저 프리팹을 순차적으로 생성합니다.
                CreateManager(_managerPrefabs[i]);
            }
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            GameEventBus.ClearScene(scene.handle);
        }

        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (oldScene.handle == newScene.handle)
            {
                return;
            }

            GameEventBus.SetActiveScene(newScene.handle);
        }

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
