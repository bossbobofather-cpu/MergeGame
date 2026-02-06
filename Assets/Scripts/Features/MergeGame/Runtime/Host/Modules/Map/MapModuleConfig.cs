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

        public SlotDefinition(int index, float x, float y, float z = 0f)
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
        public int MapId { get; set; } = 1;

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

        /// <summary>
        /// 그리드 형태의 슬롯 정의를 생성합니다.
        /// </summary>
        public static List<SlotDefinition> CreateGridSlotDefinitions(
            int rows, int columns, float slotWidth = 1f, float slotHeight = 1f, float z = 0f)
        {
            var definitions = new List<SlotDefinition>();
            var halfWidth = (columns - 1) * slotWidth * 0.5f;
            var halfHeight = (rows - 1) * slotHeight * 0.5f;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var index = row * columns + col;
                    var x = col * slotWidth - halfWidth;
                    var y = row * slotHeight - halfHeight;
                    definitions.Add(new SlotDefinition(index, x, y, z));
                }
            }

            return definitions;
        }
    }
}
