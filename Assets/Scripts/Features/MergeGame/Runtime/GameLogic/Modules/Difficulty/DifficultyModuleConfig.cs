namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// DifficultyModule 설정입니다.
    /// </summary>
    public sealed class DifficultyModuleConfig
    {
        /// <summary>
        /// 초기 스폰 간격 (초)입니다.
        /// </summary>
        public float DefaultSpawnInterval { get; set; } = 1f;

        /// <summary>
        /// 초기 스폰 수 (한 사이클에 스폰할 몬스터 수)입니다.
        /// </summary>
        public int DefaultSpawnCount { get; set; } = 1;

        /// <summary>
        /// 난이도 스텝 간격 (초)입니다.
        /// </summary>
        public float StepInterval { get; set; } = 10f;

        /// <summary>
        /// 스텝당 스폰 수 증가량입니다.
        /// </summary>
        public int SpawnCountIncrease { get; set; } = 1;

        /// <summary>
        /// 스텝당 체력 배율 증가량입니다.
        /// </summary>
        public float HealthMultiplierIncrease { get; set; } = 0.1f;

        /// <summary>
        /// 스텝당 스폰 간격 배율입니다. (예: 0.9 = 10% 감소)
        /// </summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.9f;

        /// <summary>
        /// 기본 몬스터 ID입니다.
        /// </summary>
        public long DefaultMonsterId { get; set; } = DevIdHelper.DEV_DEFAULT_MONSTER_ID;

        /// <summary>
        /// 기본 경로 인덱스입니다.
        /// </summary>
        public int DefaultPathIndex { get; set; } = 0;
    }
}
