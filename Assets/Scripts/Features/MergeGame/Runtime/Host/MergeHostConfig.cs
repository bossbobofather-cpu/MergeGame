namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 호스트 설정입니다.
    /// </summary>
    public class MergeHostConfig
    {
        private int _slotCount = 12;
        private int _initialUnitGrade = 1;

        private int _maxUnitGrade = 10;

        private int _scorePerGrade = 100;

        /// <summary>
        /// 전체 슬롯 개수입니다.
        /// </summary>
        public int SlotCount => _slotCount;

        /// <summary>
        /// 초기 유닛 등급입니다.
        /// </summary>
        public int InitialUnitGrade => _initialUnitGrade;

        /// <summary>
        /// 최대 유닛 등급입니다.
        /// </summary>
        public int MaxUnitGrade => _maxUnitGrade;

        /// <summary>
        /// 등급당 점수입니다.
        /// </summary>
        public int ScorePerGrade => _scorePerGrade;
    }
}
