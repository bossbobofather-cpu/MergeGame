namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 호스트 설정값입니다.
    /// </summary>
    public class MergeHostConfig
    {
        private int _initialUnitGrade = 1;
        private int _maxUnitGrade = 10;
        private int _scorePerGrade = 100;

        // 플레이어 설정
        private int _playerMaxHp = 100;
        private int _playerStartGold = 100;

        // 전투 설정
        private float _defaultAttackRange = 10f;

        // 몬스터 누적 패배 기준
        private int _maxMonsterStack = 100;

        /// <summary>
        /// 초기 유닛(타워) 등급입니다.
        /// </summary>
        public int InitialUnitGrade => _initialUnitGrade;

        /// <summary>
        /// 최대 유닛(타워) 등급입니다.
        /// </summary>
        public int MaxUnitGrade => _maxUnitGrade;

        /// <summary>
        /// 등급당 점수입니다.
        /// </summary>
        public int ScorePerGrade => _scorePerGrade;

        /// <summary>
        /// 플레이어 최대 HP입니다.
        /// </summary>
        public int PlayerMaxHp => _playerMaxHp;

        /// <summary>
        /// 플레이어 시작 골드입니다.
        /// </summary>
        public int PlayerStartGold => _playerStartGold;

        /// <summary>
        /// 기본 공격 범위입니다.
        /// </summary>
        public float DefaultAttackRange => _defaultAttackRange;

        /// <summary>
        /// 보드에 존재하는 몬스터 수가 이 값 이상이면 패배합니다.
        /// (0 이하이면 체크하지 않습니다.)
        /// </summary>
        public int MaxMonsterStack => _maxMonsterStack;

        /// <summary>
        /// 플레이어 HP를 설정합니다.
        /// </summary>
        public MergeHostConfig WithPlayerHp(int maxHp)
        {
            _playerMaxHp = maxHp;
            return this;
        }

        public MergeHostConfig WithStartGold(int gold)
        {
            _playerStartGold = gold;
            return this;
        }

        public MergeHostConfig WithAttackRange(float range)
        {
            _defaultAttackRange = range;
            return this;
        }

        public MergeHostConfig WithMaxMonsterStack(int maxMonsterStack)
        {
            _maxMonsterStack = maxMonsterStack;
            return this;
        }

        public MergeHostConfig WithMaxGrade(int maxGrade)
        {
            _maxUnitGrade = maxGrade;
            return this;
        }

        public MergeHostConfig WithInitialGrade(int initialGrade)
        {
            _initialUnitGrade = initialGrade;
            return this;
        }
    }
}
