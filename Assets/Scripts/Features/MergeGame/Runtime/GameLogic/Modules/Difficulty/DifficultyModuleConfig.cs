namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// DifficultyModuleConfig 클래스입니다.
        /// </summary>
    public sealed class DifficultyModuleConfig
    {
        /// <summary>
        /// DefaultSpawnInterval 속성입니다.
        /// </summary>
        public float DefaultSpawnInterval { get; set; } = 1f;

        /// <summary>
        /// DefaultSpawnCount 속성입니다.
        /// </summary>
        public int DefaultSpawnCount { get; set; } = 1;

        /// <summary>
        /// StepInterval 속성입니다.
        /// </summary>
        public float StepInterval { get; set; } = 10f;

        /// <summary>
        /// SpawnCountIncrease 속성입니다.
        /// </summary>
        public int SpawnCountIncrease { get; set; } = 1;

        /// <summary>
        /// HealthMultiplierIncrease 속성입니다.
        /// </summary>
        public float HealthMultiplierIncrease { get; set; } = 0.1f;

        /// <summary>
        /// SpawnIntervalMultiplier 속성입니다.
        /// </summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.9f;

        /// <summary>
        /// DefaultMonsterId 속성입니다.
        /// </summary>
        public long DefaultMonsterId { get; set; } = DevHelperSet.DevIdHelper.DEV_DEFAULT_MONSTER_ID;

        /// <summary>
        /// DefaultPathIndex 속성입니다.
        /// </summary>
        public int DefaultPathIndex { get; set; } = 0;
    }
}
