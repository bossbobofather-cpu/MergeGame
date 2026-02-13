using Noname.GameAbilitySystem;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 슬롯 상태 변경 내부 이벤트입니다.
    /// </summary>
    public sealed class SlotStateChangedInnerEvent : InnerEventBase
    {
        public int SlotIndex { get; }
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
        public int SlotIndex { get; }
        public Point3D Position { get; set; }

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
        public int PathIndex { get; }
        public MapPath Path { get; set; }

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
        public int ResultSlotIndex { get; set; } = -1;

        public EmptySlotRequestInnerEvent(long tick)
            : base(tick)
        {
        }
    }
}

