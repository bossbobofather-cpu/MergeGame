namespace MyProject.MergeGame
{
    /// <summary>
    /// 媛쒕컻???ы띁. ?ㅼ젙 媛?諛??뺤쓽?ㅼ쓣 ?쇰떒 ?ш린??愿由? 異뷀썑???곗씠?곕줈 愿由ы빐?쇳븳??
    /// </summary>
    public static class DevHelperSet
    {
        /// <summary>
        /// Id ?ы띁
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
            //????쒖옉 ?깃툒
            public static readonly int DEV_TOWER_INITIAL_GRADE = 1;

            //???理쒕? ?깃툒
            public static readonly int DEV_TOWER_MAX_GRADE = 8;

            //????깃툒???먯닔
            public static readonly int DEV_SCORE_PER_GRADE = 100;

            //紐ъ뒪???꾩쟻 ?⑤같 湲곗?
            public static readonly int DEV_MAX_MONSTER_STACK = 100;
        }
    }


}
