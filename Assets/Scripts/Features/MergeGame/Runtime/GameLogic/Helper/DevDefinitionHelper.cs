namespace MyProject.MergeGame
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public static class DevHelperSet
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static class DevIdHelper
        {
            // TowerId 정의
            public static readonly int DEV_TOWER_ID_RED = 1;
            public static readonly int DEV_TOWER_ID_GREEN = 2;
            public static readonly int DEV_TOWER_ID_BLUE = 3;
            public static readonly int DEV_DEFAULT_TOWER_ID = DEV_TOWER_ID_RED;

            // MonsterId 정의
            public static readonly int DEV_DEFAULT_MONSTER_ID = 10001;
            public static readonly int DEV_DEFAULT_BOSS_MONSTER_ID = 20001;

            // MapId 정의
            public static readonly int DEV_DEFAULT_MAP_ID = 1;
        }


        public static class DevRuleHelper
        {
            public static readonly int DEV_TOWER_INITIAL_GRADE = 1;
            public static readonly int DEV_TOWER_MAX_GRADE = 8;
            public static readonly int DEV_SCORE_PER_GRADE = 100;
            public static readonly int DEV_MAX_MONSTER_STACK = 100;
        }
    }


}
