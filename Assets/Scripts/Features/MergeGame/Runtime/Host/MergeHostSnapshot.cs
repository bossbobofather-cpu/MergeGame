using System.Collections.Generic;
using Noname.GameHost;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 세션 단계입니다.
    /// </summary>
    public enum MergeSessionPhase
    {
        /// <summary>
        /// 초기 상태입니다.
        /// </summary>
        None,

        /// <summary>
        /// 게임 진행 중입니다.
        /// </summary>
        Playing,

        /// <summary>
        /// 일시 정지 상태입니다.
        /// </summary>
        Paused,

        /// <summary>
        /// 게임 오버 상태입니다.
        /// </summary>
        GameOver
    }

    /// <summary>
    /// 슬롯 정보 스냅샷입니다.
    /// </summary>
    public readonly struct SlotSnapshot
    {
        /// <summary>
        /// 슬롯 인덱스입니다.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 유닛 UID입니다. 비어있으면 0입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 유닛 등급입니다. 비어있으면 0입니다.
        /// </summary>
        public int UnitGrade { get; }

        /// <summary>
        /// 슬롯이 비어있는지 여부입니다.
        /// </summary>
        public bool IsEmpty => UnitUid == 0;

        public SlotSnapshot(int index, long unitUid, int unitGrade)
        {
            Index = index;
            UnitUid = unitUid;
            UnitGrade = unitGrade;
        }
    }

    /// <summary>
    /// MergeGame 인스턴스 상태를 담는 스냅샷입니다.
    /// View/클라이언트에 이 데이터를 전달해 화면을 갱신합니다.
    /// </summary>
    public sealed class MergeHostSnapshot : GameSnapshotBase
    {
        /// <summary>
        /// 현재 세션 단계입니다.
        /// </summary>
        public MergeSessionPhase SessionPhase { get; }

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// 도달한 최고 등급입니다.
        /// </summary>
        public int MaxGrade { get; }

        /// <summary>
        /// 전체 슬롯 개수입니다.
        /// </summary>
        public int TotalSlots { get; }

        /// <summary>
        /// 사용 중인 슬롯 개수입니다.
        /// </summary>
        public int UsedSlots { get; }

        /// <summary>
        /// 경과 시간(초)입니다.
        /// </summary>
        public float ElapsedTime { get; }

        /// <summary>
        /// 슬롯 상태 목록입니다.
        /// </summary>
        public IReadOnlyList<SlotSnapshot> Slots { get; }

        public MergeHostSnapshot(
            long tick,
            MergeSessionPhase sessionPhase,
            int score,
            int maxGrade,
            int totalSlots,
            int usedSlots,
            float elapsedTime,
            IReadOnlyList<SlotSnapshot> slots) : base(tick)
        {
            SessionPhase = sessionPhase;
            Score = score;
            MaxGrade = maxGrade;
            TotalSlots = totalSlots;
            UsedSlots = usedSlots;
            ElapsedTime = elapsedTime;
            Slots = slots;
        }
    }
}
