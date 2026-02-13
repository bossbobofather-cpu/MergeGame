namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class DifficultyModuleConfig
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float DefaultSpawnInterval { get; set; } = 1f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int DefaultSpawnCount { get; set; } = 1;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float StepInterval { get; set; } = 10f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SpawnCountIncrease { get; set; } = 1;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float HealthMultiplierIncrease { get; set; } = 0.1f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.9f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long DefaultMonsterId { get; set; } = DevHelperSet.DevIdHelper.DEV_DEFAULT_MONSTER_ID;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int DefaultPathIndex { get; set; } = 0;
    }
}
