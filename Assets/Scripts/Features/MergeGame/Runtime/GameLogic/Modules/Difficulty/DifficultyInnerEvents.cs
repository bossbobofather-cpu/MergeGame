using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class MonsterSpawnRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int DifficultyStep { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool Handled { get; set; }

        public MonsterSpawnRequestInnerEvent(
            long tick, int playerIndex, long monsterId, int pathIndex,
            int difficultyStep, float healthMultiplier)
            : base(tick)
        {
            PlayerIndex = playerIndex;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            DifficultyStep = difficultyStep;
            HealthMultiplier = healthMultiplier;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class DifficultyStepChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float SpawnInterval { get; }

        public DifficultyStepChangedInnerEvent(
            long tick, int step, int spawnCount, float healthMultiplier, float spawnInterval)
            : base(tick)
        {
            Step = step;
            SpawnCount = spawnCount;
            HealthMultiplier = healthMultiplier;
            SpawnInterval = spawnInterval;
        }
    }

    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class MonsterDiedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long MonsterUid { get; }
        public MonsterDiedInnerEvent(long tick, int playerIndex, long monsterUid)
            : base(tick)
        {
            PlayerIndex = playerIndex;
            MonsterUid = monsterUid;
        }
    }
}
