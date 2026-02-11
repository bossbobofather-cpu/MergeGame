using System.Collections.Generic;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 슬롯 정의 데이터입니다. (인덱스 + 위치)
    /// </summary>
    public struct SlotDefinition
    {
        public int Index;
        public float X;
        public float Y;
        public float Z;

        public SlotDefinition(int index, float x, float y, float z)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D ToPoint3D() => new Point3D(X, Y, Z);
    }

    /// <summary>
    /// 경로 정의 데이터입니다.
    /// </summary>
    public struct PathDefinition
    {
        public int PathIndex;
        public List<Point3D> Waypoints;

        public PathDefinition(int pathIndex, List<Point3D> waypoints)
        {
            PathIndex = pathIndex;
            Waypoints = waypoints;
        }
    }

    /// <summary>
    /// MapModule 설정입니다.
    /// </summary>
    public sealed class MapModuleConfig
    {
        /// <summary>
        /// 맵 ID입니다.
        /// </summary>
        public int MapId { get; set; } = DevHelperSet.DevIdHelper.DEV_DEFAULT_MAP_ID;

        /// <summary>
        /// 슬롯 정의 목록입니다.
        /// </summary>
        public List<SlotDefinition> SlotDefinitions { get; set; } = new();

        /// <summary>
        /// 경로 정의 목록입니다.
        /// </summary>
        public List<PathDefinition> PathDefinitions { get; set; } = new();

        /// <summary>
        /// 전체 슬롯 수입니다.
        /// </summary>
        public int TotalSlots => SlotDefinitions.Count;
    }
}
