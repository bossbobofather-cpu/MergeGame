using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 紐ъ뒪???ㅽ룿 ?붿껌 ?대? ?대깽?몄엯?덈떎.
    /// DifficultyModule??諛쒗뻾?섍퀬 Host媛 泥섎━?⑸땲??
    /// </summary>
    public sealed class MonsterSpawnRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?뚮젅?댁뼱 ?몃뜳?ㅼ엯?덈떎.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// ?ㅽ룿??紐ъ뒪??ID?낅땲??
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// ?ъ슜??寃쎈줈 ?몃뜳?ㅼ엯?덈떎.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// ?꾩옱 ?쒖씠???ㅽ뀦?낅땲??
        /// </summary>
        public int DifficultyStep { get; }

        /// <summary>
        /// 紐ъ뒪??泥대젰 諛곗쑉?낅땲??
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// ?ㅽ룿 泥섎━ ?꾨즺 ?щ??낅땲??
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
    /// ?쒖씠???ㅽ뀦 蹂寃??뚮┝ ?대? ?대깽?몄엯?덈떎.
    /// DifficultyModule??諛쒗뻾?⑸땲??
    /// </summary>
    public sealed class DifficultyStepChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?꾩옱 ?ㅽ뀦 踰덊샇?낅땲??
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// ?꾩옱 ?ㅽ룿 ?섏엯?덈떎.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// ?꾩옱 泥대젰 諛곗쑉?낅땲??
        /// </summary>
        public float HealthMultiplier { get; }

        /// <summary>
        /// ?꾩옱 ?ㅽ룿 媛꾧꺽?낅땲??
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
    /// 紐ъ뒪???щ쭩 ?뚮┝ ?대? ?대깽?몄엯?덈떎.
    /// </summary>
    public sealed class MonsterDiedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// ?뚮젅?댁뼱 ?몃뜳?ㅼ엯?덈떎.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// ?щ쭩??紐ъ뒪??UID?낅땲??
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
