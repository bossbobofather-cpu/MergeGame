using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// MonsterSpawnRequestInnerEvent 클래스입니다.
        /// </summary>
    public sealed class MonsterSpawnRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// PlayerIndex 속성입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// MonsterId 속성입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// PathIndex 속성입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// DifficultyStep 속성입니다.
        /// </summary>
        public int DifficultyStep { get; }

        /// <summary>
        /// HealthMultiplier 속성입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// Handled 속성입니다.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// MonsterSpawnRequestInnerEvent 생성자입니다.
        /// </summary>
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
        /// DifficultyStepChangedInnerEvent 클래스입니다.
        /// </summary>
    public sealed class DifficultyStepChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// Step 속성입니다.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// SpawnCount 속성입니다.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// HealthMultiplier 속성입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// SpawnInterval 속성입니다.
        /// </summary>
        public float SpawnInterval { get; }

        /// <summary>
        /// DifficultyStepChangedInnerEvent 생성자입니다.
        /// </summary>
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
        /// MonsterDiedInnerEvent 클래스입니다.
        /// </summary>
    public sealed class MonsterDiedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// PlayerIndex 속성입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// MonsterUid 속성입니다.
        /// </summary>
        public long MonsterUid { get; }
        /// <summary>
        /// MonsterDiedInnerEvent 생성자입니다.
        /// </summary>
        public MonsterDiedInnerEvent(long tick, int playerIndex, long monsterUid)
            : base(tick)
        {
            PlayerIndex = playerIndex;
            MonsterUid = monsterUid;
        }
    }
}
