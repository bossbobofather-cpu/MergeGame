using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Models
{
    /// <summary>
    /// 몬스터가 이동하는 경로입니다.
    /// </summary>
    public sealed class MonsterPath
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

        public MonsterPath(int pathIndex, IEnumerable<Point3D> waypoints)
        {
            PathIndex = pathIndex;
            _waypoints = new List<Point3D>(waypoints);
            _segmentLengths = new List<float>();

            CalculateSegmentLengths();
        }

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

        /// <summary>
        /// 시작점을 반환합니다.
        /// </summary>
        public Point3D GetStartPosition()
        {
            return _waypoints.Count > 0 ? _waypoints[0] : Point3D.zero;
        }

        /// <summary>
        /// 도착점을 반환합니다.
        /// </summary>
        public Point3D GetEndPosition()
        {
            return _waypoints.Count > 0 ? _waypoints[^1] : Point3D.zero;
        }
    }
}


