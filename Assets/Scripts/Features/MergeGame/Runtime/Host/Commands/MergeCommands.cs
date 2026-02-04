using Noname.GameHost;

namespace MyProject.MergeGame.Commands
{
    /// <summary>
    /// MergeGame용 Command 기본 타입입니다.
    /// </summary>
    public abstract class MergeCommand : GameCommandBase
    {
        protected MergeCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// MergeGame용 Command 처리 결과 기본 타입입니다.
    /// </summary>
    public abstract class MergeCommandResult : GameCommandResultBase
    {
        protected MergeCommandResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
    }

    #region 게임 시작/종료

    /// <summary>
    /// 게임 시작 Command입니다.
    /// </summary>
    public sealed class StartMergeGameCommand : MergeCommand
    {
        public StartMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 시작 Result입니다.
    /// </summary>
    public sealed class StartMergeGameResult : MergeCommandResult
    {
        private StartMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static StartMergeGameResult Ok(long tick, long senderUid)
            => new StartMergeGameResult(tick, senderUid, true, null);

        public static StartMergeGameResult Fail(long tick, long senderUid, string reason)
            => new StartMergeGameResult(tick, senderUid, false, reason);
    }

    /// <summary>
    /// 게임 종료 Command입니다.
    /// </summary>
    public sealed class EndMergeGameCommand : MergeCommand
    {
        public EndMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 종료 Result입니다.
    /// </summary>
    public sealed class EndMergeGameResult : MergeCommandResult
    {
        private EndMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static EndMergeGameResult Ok(long tick, long senderUid)
            => new EndMergeGameResult(tick, senderUid, true, null);

        public static EndMergeGameResult Fail(long tick, long senderUid, string reason)
            => new EndMergeGameResult(tick, senderUid, false, reason);
    }

    #endregion

    #region 유닛 관리

    /// <summary>
    /// 유닛 스폰 Command입니다.
    /// </summary>
    public sealed class SpawnUnitCommand : MergeCommand
    {
        /// <summary>
        /// 스폰할 슬롯 인덱스입니다. -1이면 빈 슬롯에 자동 배치합니다.
        /// </summary>
        public int SlotIndex { get; }

        public SpawnUnitCommand(long senderUid, int slotIndex = -1) : base(senderUid)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 스폰 Result입니다.
    /// </summary>
    public sealed class SpawnUnitResult : MergeCommandResult
    {
        /// <summary>
        /// 생성된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        private SpawnUnitResult(long tick, long senderUid, bool success, string reason, long unitUid, int slotIndex)
            : base(tick, senderUid, success, reason)
        {
            UnitUid = unitUid;
            SlotIndex = slotIndex;
        }

        public static SpawnUnitResult Ok(long tick, long senderUid, long unitUid, int slotIndex)
            => new SpawnUnitResult(tick, senderUid, true, null, unitUid, slotIndex);

        public static SpawnUnitResult Fail(long tick, long senderUid, string reason)
            => new SpawnUnitResult(tick, senderUid, false, reason, 0, -1);
    }

    /// <summary>
    /// 유닛 머지 Command입니다.
    /// </summary>
    public sealed class MergeUnitCommand : MergeCommand
    {
        /// <summary>
        /// 드래그 시작 슬롯 인덱스입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 드래그 종료 슬롯 인덱스입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        public MergeUnitCommand(long senderUid, int fromSlotIndex, int toSlotIndex) : base(senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }
    }

    /// <summary>
    /// 유닛 머지 Result입니다.
    /// </summary>
    public sealed class MergeUnitResult : MergeCommandResult
    {
        /// <summary>
        /// 머지 결과로 생성된 유닛 UID입니다.
        /// </summary>
        public long NewUnitUid { get; }

        /// <summary>
        /// 머지 결과 유닛의 등급입니다.
        /// </summary>
        public int NewGrade { get; }

        private MergeUnitResult(long tick, long senderUid, bool success, string reason, long newUnitUid, int newGrade)
            : base(tick, senderUid, success, reason)
        {
            NewUnitUid = newUnitUid;
            NewGrade = newGrade;
        }

        public static MergeUnitResult Ok(long tick, long senderUid, long newUnitUid, int newGrade)
            => new MergeUnitResult(tick, senderUid, true, null, newUnitUid, newGrade);

        public static MergeUnitResult Fail(long tick, long senderUid, string reason)
            => new MergeUnitResult(tick, senderUid, false, reason, 0, 0);
    }

    #endregion
}
