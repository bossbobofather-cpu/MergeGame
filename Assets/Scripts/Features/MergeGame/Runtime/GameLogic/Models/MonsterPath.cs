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
        /// <summary>
        /// MonsterPath 함수를 처리합니다.
        /// </summary>

        public MonsterPath(int pathIndex, IEnumerable<Point3D> waypoints)
        {
            // 핵심 로직을 처리합니다.
            PathIndex = pathIndex;
            _waypoints = new List<Point3D>(waypoints);
            _segmentLengths = new List<float>();

            CalculateSegmentLengths();
        }
        /// <summary>
        /// CalculateSegmentLengths 함수를 처리합니다.
        /// </summary>

        private void CalculateSegmentLengths()
        {
            // 핵심 로직을 처리합니다.
            _totalLength = 0f;
            _segmentLengths.Clear();

            for (var i = 0; i < _waypoints.Count - 1; i++)
            {
                var length = MathF.Sqrt(Point3D.DistanceSquared(_waypoints[i], _waypoints[i + 1]));

                // 부분 길이는 WayPoint간 거리를 리스트로
                // ex 첫번째 wayPoint와 두번째 wayPoin간 길이는 _segmentLengths[0] ..
                // _segmentLengths 웨이포인트 간 거리 목록은 웨이포인트의 총 개수 - 1 이다
                // _waypoints.Count - 1 == _segmentLengths.Count
                _segmentLengths.Add(length);

                //전체 길이는 WayPoint간의 거리의 합
                _totalLength += length;
            }
        }

        /// <summary>
        /// 진행도에 해당하는 위치를 반환합니다.
        /// </summary>
        /// <param name="progress">0.0 ~ 1.0 사이의 진행도</param>
        public Point3D GetPositionAtProgress(float progress)
        {
            // 웨이포인트가 하나도 없으면 원점 반환
            if (_waypoints.Count == 0) return Point3D.zero;

            // 웨이포인트가 하나뿐이면 항상 그 위치
            if (_waypoints.Count == 1) return _waypoints[0];

            // progress 값을 0~1로 범위로 강제로 클램프
            progress = Math.Clamp(progress, 0f, 1f);

            // 진행도가 시작점이면 첫 웨이포인트 반환
            if (progress <= 0f) return _waypoints[0];

            // 진행도가 끝점이면 마지막 웨이포인트 반환
            if (progress >= 1f) return _waypoints[^1];

            // 전체 경로 길이(_totalLength) 에서
            // 현재 progress에 해당하는 목표 거리 계산
            var targetDistance = _totalLength * progress;

            // 지금까지 누적 된 거리
            var accumulatedDistance = 0f;

            for (var i = 0; i < _segmentLengths.Count; i++)
            {
                var segmentLength = _segmentLengths[i];

                // 현재 세그먼트에 목표 거리가 포함되는지?
                if (accumulatedDistance + segmentLength >= targetDistance)
                {
                    // 세그먼트 내부에서 남은 거리
                    var remainingDistance = targetDistance - accumulatedDistance;

                    // 세그먼트 내부 보간 비율
                    var t = segmentLength > 0 ? remainingDistance / segmentLength : 0f;

                    // 현재 세그먼트의 시작점과 끝점
                    var p1 = _waypoints[i];
                    var p2 = _waypoints[i + 1];

                    // 보간된 위치 반환
                    return new Point3D(
                        p1.X + (p2.X - p1.X) * t,
                        p1.Y + (p2.Y - p1.Y) * t,
                        p1.Z + (p2.Z - p1.Z) * t
                    );
                }

                accumulatedDistance += segmentLength;
            }

            // 방어코드
            return _waypoints[^1];
        }

        /// <summary>
        /// 시작점을 반환합니다.
        /// </summary>
        public Point3D GetStartPosition()
        {
            // 핵심 로직을 처리합니다.
            return _waypoints.Count > 0 ? _waypoints[0] : Point3D.zero;
        }

        /// <summary>
        /// 도착점을 반환합니다.
        /// </summary>
        public Point3D GetEndPosition()
        {
            // 핵심 로직을 처리합니다.
            return _waypoints.Count > 0 ? _waypoints[^1] : Point3D.zero;
        }
    }
}


