namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 호스트 설정입니다.
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

        // 웨이브 설정
        private float _waveSpawnInterval = 1f;
        private int _waveCompletionBonusGold = 50;

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
        /// 웨이브 몬스터 스폰 간격 (초)입니다.
        /// </summary>
        public float WaveSpawnInterval => _waveSpawnInterval;

        /// <summary>
        /// 웨이브 완료 보너스 골드입니다.
        /// </summary>
        public int WaveCompletionBonusGold => _waveCompletionBonusGold;

        /// <summary>
        /// 설정을 빌더 패턴으로 구성합니다.
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

        public MergeHostConfig WithWaveSettings(float spawnInterval, int completionBonus)
        {
            _waveSpawnInterval = spawnInterval;
            _waveCompletionBonusGold = completionBonus;
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
