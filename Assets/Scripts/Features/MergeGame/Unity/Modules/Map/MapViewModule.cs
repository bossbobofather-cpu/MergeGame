using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// View용 맵 모듈입니다.
    /// 호스트의 MapInitializedEvent를 수신하여 슬롯/경로 프리팹을 배치합니다.
    /// </summary>
    public class MapViewModule : MergeViewModuleBase
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private GameObject _pathPointPrefab;
        [SerializeField] private LineRenderer _pathLinePrefab;

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

            // 슬롯 배치
            if (_slotPrefab != null)
            {
                foreach (var slotPos in evt.SlotPositions)
                {
                    var slotObj = Instantiate(_slotPrefab, transform);
                    slotObj.transform.localPosition = new Vector3(slotPos.X, slotPos.Y, 0f);
                    slotObj.name = $"Slot_{slotPos.Index}";
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
                foreach (var wp in pathData.Waypoints)
                {
                    var pointObj = Instantiate(_pathPointPrefab, transform);
                    pointObj.transform.localPosition = new Vector3(wp.X, wp.Y, 0f);
                    pointObj.name = $"PathPoint_{pathData.PathIndex}";
                    _pathObjects.Add(pointObj);
                }
            }
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
        }

        protected override void OnShutdown()
        {
            ClearMap();
        }
    }
}
