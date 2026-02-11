using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 몬스터 스폰 요청 내부 이벤트입니다.
    /// DifficultyModule이 발행하고 Host가 처리합니다.
    /// </summary>
    public sealed class MonsterSpawnRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 플레이어 인덱스입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// 스폰할 몬스터 ID입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 사용할 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 현재 난이도 스텝입니다.
        /// </summary>
        public int DifficultyStep { get; }

        /// <summary>
        /// 몬스터 체력 배율입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// 스폰 처리 완료 여부입니다.
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
    /// 난이도 스텝 변경 알림 내부 이벤트입니다.
    /// DifficultyModule이 발행합니다.
    /// </summary>
    public sealed class DifficultyStepChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 현재 스텝 번호입니다.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// 현재 스폰 수입니다.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// 현재 체력 배율입니다.
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// 현재 스폰 간격입니다.
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
    /// 몬스터 사망 알림 내부 이벤트입니다.
    /// </summary>
    public sealed class MonsterDiedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 플레이어 인덱스입니다.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// 사망한 몬스터 UID입니다.
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
