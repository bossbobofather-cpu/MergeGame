using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 슬롯 효과 타입입니다.
    /// </summary>
    public enum SlotEffectType
    {
        None,
        AttackBoost,
        SpeedBoost,
        RangeBoost,
        Blocked
    }

    /// <summary>
    /// 맵 슬롯 정보입니다.
    /// </summary>
    public sealed class MapSlot
    {
        /// <summary>
        /// 슬롯 인덱스입니다.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 슬롯 월드 위치입니다.
        /// </summary>
        public Point3D Position { get; }

        /// <summary>
        /// 사용 가능 여부입니다.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// 현재 점유 여부입니다.
        /// </summary>
        public bool IsOccupied { get; set; }

        /// <summary>
        /// 슬롯에 부여된 효과입니다.
        /// </summary>
        public SlotEffectType Effect { get; set; } = SlotEffectType.None;

        /// <summary>
        /// 효과 수치입니다.
        /// </summary>
        public float EffectValue { get; set; }
        /// <summary>
        /// MapSlot 메서드입니다.
        /// </summary>

        public MapSlot(int index, Point3D position)
        {
            Index = index;
            Position = position;
        }
    }

    /// <summary>
    /// 맵 경로 정보입니다.
    /// 몬스터 웨이브의 이동 경로를 정의합니다.
    /// </summary>
    public sealed class MapPath
    {
        private readonly List<Point3D> _waypoints;
        private readonly List<float> _segmentLengths;
        private float _totalLength;

        /// <summary>
        /// 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 경로의 웨이포인트 목록입니다.
        /// </summary>
        public IReadOnlyList<Point3D> Waypoints => _waypoints;

        /// <summary>
        /// 경로의 총 길이입니다.
        /// </summary>
        public float TotalLength => _totalLength;

        /// <summary>
        /// 시작 위치입니다.
        /// </summary>
        public Point3D StartPosition => _waypoints.Count > 0 ? _waypoints[0] : Point3D.zero;

        /// <summary>
        /// 도착 위치입니다.
        /// </summary>
        public Point3D EndPosition => _waypoints.Count > 0 ? _waypoints[^1] : Point3D.zero;

        /// <summary>
        /// MapPath 생성자입니다.
        /// </summary>
        public MapPath(int pathIndex, IEnumerable<Point3D> waypoints)
        {
            PathIndex = pathIndex;
            _waypoints = new List<Point3D>(waypoints);
            _segmentLengths = new List<float>();
            CalculateSegmentLengths();
        }
        /// <summary>
        /// CalculateSegmentLengths 메서드입니다.
        /// </summary>

        private void CalculateSegmentLengths()
        {
            _totalLength = 0f;
            _segmentLengths.Clear();

            for (var i = 0; i < _waypoints.Count - 1; i++)
            {
                var length = MathF.Sqrt(Point3D.DistanceSquared(_waypoints[i], _waypoints[i + 1]));
                _segmentLengths.Add(length);
                _totalLength += length;
            }
        }

        /// <summary>
        /// 진행도에 해당하는 위치를 반환합니다.
        /// </summary>
        /// <param name="progress">0.0 ~ 1.0 사이의 진행도</param>
        public Point3D GetPositionAtProgress(float progress)
        {
            if (_waypoints.Count == 0) return Point3D.zero;
            if (_waypoints.Count == 1) return _waypoints[0];

            progress = Math.Clamp(progress, 0f, 1f);

            if (progress <= 0f) return _waypoints[0];
            if (progress >= 1f) return _waypoints[^1];

            var targetDistance = _totalLength * progress;
            var accumulatedDistance = 0f;

            for (var i = 0; i < _segmentLengths.Count; i++)
            {
                var segmentLength = _segmentLengths[i];

                if (accumulatedDistance + segmentLength >= targetDistance)
                {
                    var remainingDistance = targetDistance - accumulatedDistance;
                    var t = segmentLength > 0 ? remainingDistance / segmentLength : 0f;

                    var p1 = _waypoints[i];
                    var p2 = _waypoints[i + 1];

                    return new Point3D(
                        p1.X + (p2.X - p1.X) * t,
                        p1.Y + (p2.Y - p1.Y) * t,
                        p1.Z + (p2.Z - p1.Z) * t
                    );
                }

                accumulatedDistance += segmentLength;
            }

            return _waypoints[^1];
        }
    }

    /// <summary>
    /// 맵 모듈입니다.
    /// 보드, 슬롯, 몬스터 경로를 관리합니다.
    /// </summary>
    public sealed class MapModule : HostModuleBase<MapModuleConfig>
    {
        /// <summary>
        /// MODULE_ID 필드입니다.
        /// </summary>
        public const string MODULE_ID = "map";

        private MapSlot[] _slots;
        private readonly List<MapPath> _paths = new();

        /// <inheritdoc />
        public override string ModuleId => MODULE_ID;

        /// <inheritdoc />
        public override bool IsRequired => true;

        /// <inheritdoc />
        public override int Priority => 100; // 높은 우선순위 (다른 모듈보다 먼저 초기화)

        /// <summary>
        /// 전체 슬롯 수입니다.
        /// </summary>
        public int TotalSlots => Config.TotalSlots;

        /// <summary>
        /// 맵 ID입니다.
        /// </summary>
        public int MapId => Config.MapId;

        /// <summary>
        /// 슬롯 목록입니다.
        /// </summary>
        public IReadOnlyList<MapSlot> Slots => _slots;

        /// <summary>
        /// 경로 목록입니다.
        /// </summary>
        public IReadOnlyList<MapPath> Paths => _paths;

        /// <inheritdoc />
        protected override void OnConfigure(MapModuleConfig config)
        {
            InitializeSlots();
            InitializePaths();
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // 내부 이벤트 구독 (모듈 전용)
            SubscribeInnerEvent<SlotStateChangedInnerEvent>(OnSlotStateChanged);
            SubscribeInnerEvent<SlotPositionRequestInnerEvent>(OnSlotPositionRequest);
            SubscribeInnerEvent<PathRequestInnerEvent>(OnPathRequest);
            SubscribeInnerEvent<EmptySlotRequestInnerEvent>(OnEmptySlotRequest);

            // 공유 내부 이벤트 구독 (다른 모듈에서 호출)
            SubscribeInnerEvent<GetSlotPositionRequest>(OnGetSlotPositionRequest);
            SubscribeInnerEvent<GetEmptySlotRequest>(OnGetEmptySlotRequest);
            SubscribeInnerEvent<GetPathRequest>(OnGetPathRequest);
            SubscribeInnerEvent<GetPathPositionRequest>(OnGetPathPositionRequest);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            // 내부 이벤트 구독 해제 (모듈 전용)
            UnsubscribeInnerEvent<SlotStateChangedInnerEvent>(OnSlotStateChanged);
            UnsubscribeInnerEvent<SlotPositionRequestInnerEvent>(OnSlotPositionRequest);
            UnsubscribeInnerEvent<PathRequestInnerEvent>(OnPathRequest);
            UnsubscribeInnerEvent<EmptySlotRequestInnerEvent>(OnEmptySlotRequest);

            // 공유 내부 이벤트 구독 해제
            UnsubscribeInnerEvent<GetSlotPositionRequest>(OnGetSlotPositionRequest);
            UnsubscribeInnerEvent<GetEmptySlotRequest>(OnGetEmptySlotRequest);
            UnsubscribeInnerEvent<GetPathRequest>(OnGetPathRequest);
            UnsubscribeInnerEvent<GetPathPositionRequest>(OnGetPathPositionRequest);
        }
        /// <summary>
        /// InitializeSlots 메서드입니다.
        /// </summary>

        private void InitializeSlots()
        {
            var definitions = Config.SlotDefinitions;
            _slots = new MapSlot[definitions.Count];

            for (var i = 0; i < definitions.Count; i++)
            {
                var def = definitions[i];
                _slots[i] = new MapSlot(def.Index, new Point3D(def.X, def.Y, def.Z));
            }
        }
        /// <summary>
        /// InitializePaths 메서드입니다.
        /// </summary>

        private void InitializePaths()
        {
            _paths.Clear();

            foreach (var pathDef in Config.PathDefinitions)
            {
                _paths.Add(new MapPath(pathDef.PathIndex, pathDef.Waypoints));
            }
        }

        #region 슬롯 관리

        /// <summary>
        /// 슬롯을 가져옵니다.
        /// </summary>
        public MapSlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                return null;
            }

            return _slots[index];
        }

        /// <summary>
        /// 슬롯 점유 상태를 설정합니다.
        /// </summary>
        public void SetSlotOccupied(int index, bool occupied)
        {
            var slot = GetSlot(index);
            if (slot != null)
            {
                slot.IsOccupied = occupied;
            }
        }

        /// <summary>
        /// 슬롯 사용 가능 여부를 설정합니다.
        /// </summary>
        public void SetSlotAvailable(int index, bool available)
        {
            var slot = GetSlot(index);
            if (slot != null)
            {
                slot.IsAvailable = available;
            }
        }

        /// <summary>
        /// 슬롯 효과를 설정합니다.
        /// </summary>
        public void SetSlotEffect(int index, SlotEffectType effect, float value = 0f)
        {
            var slot = GetSlot(index);
            if (slot != null)
            {
                slot.Effect = effect;
                slot.EffectValue = value;
            }
        }

        /// <summary>
        /// 빈 슬롯 인덱스를 찾습니다.
        /// </summary>
        /// <returns>빈 슬롯 인덱스. 없으면 -1</returns>
        public int FindEmptySlot()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot.IsAvailable && !slot.IsOccupied)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 모든 빈 슬롯 인덱스를 찾습니다.
        /// </summary>
        public List<int> FindAllEmptySlots()
        {
            var result = new List<int>();

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot.IsAvailable && !slot.IsOccupied)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// 점유 중인 슬롯 개수를 계산합니다.
        /// </summary>
        public int CountOccupiedSlots()
        {
            var count = 0;
            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsOccupied)
                {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region 경로 관리

        /// <summary>
        /// 경로를 추가합니다.
        /// </summary>
        public void AddPath(IEnumerable<Point3D> waypoints)
        {
            var pathIndex = _paths.Count;
            var path = new MapPath(pathIndex, waypoints);
            _paths.Add(path);
        }

        /// <summary>
        /// 경로를 가져옵니다.
        /// </summary>
        public MapPath GetPath(int pathIndex)
        {
            if (pathIndex < 0 || pathIndex >= _paths.Count)
            {
                return null;
            }

            return _paths[pathIndex];
        }

        /// <summary>
        /// 기본 경로를 가져옵니다.
        /// </summary>
        public MapPath GetDefaultPath()
        {
            return _paths.Count > 0 ? _paths[0] : null;
        }

        /// <summary>
        /// 모든 경로를 초기화합니다.
        /// </summary>
        public void ClearPaths()
        {
            _paths.Clear();
        }

        #endregion

        #region 내부 이벤트 핸들러
        /// <summary>
        /// OnSlotStateChanged 메서드입니다.
        /// </summary>

        private void OnSlotStateChanged(SlotStateChangedInnerEvent evt)
        {
            SetSlotOccupied(evt.SlotIndex, evt.IsOccupied);
        }
        /// <summary>
        /// OnSlotPositionRequest 메서드입니다.
        /// </summary>

        private void OnSlotPositionRequest(SlotPositionRequestInnerEvent evt)
        {
            var slot = GetSlot(evt.SlotIndex);
            if (slot != null)
            {
                evt.Position = slot.Position;
            }
        }
        /// <summary>
        /// OnPathRequest 메서드입니다.
        /// </summary>

        private void OnPathRequest(PathRequestInnerEvent evt)
        {
            evt.Path = GetPath(evt.PathIndex);
        }
        /// <summary>
        /// OnEmptySlotRequest 메서드입니다.
        /// </summary>

        private void OnEmptySlotRequest(EmptySlotRequestInnerEvent evt)
        {
            evt.ResultSlotIndex = FindEmptySlot();
        }

        #endregion

        #region 공유 내부 이벤트 핸들러
        /// <summary>
        /// OnGetSlotPositionRequest 메서드입니다.
        /// </summary>

        private void OnGetSlotPositionRequest(GetSlotPositionRequest evt)
        {
            var slot = GetSlot(evt.SlotIndex);
            if (slot != null)
            {
                evt.Position = slot.Position;
                evt.Found = true;
            }
        }
        /// <summary>
        /// OnGetEmptySlotRequest 메서드입니다.
        /// </summary>

        private void OnGetEmptySlotRequest(GetEmptySlotRequest evt)
        {
            evt.SlotIndex = FindEmptySlot();
        }
        /// <summary>
        /// OnGetPathRequest 메서드입니다.
        /// </summary>

        private void OnGetPathRequest(GetPathRequest evt)
        {
            evt.Path = GetPath(evt.PathIndex);
        }
        /// <summary>
        /// OnGetPathPositionRequest 메서드입니다.
        /// </summary>

        private void OnGetPathPositionRequest(GetPathPositionRequest evt)
        {
            var path = GetPath(evt.PathIndex);
            if (path != null)
            {
                evt.Position = path.GetPositionAtProgress(evt.Progress);
                evt.Found = true;
            }
        }

        #endregion
    }
}


