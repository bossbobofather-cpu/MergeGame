using System;
using System.Collections.Generic;
using MyProject.MergeGame.Events;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 맵 View 모듈입니다.
    /// MapInitializedEvent를 수신해 배경/슬롯/경로 오브젝트를 배치합니다.
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

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (evt is not MapInitializedEvent mapEvt)
            {
                return;
            }

            if (!IsMyPlayer(mapEvt.PlayerIndex))
            {
                return;
            }

            BuildMap(mapEvt);
        }

        private void BuildMap(MapInitializedEvent evt)
        {
            ClearMap();

            BuildBackground(evt.MapId);

            if(_backgroundInstance == null)
            {
                Debug.LogError("Failed to build map. Background instance is null.");
                return;
            }

            // 슬롯 배치
            if (_slotPrefab != null)
            {
                foreach (var slotPos in evt.SlotPositions)
                {
                    var slotObj = Instantiate(_slotPrefab, transform);
                    slotObj.transform.localPosition = new Vector3(slotPos.X, slotPos.Y, slotPos.Z);
                    slotObj.name = $"Slot_{slotPos.Index}";

                    // 슬롯 오브젝트에 슬롯 인덱스를 주입합니다.
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

            // LineRenderer로 경로 선을 그립니다.
            if (_pathLinePrefab != null)
            {
                var lineObj = Instantiate(_pathLinePrefab, _backgroundInstance.transform);
                lineObj.name = $"Path_{pathData.PathIndex}";
                lineObj.positionCount = pathData.Waypoints.Count;

                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];
                    lineObj.SetPosition(i, new Vector3(wp.X, wp.Y, wp.Z));
                }

                _pathObjects.Add(lineObj.gameObject);
            }

            // 경로 포인트 마커 배치
            if (_pathPointPrefab != null)
            {
                for (var i = 0; i < pathData.Waypoints.Count; i++)
                {
                    var wp = pathData.Waypoints[i];

                    var pointObj = Instantiate(_pathPointPrefab, _backgroundInstance.transform);
                    pointObj.transform.localPosition = new Vector3(wp.X, wp.Y, wp.Z);
                    pointObj.name = $"PathPoint_{pathData.PathIndex}_{i}";

                    // 경로 포인트에 (PathIndex, WaypointIndex)를 주입합니다.
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
            //_backgroundInstance.transform.localScale = Vector3.one;
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

            if (_backgroundInstance != null)
            {
                Destroy(_backgroundInstance);
                _backgroundInstance = null;
            }
        }

        protected override void OnShutdown()
        {
            ClearMap();
        }
    }
}

