using Noname.GameHost;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 이벤트의 기본 타입입니다.
    /// </summary>
    public abstract class MergeHostEvent : GameEventBase
    {
        protected MergeHostEvent(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 게임 시작 이벤트입니다.
    /// </summary>
    public sealed class MergeGameStartedEvent : MergeHostEvent
    {
        /// <summary>
        /// 초기 슬롯 개수입니다.
        /// </summary>
        public int SlotCount { get; }

        public MergeGameStartedEvent(long tick, int slotCount) : base(tick)
        {
            SlotCount = slotCount;
        }
    }

    /// <summary>
    /// 유닛 스폰 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitSpawnedEvent : MergeHostEvent
    {
        /// <summary>
        /// 생성된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 유닛 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitSpawnedEvent(long tick, long unitUid, int grade, int slotIndex) : base(tick)
        {
            UnitUid = unitUid;
            Grade = grade;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 머지 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitMergedEvent : MergeHostEvent
    {
        /// <summary>
        /// 머지에 사용된 유닛1 UID입니다.
        /// </summary>
        public long SourceUnitUid1 { get; }

        /// <summary>
        /// 머지에 사용된 유닛2 UID입니다.
        /// </summary>
        public long SourceUnitUid2 { get; }

        /// <summary>
        /// 머지 결과 유닛 UID입니다.
        /// </summary>
        public long ResultUnitUid { get; }

        /// <summary>
        /// 머지 결과 유닛 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 유닛이 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitMergedEvent(
            long tick,
            long sourceUnitUid1,
            long sourceUnitUid2,
            long resultUnitUid,
            int resultGrade,
            int slotIndex) : base(tick)
        {
            SourceUnitUid1 = sourceUnitUid1;
            SourceUnitUid2 = sourceUnitUid2;
            ResultUnitUid = resultUnitUid;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 제거 이벤트입니다.
    /// </summary>
    public sealed class MergeUnitRemovedEvent : MergeHostEvent
    {
        /// <summary>
        /// 제거된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 제거된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        public MergeUnitRemovedEvent(long tick, long unitUid, int slotIndex) : base(tick)
        {
            UnitUid = unitUid;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 게임 오버 이벤트입니다.
    /// </summary>
    public sealed class MergeGameOverEvent : MergeHostEvent
    {
        /// <summary>
        /// 승리 여부입니다.
        /// </summary>
        public bool IsVictory { get; }

        /// <summary>
        /// 최종 점수입니다.
        /// </summary>
        public int FinalScore { get; }

        /// <summary>
        /// 도달한 최고 등급입니다.
        /// </summary>
        public int MaxGradeReached { get; }

        public MergeGameOverEvent(long tick, bool isVictory, int finalScore, int maxGradeReached) : base(tick)
        {
            IsVictory = isVictory;
            FinalScore = finalScore;
            MaxGradeReached = maxGradeReached;
        }
    }

    /// <summary>
    /// 점수 변경 이벤트입니다.
    /// </summary>
    public sealed class MergeScoreChangedEvent : MergeHostEvent
    {
        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int CurrentScore { get; }

        /// <summary>
        /// 점수 변화량입니다.
        /// </summary>
        public int ScoreDelta { get; }

        public MergeScoreChangedEvent(long tick, int currentScore, int scoreDelta) : base(tick)
        {
            CurrentScore = currentScore;
            ScoreDelta = scoreDelta;
        }
    }
}
