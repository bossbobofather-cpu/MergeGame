using System.Collections;
using MyProject.Common.UI;
using MyProject.MergeGame.Events;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// Handles visual effects for combat/interaction.
    /// Currently renders curved trails for monster injection events.
    /// </summary>
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
        /// OnInit 함수를 처리합니다.
        /// </summary>

        protected override void OnInit()
        {
            // 핵심 로직을 처리합니다.
            base.OnInit();
            RefreshCache();
        }
        /// <summary>
        /// OnEventMsg 함수를 처리합니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (evt is not MonsterInjectionTriggeredEvent injectionEvent)
            {
                return;
            }

            HandleMonsterInjectionTrail(injectionEvent);
        }
        /// <summary>
        /// HandleMonsterInjectionTrail 함수를 처리합니다.
        /// </summary>

        private void HandleMonsterInjectionTrail(MonsterInjectionTriggeredEvent evt)
        {
            // 핵심 로직을 처리합니다.
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
        /// TryResolveTrailPoints 함수를 처리합니다.
        /// </summary>

        private bool TryResolveTrailPoints(MonsterInjectionTriggeredEvent evt, out Vector3 start, out Vector3 end)
        {
            // 핵심 로직을 처리합니다.
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
        /// TryResolveTrailScreenPoints 함수를 처리합니다.
        /// </summary>

        private bool TryResolveTrailScreenPoints(MonsterInjectionTriggeredEvent evt, out Vector2 startScreen, out Vector2 endScreen)
        {
            // 핵심 로직을 처리합니다.
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

            // Local source: Source world position to target mini-map center.
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

            // Local target: source mini-map center to target spawn world position.
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

            // Unrelated injection (B -> C): mini-map center to mini-map center.
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
        /// TryResolveMiniMapCenterScreenPosition 함수를 처리합니다.
        /// </summary>

        private bool TryResolveMiniMapCenterScreenPosition(int playerIndex, out Vector2 screenPosition)
        {
            // 핵심 로직을 처리합니다.
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
        /// TryWorldToScreen 함수를 처리합니다.
        /// </summary>

        private bool TryWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            // 핵심 로직을 처리합니다.
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
        /// TryScreenToUiWorld 함수를 처리합니다.
        /// </summary>

        private bool TryScreenToUiWorld(Vector2 screenPosition, out Vector3 worldPosition)
        {
            // 핵심 로직을 처리합니다.
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
        /// TryResolveMainHud 함수를 처리합니다.
        /// </summary>

        private bool TryResolveMainHud(out Page_MainHud hud)
        {
            // 핵심 로직을 처리합니다.
            if ((_mainHud == null || !_mainHud) && UIManager.Instance != null)
            {
                UIManager.Instance.TryGetCachedPage(out _mainHud);
            }

            hud = _mainHud;
            return hud != null;
        }
        /// <summary>
        /// TryResolveWorldCamera 함수를 처리합니다.
        /// </summary>

        private bool TryResolveWorldCamera(out Camera cam)
        {
            // 핵심 로직을 처리합니다.
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }

            cam = _worldCamera;
            return cam != null;
        }
        /// <summary>
        /// TryResolveUiCameraSpace 함수를 처리합니다.
        /// </summary>

        private bool TryResolveUiCameraSpace(out RectTransform uiRoot, out Camera uiCamera)
        {
            // 핵심 로직을 처리합니다.
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
        /// LogUiPipelineWarningOnce 함수를 처리합니다.
        /// </summary>

        private void LogUiPipelineWarningOnce()
        {
            // 핵심 로직을 처리합니다.
            if (_loggedUiPipelineWarning)
            {
                return;
            }

            _loggedUiPipelineWarning = true;
            Debug.LogWarning("[EffectViewModule] UI camera/SystemRoot is not ready. Injection trail is skipped.");
        }
        /// <summary>
        /// RefreshCache 함수를 처리합니다.
        /// </summary>

        private void RefreshCache()
        {
            // 핵심 로직을 처리합니다.
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
        /// ApplyPlayerOffset 함수를 처리합니다.
        /// </summary>

        private Vector3 ApplyPlayerOffset(int playerIndex, float x, float y, float z)
        {
            // 핵심 로직을 처리합니다.
            var offset = GameView != null ? GameView.GetPlayerOffsetPosition(playerIndex) : Vector3.zero;
            return new Vector3(x, y, z) + offset;
        }
        /// <summary>
        /// SpawnInjectionTrail 함수를 처리합니다.
        /// </summary>

        private void SpawnInjectionTrail(Vector3 start, Vector3 end, bool isMyOwnerTrail)
        {
            // 핵심 로직을 처리합니다.
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
        /// ApplyTrailRenderOverrides 함수를 처리합니다.
        /// </summary>

        private void ApplyTrailRenderOverrides(GameObject obj, bool isMyOwnerTrail)
        {
            // 핵심 로직을 처리합니다.
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
        /// SetLayerRecursively 함수를 처리합니다.
        /// </summary>

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            // 핵심 로직을 처리합니다.
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
        /// CreateTrailObject 함수를 처리합니다.
        /// </summary>

        private GameObject CreateTrailObject()
        {
            // 핵심 로직을 처리합니다.
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
        /// ApplyTrailColor 함수를 처리합니다.
        /// </summary>

        private static void ApplyTrailColor(GameObject obj, Color color)
        {
            // 핵심 로직을 처리합니다.
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
        /// AnimateTrail 함수를 처리합니다.
        /// </summary>

        private IEnumerator AnimateTrail(Transform trailTransform, Vector3 start, Vector3 end)
        {
            // 핵심 로직을 처리합니다.
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
        /// EvaluateQuadraticBezier 함수를 처리합니다.
        /// </summary>

        private static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            // 핵심 로직을 처리합니다.
            var oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0
                   + 2f * oneMinusT * t * p1
                   + t * t * p2;
        }
    }
}
