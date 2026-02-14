namespace MyProject.MergeGame
{
    /// <summary>
        /// DevHelperSet 클래스입니다.
        /// </summary>
    public static class DevHelperSet
    {
        /// <summary>
        /// DevIdHelper 클래스입니다.
        /// </summary>
        public static class DevIdHelper
        {
            // TowerId 정의
            /// <summary>
            /// DEV_TOWER_ID_RED 필드입니다.
            /// </summary>
            public static readonly int DEV_TOWER_ID_RED = 1;
            /// <summary>
            /// DEV_TOWER_ID_GREEN 필드입니다.
            /// </summary>
            public static readonly int DEV_TOWER_ID_GREEN = 2;
            /// <summary>
            /// DEV_TOWER_ID_BLUE 필드입니다.
            /// </summary>
            public static readonly int DEV_TOWER_ID_BLUE = 3;
            /// <summary>
            /// DEV_DEFAULT_TOWER_ID 필드입니다.
            /// </summary>
            public static readonly int DEV_DEFAULT_TOWER_ID = DEV_TOWER_ID_RED;

            // MonsterId 정의
            /// <summary>
            /// DEV_DEFAULT_MONSTER_ID 필드입니다.
            /// </summary>
            public static readonly int DEV_DEFAULT_MONSTER_ID = 10001;
            /// <summary>
            /// DEV_DEFAULT_BOSS_MONSTER_ID 필드입니다.
            /// </summary>
            public static readonly int DEV_DEFAULT_BOSS_MONSTER_ID = 20001;

            // MapId 정의
            /// <summary>
            /// DEV_DEFAULT_MAP_ID 필드입니다.
            /// </summary>
            public static readonly int DEV_DEFAULT_MAP_ID = 1;
        }


        /// <summary>
        /// DevRuleHelper 클래스입니다.
        /// </summary>
        public static class DevRuleHelper
        {
            /// <summary>
            /// DEV_TOWER_INITIAL_GRADE 필드입니다.
            /// </summary>
            public static readonly int DEV_TOWER_INITIAL_GRADE = 1;
            /// <summary>
            /// DEV_TOWER_MAX_GRADE 필드입니다.
            /// </summary>
            public static readonly int DEV_TOWER_MAX_GRADE = 8;
            /// <summary>
            /// DEV_SCORE_PER_GRADE 필드입니다.
            /// </summary>
            public static readonly int DEV_SCORE_PER_GRADE = 100;
            /// <summary>
            /// DEV_MAX_MONSTER_STACK 필드입니다.
            /// </summary>
            public static readonly int DEV_MAX_MONSTER_STACK = 100;
        }
    }


}
