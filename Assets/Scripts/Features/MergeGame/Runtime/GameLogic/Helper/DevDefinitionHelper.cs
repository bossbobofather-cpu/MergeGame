namespace MyProject.MergeGame
{
    /// <summary>
    /// 개발용 헬퍼. 설정 값 및 정의들을 일단 여기서 관리. 추후에 데이터로 관리해야한다.
    /// </summary>
    public static class DevHelperSet
    {
        /// <summary>
        /// Id 헬퍼
        /// </summary>
        public static class DevIdHelper
        {
            //TowerId
            public static readonly int DEV_TOWER_ID_RED = 1;
            public static readonly int DEV_TOWER_ID_GREEN = 2;
            public static readonly int DEV_TOWER_ID_BLUE = 3;
            public static readonly int DEV_DEFAULT_TOWER_ID = DEV_TOWER_ID_RED;

            //MonsterId
            public static readonly int DEV_DEFAULT_MONSTER_ID = 10001;
            public static readonly int DEV_DEFAULT_BOSS_MONSTER_ID = 20001;

            //MapId
            public static readonly int DEV_DEFAULT_MAP_ID = 1;
        }


        public static class DevRuleHelper
        {
            //타워 시작 등급
            public static readonly int DEV_TOWER_INITIAL_GRADE = 1;

            //타워 최대 등급
            public static readonly int DEV_TOWER_MAX_GRADE = 8;

            //타워 등급당 점수
            public static readonly int DEV_SCORE_PER_GRADE = 100;

            //몬스터 누적 패배 기준
            public static readonly int DEV_MAX_MONSTER_STACK = 100;
        }
    }


}