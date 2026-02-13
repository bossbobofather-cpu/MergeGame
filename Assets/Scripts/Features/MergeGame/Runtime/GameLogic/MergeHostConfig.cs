namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 호스트 설정값입니다.
    /// </summary>
    public class MergeHostConfig
    {
        private int _initialTowerGrade = DevHelperSet.DevRuleHelper.DEV_TOWER_INITIAL_GRADE;

        private int _maxTowerGrade = DevHelperSet.DevRuleHelper.DEV_TOWER_MAX_GRADE;

        private int _scorePerGrade = DevHelperSet.DevRuleHelper.DEV_SCORE_PER_GRADE;

        private int _playerStartGold = 100;

        // 몬스터 누적 패배 기준
        private int _maxMonsterStack = DevHelperSet.DevRuleHelper.DEV_MAX_MONSTER_STACK;

        /// <summary>
        /// 초기 유닛(타워) 등급입니다.
        /// </summary>
        public int InitialTowerGrade => _initialTowerGrade;

        /// <summary>
        /// 최대 유닛(타워) 등급입니다.
        /// </summary>
        public int MaxTowerGrade => _maxTowerGrade;

        /// <summary>
        /// 등급당 점수입니다.
        /// </summary>
        public int ScorePerGrade => _scorePerGrade;

        /// <summary>
        /// 플레이어 시작 골드입니다.
        /// </summary>
        public int PlayerStartGold => _playerStartGold;

        /// <summary>
        /// 보드에 존재하는 몬스터 수가 이 값 이상이면 패배합니다.
        /// (0 이하이면 체크하지 않습니다.)
        /// </summary>
        public int MaxMonsterStack => _maxMonsterStack;
        /// <summary>
        /// WithStartGold 함수를 처리합니다.
        /// </summary>

        public MergeHostConfig WithStartGold(int gold)
        {
            // 핵심 로직을 처리합니다.
            _playerStartGold = gold;
            return this;
        }
        /// <summary>
        /// WithMaxMonsterStack 함수를 처리합니다.
        /// </summary>

        public MergeHostConfig WithMaxMonsterStack(int maxMonsterStack)
        {
            // 핵심 로직을 처리합니다.
            _maxMonsterStack = maxMonsterStack;
            return this;
        }
        /// <summary>
        /// WithMaxGrade 함수를 처리합니다.
        /// </summary>

        public MergeHostConfig WithMaxGrade(int maxGrade)
        {
            // 핵심 로직을 처리합니다.
            _maxTowerGrade = maxGrade;
            return this;
        }
        /// <summary>
        /// WithInitialGrade 함수를 처리합니다.
        /// </summary>

        public MergeHostConfig WithInitialGrade(int initialGrade)
        {
            // 핵심 로직을 처리합니다.
            _initialTowerGrade = initialGrade;
            return this;
        }
    }
}
