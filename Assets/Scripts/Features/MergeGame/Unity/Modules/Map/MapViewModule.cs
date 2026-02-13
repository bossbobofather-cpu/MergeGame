using System;
using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Unity.Events;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 맵 View 모듈입니다.
    /// MapInitializedEvent를 수신해 플레이어별 배경/슬롯/경로 오브젝트를 배치합니다.
    /// 로컬 플레이어는 MainCamera를, 원격 플레이어는 RenderTexture 카메라를 구성합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MapViewModule : MergeViewModuleBase
    {
        [Serializable]
        private sealed class MapBackgroundEntry
        {
            [SerializeField] private int _mapId;
            [SerializeField] private GameObject _prefab;

            public int MapId => _mapId;
            public GameObject Prefab => _prefab;
        }

        [Serializable]
        private sealed class CameraRigSettings
        {
            [SerializeField] private Vector3 _position = new Vector3(0f, 30f, 0f);
            [SerializeField] private Vector3 _rotationEuler = new Vector3(90f, 0f, 0f);
            [SerializeField] private bool _orthographic;
            [SerializeField] private float _orthographicSize = 10f;
            [SerializeField] private float _fieldOfView = 60f;
            [SerializeField] private float _nearClip = 0.3f;
            [SerializeField] private float _farClip = 100f;
            [SerializeField] private CameraClearFlags _clearFlags = CameraClearFlags.SolidColor;
            [SerializeField] private Color _backgroundColor = Color.black;
            [SerializeField] private LayerMask _cullingMask = ~0;
            [SerializeField] private float _depth = -1f;

            public Vector3 Position => _position;
            public Vector3 RotationEuler => _rotationEuler;
            public bool Orthographic => _orthographic;
            public float OrthographicSize => _orthographicSize;
            public float FieldOfView => _fieldOfView;
            public float NearClip => _nearClip;
            public float FarClip => _farClip;
            public CameraClearFlags ClearFlags => _clearFlags;
            public Color BackgroundColor => _backgroundColor;
            public int CullingMask => _cullingMask.value;
            public float Depth => _depth;
        }

        [Serializable]
        private sealed class ObserverRenderTextureSettings
        {
            [SerializeField] private int _width = 256;
            [SerializeField] private int _height = 256;
            [SerializeField] private int _depthBuffer = 16;
            [SerializeField] private RenderTextureFormat _format = RenderTextureFormat.ARGB32;

            public int Width => Mathf.Max(64, _width);
            public int Height => Mathf.Max(64, _height);
            public int DepthBuffer => _depthBuffer;
            public RenderTextureFormat Format => _format;
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject _defaultBackgroundPrefab;
        [SerializeField] private List<MapBackgroundEntry> _backgroundPrefabs = new();
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private GameObject _pathPointPrefab;
        [SerializeField] private LineRenderer _pathLinePrefab;

        [Header("Camera")]
        [SerializeField] private bool _autoConfigureMainCamera = true;
        [SerializeField] private CameraRigSettings _mainCameraSettings = new();
        [SerializeField] private bool _createObserverCameras = true;
        [SerializeField] private CameraRigSettings _observerCameraSettings = new();
        [SerializeField] private ObserverRenderTextureSettings _observerRenderTextureSettings = new();

        [Header("MiniMap Broadcast")]
        [SerializeField] private float _observerMiniMapBroadcastInterval = 0.5f;
        [SerializeField] private bool _broadcastImmediatelyOnChange = true;

        private static readonly IReadOnlyDictionary<int, GameObject> EmptySlotMap = new Dictionary<int, GameObject>();

        private readonly Dictionary<int, GameObject> _mapRootsByPlayer = new();
        private readonly Dictionary<int, GameObject> _backgroundByPlayer = new();
        private readonly Dictionary<int, Dictionary<int, GameObject>> _slotObjectsByPlayer = new();
        private readonly Dictionary<int, List<GameObject>> _pathObjectsByPlayer = new();

        private readonly Dictionary<int, Camera> _observerCamerasByPlayer = new();
        private readonly Dictionary<int, RenderTexture> _observerRenderTexturesByPlayer = new();

        private readonly List<MiniMapRenderTargetInfo> _miniMapTargetsBuffer = new();

        private float _miniMapBroadcastTimer;
        private bool _miniMapDirty;
        private int _miniMapVersion;

        /// <summary>
        /// 로컬 플레이어 슬롯 오브젝트를 가져옵니다.
        /// </summary>
        public GameObject GetSlotObject(int slotIndex)
        {
            var localPlayer = GameView != null ? GameView.AssignedPlayerIndex : -1;
            return GetSlotObject(localPlayer, slotIndex);
        }

        /// <summary>
        /// 지정 플레이어의 슬롯 오브젝트를 가져옵니다.
        /// </summary>
        public GameObject GetSlotObject(int playerIndex, int slotIndex)
        {
            if (!_slotObjectsByPlayer.TryGetValue(playerIndex, out var slotMap))
            {
                return null;
            }

            return slotMap.TryGetValue(slotIndex, out var obj) ? obj : null;
        }

        /// <summary>
        /// 로컬 플레이어 슬롯 오브젝트 목록입니다.
        /// </summary>
        public IReadOnlyDictionary<int, GameObject> SlotObjects
        {
            get
            {
                var localPlayer = GameView != null ? GameView.AssignedPlayerIndex : -1;
                return _slotObjectsByPlayer.TryGetValue(localPlayer, out var slotMap)
                    ? slotMap
                    : EmptySlotMap;
            }
        }

        /// <summary>
        /// 지정 플레이어 슬롯의 월드 위치를 반환합니다.
        /// </summary>
        public bool TryGetSlotWorldPosition(int playerIndex, int slotIndex, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            var slotObj = GetSlotObject(playerIndex, slotIndex);
            if (slotObj == null)
            {
                return false;
            }

            worldPosition = slotObj.transform.position;
            return true;
        }
        /// <summary>
        /// OnEventMsg 메서드입니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (evt is not MapInitializedEvent mapEvt)
            {
                return;
            }

            BuildMap(mapEvt);
        }
        /// <summary>
        /// OnDisconnectedEvent 메서드입니다.
        /// </summary>

        public override void OnDisconnectedEvent()
        {
            ClearAllMaps();
            MarkMiniMapDirty();
            PublishMiniMapTargets();
        }
        /// <summary>
        /// Update 메서드입니다.
        /// </summary>

        private void Update()
        {
            if (GameView == null)
            {
                return;
            }

            var interval = Mathf.Max(0.1f, _observerMiniMapBroadcastInterval);
            _miniMapBroadcastTimer += Time.deltaTime;
            if (_miniMapBroadcastTimer < interval)
            {
                return;
            }

            _miniMapBroadcastTimer = 0f;
            PublishMiniMapTargets();
        }
        /// <summary>
        /// BuildMap 메서드입니다.
        /// </summary>

        private void BuildMap(MapInitializedEvent evt)
        {
            var playerIndex = evt.PlayerIndex;
            ClearMap(playerIndex);

            var mapRoot = GetOrCreateMapRoot(playerIndex);
            mapRoot.name = GetMapRootName(playerIndex);
            mapRoot.transform.localPosition = GetPlayerOffset(playerIndex);

            BuildBackground(playerIndex, evt.MapId, mapRoot.transform);

            var slotMap = GetOrCreateSlotMap(playerIndex);
            if (_slotPrefab != null)
            {
                foreach (var slotPos in evt.SlotPositions)
                {
                    var slotObj = Instantiate(_slotPrefab, mapRoot.transform);
                    slotObj.transform.localPosition = new Vector3(slotPos.X, slotPos.Y, slotPos.Z);
                    slotObj.name = $"Slot_P{playerIndex}_{slotPos.Index}";

                    (slotObj.GetComponent<MergeSlotView>() ?? slotObj.AddComponent<MergeSlotView>()).SetSlotIndex(slotPos.Index);
                    slotMap[slotPos.Index] = slotObj;
                }
            }

            var pathObjectList = GetOrCreatePathObjectList(playerIndex);
            var pathParent = _backgroundByPlayer.TryGetValue(playerIndex, out var background) && background != null
                ? background.transform
                : mapRoot.transform;

            foreach (var pathData in evt.Paths)
            {
                BuildPath(pathData, pathParent, pathObjectList, playerIndex);
            }

            ConfigureMapCamera(playerIndex, mapRoot.transform);
            MarkMiniMapDirty();
            TryPublishMiniMapTargetsImmediately();
        }
        /// <summary>
        /// ConfigureMapCamera 메서드입니다.
        /// </summary>

        private void ConfigureMapCamera(int playerIndex, Transform mapRoot)
        {
            if (IsLocalPlayer(playerIndex))
            {
                ReleaseObserverCamera(playerIndex);

                if (_autoConfigureMainCamera)
                {
                    ConfigureMainCamera(mapRoot);
                }

                return;
            }

            if (_createObserverCameras)
            {
                ConfigureObserverCamera(playerIndex, mapRoot);
            }
        }
        /// <summary>
        /// ConfigureMainCamera 메서드입니다.
        /// </summary>

        private void ConfigureMainCamera(Transform mapRoot)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
                if (camObj.GetComponent<AudioListener>() == null)
                {
                    camObj.AddComponent<AudioListener>();
                }
            }

            var worldPos = mapRoot.position + _mainCameraSettings.Position;
            cam.transform.position = worldPos;
            cam.transform.rotation = Quaternion.Euler(_mainCameraSettings.RotationEuler);

            ApplyCameraComponentSettings(cam, _mainCameraSettings);
            cam.targetTexture = null;
        }
        /// <summary>
        /// ConfigureObserverCamera 메서드입니다.
        /// </summary>

        private void ConfigureObserverCamera(int playerIndex, Transform mapRoot)
        {
            if (!_observerCamerasByPlayer.TryGetValue(playerIndex, out var cam) || cam == null)
            {
                var camObj = new GameObject($"ObserverCamera_P{playerIndex}");
                camObj.transform.SetParent(mapRoot, false);
                cam = camObj.AddComponent<Camera>();
                _observerCamerasByPlayer[playerIndex] = cam;
            }
            else
            {
                cam.transform.SetParent(mapRoot, false);
            }

            cam.transform.localPosition = _observerCameraSettings.Position;
            cam.transform.localRotation = Quaternion.Euler(_observerCameraSettings.RotationEuler);

            ApplyCameraComponentSettings(cam, _observerCameraSettings);
            cam.enabled = true;

            var rt = GetOrCreateObserverRenderTexture(playerIndex);
            cam.targetTexture = rt;
        }
        /// <summary>
        /// GetOrCreateObserverRenderTexture 메서드입니다.
        /// </summary>

        private RenderTexture GetOrCreateObserverRenderTexture(int playerIndex)
        {
            if (_observerRenderTexturesByPlayer.TryGetValue(playerIndex, out var existing)
                && existing != null
                && existing.width == _observerRenderTextureSettings.Width
                && existing.height == _observerRenderTextureSettings.Height)
            {
                return existing;
            }

            if (existing != null)
            {
                existing.Release();
                Destroy(existing);
                MarkMiniMapDirty();
            }

            var rt = new RenderTexture(
                _observerRenderTextureSettings.Width,
                _observerRenderTextureSettings.Height,
                _observerRenderTextureSettings.DepthBuffer,
                _observerRenderTextureSettings.Format)
            {
                name = $"ObserverRT_P{playerIndex}",
                useMipMap = false,
                autoGenerateMips = false
            };

            rt.Create();
            _observerRenderTexturesByPlayer[playerIndex] = rt;
            MarkMiniMapDirty();
            return rt;
        }
        /// <summary>
        /// ApplyCameraComponentSettings 메서드입니다.
        /// </summary>

        private static void ApplyCameraComponentSettings(Camera cam, CameraRigSettings settings)
        {
            cam.orthographic = settings.Orthographic;
            cam.orthographicSize = settings.OrthographicSize;
            cam.fieldOfView = settings.FieldOfView;
            cam.nearClipPlane = settings.NearClip;
            cam.farClipPlane = settings.FarClip;
            cam.clearFlags = settings.ClearFlags;
            cam.backgroundColor = settings.BackgroundColor;
            cam.cullingMask = settings.CullingMask;
            cam.depth = settings.Depth;
        }
        /// <summary>
        /// IsLocalPlayer 메서드입니다.
        /// </summary>

        private bool IsLocalPlayer(int playerIndex)
        {
            return GameView != null
                   && GameView.AssignedPlayerIndex >= 0
                   && GameView.AssignedPlayerIndex == playerIndex;
        }
        /// <summary>
        /// BuildPath 메서드입니다.
        /// </summary>

        private void BuildPath(PathData pathData, Transform parent, List<GameObject> pathObjects, int playerIndex)
        {
            if (pathData.Waypoints == null || pathData.Waypoints.Count == 0)
            {
                return;
            }

            if (_pathLinePrefab != null)
            {
                var lineObj = Instantiate(_pathLinePrefab, parent);
                lineObj.name = $"Path_P{playerIndex}_{pathData.PathIndex}";
                lineObj.positionCount = pathData.Waypoints.Count;
                lineObj.useWorldSpace = false;

                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];
                    lineObj.SetPosition(i, new Vector3(wp.X, wp.Y, wp.Z));
                }

                pathObjects.Add(lineObj.gameObject);
            }

            if (_pathPointPrefab != null)
            {
                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];

                    var pointObj = Instantiate(_pathPointPrefab, parent);
                    pointObj.transform.localPosition = new Vector3(wp.X, wp.Y, wp.Z);
                    pointObj.name = $"PathPoint_P{playerIndex}_{pathData.PathIndex}_{i}";

                    (pointObj.GetComponent<MergePathPointView>() ?? pointObj.AddComponent<MergePathPointView>())
                        .SetIndices(pathData.PathIndex, i);

                    pathObjects.Add(pointObj);
                }
            }
        }
        /// <summary>
        /// BuildBackground 메서드입니다.
        /// </summary>

        private void BuildBackground(int playerIndex, int mapId, Transform parent)
        {
            var prefab = ResolveBackgroundPrefab(mapId);
            if (prefab == null)
            {
                return;
            }

            var background = Instantiate(prefab, parent);
            background.name = $"MapBackground_P{playerIndex}_{mapId}";
            background.transform.localPosition = Vector3.zero;
            background.transform.localRotation = Quaternion.identity;

            _backgroundByPlayer[playerIndex] = background;
        }
        /// <summary>
        /// ResolveBackgroundPrefab 메서드입니다.
        /// </summary>

        private GameObject ResolveBackgroundPrefab(int mapId)
        {
            for (var i = 0; i < _backgroundPrefabs.Count; i++)
            {
                var entry = _backgroundPrefabs[i];
                if (entry != null && entry.Prefab != null && entry.MapId == mapId)
                {
                    return entry.Prefab;
                }
            }

            return _defaultBackgroundPrefab;
        }
        /// <summary>
        /// GetOrCreateMapRoot 메서드입니다.
        /// </summary>

        private GameObject GetOrCreateMapRoot(int playerIndex)
        {
            if (_mapRootsByPlayer.TryGetValue(playerIndex, out var root) && root != null)
            {
                return root;
            }

            root = new GameObject(GetMapRootName(playerIndex));
            root.transform.SetParent(transform, false);
            root.transform.localPosition = GetPlayerOffset(playerIndex);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            _mapRootsByPlayer[playerIndex] = root;
            return root;
        }
        /// <summary>
        /// GetOrCreateSlotMap 메서드입니다.
        /// </summary>

        private Dictionary<int, GameObject> GetOrCreateSlotMap(int playerIndex)
        {
            if (_slotObjectsByPlayer.TryGetValue(playerIndex, out var slotMap))
            {
                return slotMap;
            }

            slotMap = new Dictionary<int, GameObject>();
            _slotObjectsByPlayer[playerIndex] = slotMap;
            return slotMap;
        }
        /// <summary>
        /// GetOrCreatePathObjectList 메서드입니다.
        /// </summary>

        private List<GameObject> GetOrCreatePathObjectList(int playerIndex)
        {
            if (_pathObjectsByPlayer.TryGetValue(playerIndex, out var pathObjects))
            {
                return pathObjects;
            }

            pathObjects = new List<GameObject>();
            _pathObjectsByPlayer[playerIndex] = pathObjects;
            return pathObjects;
        }
        /// <summary>
        /// GetMapRootName 메서드입니다.
        /// </summary>

        private string GetMapRootName(int playerIndex)
        {
            if (IsLocalPlayer(playerIndex))
            {
                return "MapRoot_Player_Mine";
            }

            return $"MapRoot_Player_{playerIndex}";
        }
        /// <summary>
        /// GetPlayerOffset 메서드입니다.
        /// </summary>

        private Vector3 GetPlayerOffset(int playerIndex)
        {
            return GameView != null
                ? GameView.GetPlayerOffsetPosition(playerIndex)
                : Vector3.zero;
        }
        /// <summary>
        /// ReleaseObserverCamera 메서드입니다.
        /// </summary>

        private void ReleaseObserverCamera(int playerIndex)
        {
            if (_observerCamerasByPlayer.TryGetValue(playerIndex, out var cam) && cam != null)
            {
                Destroy(cam.gameObject);
                MarkMiniMapDirty();
            }

            _observerCamerasByPlayer.Remove(playerIndex);

            if (_observerRenderTexturesByPlayer.TryGetValue(playerIndex, out var rt) && rt != null)
            {
                rt.Release();
                Destroy(rt);
                MarkMiniMapDirty();
            }

            _observerRenderTexturesByPlayer.Remove(playerIndex);
        }
        /// <summary>
        /// ClearMap 메서드입니다.
        /// </summary>

        private void ClearMap(int playerIndex)
        {
            ReleaseObserverCamera(playerIndex);

            if (_mapRootsByPlayer.TryGetValue(playerIndex, out var root) && root != null)
            {
                Destroy(root);
            }

            _mapRootsByPlayer.Remove(playerIndex);
            _backgroundByPlayer.Remove(playerIndex);
            _slotObjectsByPlayer.Remove(playerIndex);
            _pathObjectsByPlayer.Remove(playerIndex);
            MarkMiniMapDirty();
        }
        /// <summary>
        /// ClearAllMaps 메서드입니다.
        /// </summary>

        private void ClearAllMaps()
        {
            foreach (var pair in _mapRootsByPlayer)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);
                }
            }

            _mapRootsByPlayer.Clear();
            _backgroundByPlayer.Clear();
            _slotObjectsByPlayer.Clear();
            _pathObjectsByPlayer.Clear();

            foreach (var pair in _observerCamerasByPlayer)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }
            _observerCamerasByPlayer.Clear();

            foreach (var pair in _observerRenderTexturesByPlayer)
            {
                if (pair.Value != null)
                {
                    pair.Value.Release();
                    Destroy(pair.Value);
                }
            }
            _observerRenderTexturesByPlayer.Clear();
            MarkMiniMapDirty();
        }
        /// <summary>
        /// MarkMiniMapDirty 메서드입니다.
        /// </summary>

        private void MarkMiniMapDirty()
        {
            _miniMapDirty = true;
        }
        /// <summary>
        /// TryPublishMiniMapTargetsImmediately 메서드입니다.
        /// </summary>

        private void TryPublishMiniMapTargetsImmediately()
        {
            if (!_broadcastImmediatelyOnChange)
            {
                return;
            }

            PublishMiniMapTargets();
        }
        /// <summary>
        /// PublishMiniMapTargets 메서드입니다.
        /// </summary>

        private void PublishMiniMapTargets()
        {
            if (GameView == null)
            {
                return;
            }

            if (_miniMapDirty)
            {
                _miniMapVersion++;
                _miniMapDirty = false;
            }

            _miniMapTargetsBuffer.Clear();

            var localPlayerIndex = GameView.AssignedPlayerIndex;
            foreach (var pair in _observerRenderTexturesByPlayer)
            {
                if (localPlayerIndex >= 0 && pair.Key == localPlayerIndex)
                {
                    continue;
                }

                var texture = pair.Value;
                if (texture == null)
                {
                    continue;
                }

                _miniMapTargetsBuffer.Add(new MiniMapRenderTargetInfo(pair.Key, texture));
            }

            _miniMapTargetsBuffer.Sort((lhs, rhs) => lhs.PlayerIndex.CompareTo(rhs.PlayerIndex));
            var snapshot = _miniMapTargetsBuffer.ToArray();

            GameView.Publish(new MiniMapRenderTargetsUpdatedEvent(this, snapshot, _miniMapVersion));
        }
        /// <summary>
        /// OnShutdown 메서드입니다.
        /// </summary>

        protected override void OnShutdown()
        {
            ClearAllMaps();
            base.OnShutdown();
        }
    }
}

