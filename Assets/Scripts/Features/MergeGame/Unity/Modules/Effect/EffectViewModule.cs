using System.Collections;
using MyProject.Common.UI;
using MyProject.MergeGame.Events;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 전투/상호작용 이펙트를 담당하는 View 모듈입니다.
    /// 현재는 몬스터 주입 이벤트의 곡선 트레일 연출을 렌더링합니다.
    [DisallowMultipleComponent]
    public sealed class EffectViewModule : MergeViewModuleBase
    {
        [Header("Injection Trail")]
        [SerializeField] private GameObject _injectionTrailPrefab;
        [SerializeField] private float _flightDuration = 0.7f;
        [SerializeField] private float _arcHeight = 7f;
        [SerializeField] private float _arrivalHoldDuration = 0.15f;
        [SerializeField] private float _spawnHeightOffset = 0.25f;
        [SerializeField] private float _randomSpread = 0.75f;
        [SerializeField] private int _maxTrailsPerEvent = 3;
        [SerializeField] private Color _myOwnerTrailColor = new Color(0.25f, 0.75f, 1f, 1f);
        [SerializeField] private Color _otherOwnerTrailColor = new Color(1f, 0.45f, 0.2f, 1f);

        [Header("UI Render Override")]
        [SerializeField] private int _uiLayer = 5;
        [SerializeField] private int _myTrailSortingOrder = 600;
        [SerializeField] private int _otherTrailSortingOrder = 550;
        [SerializeField] private bool _useUiCameraSpace = true;

        private Page_MainHud _mainHud;
        private Camera _worldCamera;
        private Camera _uiCamera;
        private RectTransform _uiSystemRoot;

        private bool _loggedUiPipelineWarning;
        /// <summary>
        /// OnInit 메서드입니다.
        /// </summary>

        protected override void OnInit()
        {
            base.OnInit();
            RefreshCache();
        }
        /// <summary>
        /// OnEventMsg 메서드입니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (evt is not MonsterInjectionTriggeredEvent injectionEvent)
            {
                return;
            }

            HandleMonsterInjectionTrail(injectionEvent);
        }
        /// <summary>
        /// HandleMonsterInjectionTrail 메서드입니다.
        /// </summary>

        private void HandleMonsterInjectionTrail(MonsterInjectionTriggeredEvent evt)
        {
            if (GameView == null)
            {
                return;
            }

            if (!TryResolveTrailPoints(evt, out var start, out var end))
            {
                return;
            }

            var localPlayerIndex = GameView.AssignedPlayerIndex;
            var isMyOwnerTrail = localPlayerIndex >= 0 && evt.SourcePlayerIndex == localPlayerIndex;

            var trailCount = Mathf.Clamp(evt.InjectedCount, 1, Mathf.Max(1, _maxTrailsPerEvent));
            for (var i = 0; i < trailCount; i++)
            {
                var jitter = new Vector3(
                    Random.Range(-_randomSpread, _randomSpread),
                    Random.Range(-_randomSpread, _randomSpread),
                    0f);

                var jitteredStart = start + jitter;
                var jitteredEnd = end + jitter * 0.35f;
                SpawnInjectionTrail(jitteredStart, jitteredEnd, isMyOwnerTrail);
            }
        }
        /// <summary>
        /// TryResolveTrailPoints 메서드입니다.
        /// </summary>

        private bool TryResolveTrailPoints(MonsterInjectionTriggeredEvent evt, out Vector3 start, out Vector3 end)
        {
            start = default;
            end = default;

            if (!TryResolveTrailScreenPoints(evt, out var startScreen, out var endScreen))
            {
                return false;
            }

            if (!TryScreenToUiWorld(startScreen, out start))
            {
                return false;
            }

            if (!TryScreenToUiWorld(endScreen, out end))
            {
                return false;
            }

            start.y += _spawnHeightOffset;
            end.y += _spawnHeightOffset;
            return true;
        }
        /// <summary>
        /// TryResolveTrailScreenPoints 메서드입니다.
        /// </summary>

        private bool TryResolveTrailScreenPoints(MonsterInjectionTriggeredEvent evt, out Vector2 startScreen, out Vector2 endScreen)
        {
            RefreshCache();

            startScreen = default;
            endScreen = default;

            if (GameView == null)
            {
                return false;
            }

            var localPlayerIndex = GameView.AssignedPlayerIndex;
            var isLocalSource = localPlayerIndex >= 0 && evt.SourcePlayerIndex == localPlayerIndex;
            var isLocalTarget = localPlayerIndex >= 0 && evt.TargetPlayerIndex == localPlayerIndex;

            // 로컬 주입자: 월드 시작점에서 대상 미니맵 중심으로 이동
            if (isLocalSource)
            {
                if (!evt.HasSourcePosition)
                {
                    return false;
                }

                var sourceWorld = ApplyPlayerOffset(evt.SourcePlayerIndex, evt.SourceX, evt.SourceY, evt.SourceZ);
                if (!TryWorldToScreen(sourceWorld, out startScreen))
                {
                    return false;
                }

                if (!TryResolveMiniMapCenterScreenPosition(evt.TargetPlayerIndex, out endScreen))
                {
                    return false;
                }

                return true;
            }

            // 로컬 피격자: 소스 미니맵 중심에서 로컬 스폰 지점으로 이동
            if (isLocalTarget)
            {
                if (!TryResolveMiniMapCenterScreenPosition(evt.SourcePlayerIndex, out startScreen))
                {
                    return false;
                }

                var targetSpawnWorld = ApplyPlayerOffset(evt.TargetPlayerIndex, evt.TargetSpawnX, evt.TargetSpawnY, evt.TargetSpawnZ);
                if (!TryWorldToScreen(targetSpawnWorld, out endScreen))
                {
                    return false;
                }

                return true;
            }

            // 제3자 주입(B->C): 미니맵 중심에서 미니맵 중심으로 이동
            if (!TryResolveMiniMapCenterScreenPosition(evt.SourcePlayerIndex, out startScreen))
            {
                return false;
            }

            if (!TryResolveMiniMapCenterScreenPosition(evt.TargetPlayerIndex, out endScreen))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// TryResolveMiniMapCenterScreenPosition 메서드입니다.
        /// </summary>

        private bool TryResolveMiniMapCenterScreenPosition(int playerIndex, out Vector2 screenPosition)
        {
            screenPosition = default;

            if (playerIndex < 0)
            {
                return false;
            }

            if (!TryResolveMainHud(out var hud))
            {
                return false;
            }

            return hud.TryGetObserverMiniMapCenterScreenPosition(playerIndex, out screenPosition);
        }
        /// <summary>
        /// TryWorldToScreen 메서드입니다.
        /// </summary>

        private bool TryWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            screenPosition = default;
            if (!TryResolveWorldCamera(out var cam))
            {
                return false;
            }

            var projected = cam.WorldToScreenPoint(worldPosition);
            if (projected.z <= 0f)
            {
                return false;
            }

            screenPosition = new Vector2(projected.x, projected.y);
            return true;
        }
        /// <summary>
        /// TryScreenToUiWorld 메서드입니다.
        /// </summary>

        private bool TryScreenToUiWorld(Vector2 screenPosition, out Vector3 worldPosition)
        {
            worldPosition = default;

            if (!_useUiCameraSpace)
            {
                if (!TryResolveWorldCamera(out var worldCam))
                {
                    return false;
                }

                var depth = Mathf.Max(worldCam.nearClipPlane + 0.05f, 20f);
                worldPosition = worldCam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
                return true;
            }

            if (!TryResolveUiCameraSpace(out var uiRoot, out var uiCam))
            {
                LogUiPipelineWarningOnce();
                return false;
            }

            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(uiRoot, screenPosition, uiCam, out worldPosition))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// TryResolveMainHud 메서드입니다.
        /// </summary>

        private bool TryResolveMainHud(out Page_MainHud hud)
        {
            if ((_mainHud == null || !_mainHud) && UIManager.Instance != null)
            {
                UIManager.Instance.TryGetCachedPage(out _mainHud);
            }

            hud = _mainHud;
            return hud != null;
        }
        /// <summary>
        /// TryResolveWorldCamera 메서드입니다.
        /// </summary>

        private bool TryResolveWorldCamera(out Camera cam)
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }

            cam = _worldCamera;
            return cam != null;
        }
        /// <summary>
        /// TryResolveUiCameraSpace 메서드입니다.
        /// </summary>

        private bool TryResolveUiCameraSpace(out RectTransform uiRoot, out Camera uiCamera)
        {
            uiRoot = null;
            uiCamera = null;

            var root = UIManager.Instance != null ? UIManager.Instance.Root : null;
            if (root == null)
            {
                return false;
            }

            if (_uiSystemRoot == null || !_uiSystemRoot)
            {
                _uiSystemRoot = root.SystemRoot;
            }

            if (_uiCamera == null)
            {
                _uiCamera = root.UICamera;
            }

            uiRoot = _uiSystemRoot;
            uiCamera = _uiCamera;

            return uiRoot != null && uiCamera != null;
        }
        /// <summary>
        /// LogUiPipelineWarningOnce 메서드입니다.
        /// </summary>

        private void LogUiPipelineWarningOnce()
        {
            if (_loggedUiPipelineWarning)
            {
                return;
            }

            _loggedUiPipelineWarning = true;
            Debug.LogWarning("[EffectViewModule] UI camera/SystemRoot is not ready. Injection trail is skipped.");
        }
        /// <summary>
        /// RefreshCache 메서드입니다.
        /// </summary>

        private void RefreshCache()
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }

            if ((_mainHud == null || !_mainHud) && UIManager.Instance != null)
            {
                UIManager.Instance.TryGetCachedPage(out _mainHud);
            }

            if (UIManager.Instance != null && UIManager.Instance.Root != null)
            {
                if (_uiSystemRoot == null || !_uiSystemRoot)
                {
                    _uiSystemRoot = UIManager.Instance.Root.SystemRoot;
                }

                if (_uiCamera == null)
                {
                    _uiCamera = UIManager.Instance.Root.UICamera;
                }
            }
        }
        /// <summary>
        /// ApplyPlayerOffset 메서드입니다.
        /// </summary>

        private Vector3 ApplyPlayerOffset(int playerIndex, float x, float y, float z)
        {
            var offset = GameView != null ? GameView.GetPlayerOffsetPosition(playerIndex) : Vector3.zero;
            return new Vector3(x, y, z) + offset;
        }
        /// <summary>
        /// SpawnInjectionTrail 메서드입니다.
        /// </summary>

        private void SpawnInjectionTrail(Vector3 start, Vector3 end, bool isMyOwnerTrail)
        {
            var trailObject = CreateTrailObject();
            if (trailObject == null)
            {
                return;
            }

            trailObject.transform.SetParent(_uiSystemRoot != null ? _uiSystemRoot : transform, true);
            trailObject.transform.position = start;

            ApplyTrailRenderOverrides(trailObject, isMyOwnerTrail);

            var color = isMyOwnerTrail ? _myOwnerTrailColor : _otherOwnerTrailColor;
            ApplyTrailColor(trailObject, color);

            StartCoroutine(AnimateTrail(trailObject.transform, start, end));
        }
        /// <summary>
        /// ApplyTrailRenderOverrides 메서드입니다.
        /// </summary>

        private void ApplyTrailRenderOverrides(GameObject obj, bool isMyOwnerTrail)
        {
            if (obj == null)
            {
                return;
            }

            var layer = Mathf.Clamp(_uiLayer, 0, 31);
            SetLayerRecursively(obj, layer);

            var sortingOrder = isMyOwnerTrail ? _myTrailSortingOrder : _otherTrailSortingOrder;

            var trailRenderers = obj.GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trailRenderers.Length; i++)
            {
                var trail = trailRenderers[i];
                if (trail == null)
                {
                    continue;
                }

                trail.sortingLayerID = 0;
                trail.sortingOrder = sortingOrder;
            }

            var lineRenderers = obj.GetComponentsInChildren<LineRenderer>(true);
            for (var i = 0; i < lineRenderers.Length; i++)
            {
                var line = lineRenderers[i];
                if (line == null)
                {
                    continue;
                }

                line.sortingLayerID = 0;
                line.sortingOrder = sortingOrder;
            }

            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.sortingLayerID = 0;
                renderer.sortingOrder = sortingOrder;
            }
        }
        /// <summary>
        /// SetLayerRecursively 메서드입니다.
        /// </summary>

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null)
            {
                return;
            }

            var transforms = obj.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t == null)
                {
                    continue;
                }

                t.gameObject.layer = layer;
            }
        }
        /// <summary>
        /// CreateTrailObject 메서드입니다.
        /// </summary>

        private GameObject CreateTrailObject()
        {
            if (_injectionTrailPrefab != null)
            {
                return Instantiate(_injectionTrailPrefab);
            }

            var fallback = new GameObject("InjectionTrail_Fallback");
            var trail = fallback.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.widthMultiplier = 0.2f;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                trail.material = new Material(shader);
            }

            return fallback;
        }
        /// <summary>
        /// ApplyTrailColor 메서드입니다.
        /// </summary>

        private static void ApplyTrailColor(GameObject obj, Color color)
        {
            if (obj == null)
            {
                return;
            }

            var trailRenderers = obj.GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trailRenderers.Length; i++)
            {
                var trail = trailRenderers[i];
                if (trail == null)
                {
                    continue;
                }

                trail.startColor = color;
                trail.endColor = color;
            }

            var lineRenderers = obj.GetComponentsInChildren<LineRenderer>(true);
            for (var i = 0; i < lineRenderers.Length; i++)
            {
                var line = lineRenderers[i];
                if (line == null)
                {
                    continue;
                }

                line.startColor = color;
                line.endColor = color;
            }

            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.material == null)
                {
                    continue;
                }

                if (renderer.material.HasProperty("_Color"))
                {
                    renderer.material.color = color;
                }
            }
        }
        /// <summary>
        /// AnimateTrail 메서드입니다.
        /// </summary>

        private IEnumerator AnimateTrail(Transform trailTransform, Vector3 start, Vector3 end)
        {
            if (trailTransform == null)
            {
                yield break;
            }

            var duration = Mathf.Max(0.05f, _flightDuration);
            var elapsed = 0f;
            var previous = start;
            var control = (start + end) * 0.5f + Vector3.up * Mathf.Max(0f, _arcHeight);

            while (elapsed < duration)
            {
                if (trailTransform == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var point = EvaluateQuadraticBezier(start, control, end, t);
                trailTransform.position = point;

                var direction = point - previous;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    trailTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }

                previous = point;
                yield return null;
            }

            if (trailTransform != null)
            {
                trailTransform.position = end;

                if (_arrivalHoldDuration > 0f)
                {
                    yield return new WaitForSeconds(_arrivalHoldDuration);
                }

                if (trailTransform != null)
                {
                    Destroy(trailTransform.gameObject);
                }
            }
        }
        /// <summary>
        /// EvaluateQuadraticBezier 메서드입니다.
        /// </summary>

        private static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0
                   + 2f * oneMinusT * t * p1
                   + t * t * p2;
        }
    }
}
