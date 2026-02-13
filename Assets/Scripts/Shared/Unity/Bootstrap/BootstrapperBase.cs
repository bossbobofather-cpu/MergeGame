using System.Collections.Generic;
using Noname.GameHost.GameEvent;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 寃뚯엫 遺?몄뒪?몃옒?쇱쓽 湲곕낯 ?숈옉???뺤쓽?섎뒗 踰좎씠?ㅼ엯?덈떎.
    /// ?고????대깽??諛붿씤?⑷낵 留ㅻ땲? ?앹꽦 ?먮쫫???대떦?⑸땲??
    /// </summary>
    public abstract class BootstrapperBase : MonoBehaviour
    {
        /// <summary>
        /// 珥덇린?????앹꽦??留ㅻ땲? ?꾨━??紐⑸줉?낅땲??
        /// </summary>
        [SerializeField] private List<MonoBehaviour> _managerPrefabs = new();

        private static bool _initialized;
        private bool _didInit;

        /// <summary>
        /// ?뺤쟻 ?곹깭? ?대깽??諛붿씤?⑹쓣 珥덇린?뷀빀?덈떎.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // 핵심 로직을 처리합니다.
            _initialized = false;
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            GameEventBus.ResetAll();
        }

        /// <summary>
        /// ?고????쒖옉 ?????대깽?몃? ?깅줉?⑸땲??
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // 핵심 로직을 처리합니다.
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
        /// Start 함수를 처리합니다.
        /// </summary>

        private void Start()
        {
            // 핵심 로직을 처리합니다.
            if (_didInit)
            {
                return;
            }

            // ??濡쒕뱶 吏곹썑 ??踰덈쭔 珥덇린?붾? ?ㅽ뻾?⑸땲??
            _didInit = true;
            OnInit();
        }

        /// <summary>
        /// ?먯떇 ?대옒??珥덇린??吏?먯엯?덈떎.
        /// </summary>
        protected virtual void OnInit()
        {
            // 핵심 로직을 처리합니다.
            for (var i = 0; i < _managerPrefabs.Count; i++)
            {
                // ?깅줉??留ㅻ땲? ?꾨━?뱀쓣 ?쒖감?곸쑝濡??앹꽦?⑸땲??
                var manager = CreateManager(_managerPrefabs[i]);
                if(manager != null)
                {
                    manager.Initialize();
                }
            }
        }
        /// <summary>
        /// OnSceneUnloaded 함수를 처리합니다.
        /// </summary>

        private static void OnSceneUnloaded(Scene scene)
        {
            // 핵심 로직을 처리합니다.
            GameEventBus.ClearScene(scene.handle);
        }
        /// <summary>
        /// OnActiveSceneChanged 함수를 처리합니다.
        /// </summary>

        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            // 핵심 로직을 처리합니다.
            if (oldScene.handle == newScene.handle)
            {
                return;
            }

            GameEventBus.SetActiveScene(newScene.handle);
        }
        /// <summary>
        /// OnQuit 함수를 처리합니다.
        /// </summary>

        private static void OnQuit()
        {
            // 핵심 로직을 처리합니다.
            Application.quitting -= OnQuit;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            GameEventBus.ResetAll();
        }

        /// <summary>
        /// 留ㅻ땲? ?꾨━?뱀쓣 ?앹꽦?섍굅??湲곗〈 ?몄뒪?댁뒪瑜?諛섑솚?⑸땲??
        /// </summary>
        protected IManager CreateManager(MonoBehaviour prefab)
        {
            // 핵심 로직을 처리합니다.
            if (prefab == null)
            {
                return null;
            }

            if (prefab is not IManager)
            {
                Debug.LogWarning($"IManager瑜?援ы쁽?섏? ?딆? 留ㅻ땲? ?꾨━?뱀엯?덈떎: {prefab.name}");
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
