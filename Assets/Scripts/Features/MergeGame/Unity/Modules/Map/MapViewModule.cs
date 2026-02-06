using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// View용 맵 모듈입니다.
    /// 호스트의 MapInitializedEvent를 수신하여 맵 배경/슬롯/경로 프리팹을 배치합니다.
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

        [Header("Prefabs")]
        [SerializeField] private GameObject _defaultBackgroundPrefab;
        [SerializeField] private List<MapBackgroundEntry> _backgroundPrefabs = new();
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private GameObject _pathPointPrefab;
        [SerializeField] private LineRenderer _pathLinePrefab;

        private GameObject _backgroundInstance;

        private readonly Dictionary<int, GameObject> _slotObjects = new();
        private readonly List<GameObject> _pathObjects = new();

        /// <summary>
        /// 슬롯 오브젝트를 가져옵니다.
        /// </summary>
        public GameObject GetSlotObject(int slotIndex)
        {
            return _slotObjects.TryGetValue(slotIndex, out var obj) ? obj : null;
        }

        /// <summary>
        /// 슬롯 오브젝트 목록입니다.
        /// </summary>
        public IReadOnlyDictionary<int, GameObject> SlotObjects => _slotObjects;

        public override void OnHostEvent(MergeHostEvent evt)
        {
            if (evt is MapInitializedEvent mapEvt)
            {
                BuildMap(mapEvt);
            }
        }

        private void BuildMap(MapInitializedEvent evt)
        {
            ClearMap();

            BuildBackground(evt.MapId);

            // 슬롯 배치
            if (_slotPrefab != null)
            {
                foreach (var slotPos in evt.SlotPositions)
                {
                    var slotObj = Instantiate(_slotPrefab, transform);
                    slotObj.transform.localPosition = new Vector3(slotPos.X, slotPos.Y, 0f);
                    slotObj.name = $"Slot_{slotPos.Index}";

                    // 런타임에서 슬롯 인덱스를 주입합니다. (프리팹이 스크립트를 갖고 있지 않아도 됨)
                    (slotObj.GetComponent<MergeSlotView>() ?? slotObj.AddComponent<MergeSlotView>()).SetSlotIndex(slotPos.Index);

                    _slotObjects[slotPos.Index] = slotObj;
                }
            }

            // 경로 배치
            foreach (var pathData in evt.Paths)
            {
                BuildPath(pathData);
            }
        }

        private void BuildPath(PathData pathData)
        {
            if (pathData.Waypoints == null || pathData.Waypoints.Count == 0)
            {
                return;
            }

            // LineRenderer로 경로 시각화
            if (_pathLinePrefab != null)
            {
                var lineObj = Instantiate(_pathLinePrefab, transform);
                lineObj.name = $"Path_{pathData.PathIndex}";
                lineObj.positionCount = pathData.Waypoints.Count;

                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];
                    lineObj.SetPosition(i, new Vector3(wp.X, wp.Y, 0f));
                }

                _pathObjects.Add(lineObj.gameObject);
            }

            // 웨이포인트 마커 배치
            if (_pathPointPrefab != null)
            {
                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];

                    var pointObj = Instantiate(_pathPointPrefab, transform);
                    pointObj.transform.localPosition = new Vector3(wp.X, wp.Y, 0f);
                    pointObj.name = $"PathPoint_{pathData.PathIndex}_{i}";

                    // 런타임에서 (PathIndex, WaypointIndex)를 주입합니다.
                    (pointObj.GetComponent<MergePathPointView>() ?? pointObj.AddComponent<MergePathPointView>()).SetIndices(pathData.PathIndex, i);

                    _pathObjects.Add(pointObj);
                }
            }
        }

        private void BuildBackground(int mapId)
        {
            var prefab = ResolveBackgroundPrefab(mapId);
            if (prefab == null)
            {
                return;
            }

            _backgroundInstance = Instantiate(prefab, transform);
            _backgroundInstance.name = $"MapBackground_{mapId}";
            _backgroundInstance.transform.localPosition = Vector3.zero;
            _backgroundInstance.transform.localRotation = Quaternion.identity;
            _backgroundInstance.transform.localScale = Vector3.one;
        }

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

        private void ClearMap()
        {
            if (_backgroundInstance != null)
            {
                Destroy(_backgroundInstance);
                _backgroundInstance = null;
            }

            foreach (var slotObj in _slotObjects.Values)
            {
                if (slotObj != null)
                {
                    Destroy(slotObj);
                }
            }
            _slotObjects.Clear();

            foreach (var pathObj in _pathObjects)
            {
                if (pathObj != null)
                {
                    Destroy(pathObj);
                }
            }
            _pathObjects.Clear();
        }

        protected override void OnShutdown()
        {
            ClearMap();
        }
    }
}
