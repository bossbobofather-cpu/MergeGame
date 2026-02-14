using Noname.GameAbilitySystem;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 슬롯 상태 변경 내부 이벤트입니다.
    /// </summary>
    public sealed class SlotStateChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// SlotIndex 속성입니다.
        /// </summary>
        public int SlotIndex { get; }
        /// <summary>
        /// IsOccupied 속성입니다.
        /// </summary>
        public bool IsOccupied { get; }
        /// <summary>
        /// SlotStateChangedInnerEvent 메서드입니다.
        /// </summary>

        public SlotStateChangedInnerEvent(long tick, int slotIndex, bool isOccupied)
            : base(tick)
        {
            SlotIndex = slotIndex;
            IsOccupied = isOccupied;
        }
    }

    /// <summary>
    /// 슬롯 위치 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class SlotPositionRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// SlotIndex 속성입니다.
        /// </summary>
        public int SlotIndex { get; }
        /// <summary>
        /// Position 속성입니다.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// SlotPositionRequestInnerEvent 생성자입니다.
        /// </summary>
        public SlotPositionRequestInnerEvent(long tick, int slotIndex)
            : base(tick)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 경로 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class PathRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// PathIndex 속성입니다.
        /// </summary>
        public int PathIndex { get; }
        /// <summary>
        /// Path 속성입니다.
        /// </summary>
        public MapPath Path { get; set; }

        /// <summary>
        /// PathRequestInnerEvent 생성자입니다.
        /// </summary>
        public PathRequestInnerEvent(long tick, int pathIndex)
            : base(tick)
        {
            PathIndex = pathIndex;
        }
    }

    /// <summary>
    /// 빈 슬롯 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class EmptySlotRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ResultSlotIndex 속성입니다.
        /// </summary>
        public int ResultSlotIndex { get; set; } = -1;

        /// <summary>
        /// EmptySlotRequestInnerEvent 생성자입니다.
        /// </summary>
        public EmptySlotRequestInnerEvent(long tick)
            : base(tick)
        {
        }
    }
}
