using System.Collections.Generic;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 슬롯 정의입니다 (인덱스 + 위치).
    /// </summary>
    public struct SlotDefinition
    {
        public int Index;
        public float X;
        public float Y;

        public SlotDefinition(int index, float x, float y)
        {
            Index = index;
            X = x;
            Y = y;
        }

        public Point2D ToPoint2D() => new Point2D(X, Y);
    }

    /// <summary>
    /// 경로 정의입니다.
    /// </summary>
    public struct PathDefinition
    {
        public int PathIndex;
        public List<Point2D> Waypoints;

        public PathDefinition(int pathIndex, List<Point2D> waypoints)
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
        /// 슬롯 정의 목록입니다 (인덱스 + 위치).
        /// </summary>
        public List<SlotDefinition> SlotDefinitions { get; set; } = new();

        /// <summary>
        /// 경로 정의 목록입니다.
        /// </summary>
        public List<PathDefinition> PathDefinitions { get; set; } = new();

        /// <summary>
        /// 총 슬롯 수입니다.
        /// </summary>
        public int TotalSlots => SlotDefinitions.Count;

        /// <summary>
        /// 그리드 형태의 슬롯 정의를 생성하는 편의 메서드입니다.
        /// </summary>
        public static List<SlotDefinition> CreateGridSlotDefinitions(
            int rows, int columns, float slotWidth = 1f, float slotHeight = 1f)
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
                    definitions.Add(new SlotDefinition(index, x, y));
                }
            }

            return definitions;
        }
    }
}
