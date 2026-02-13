namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// DifficultyModule ?ㅼ젙?낅땲??
    /// </summary>
    public sealed class DifficultyModuleConfig
    {
        /// <summary>
        /// 珥덇린 ?ㅽ룿 媛꾧꺽 (珥??낅땲??
        /// </summary>
        public float DefaultSpawnInterval { get; set; } = 1f;

        /// <summary>
        /// 珥덇린 ?ㅽ룿 ??(???ъ씠?댁뿉 ?ㅽ룿??紐ъ뒪?????낅땲??
        /// </summary>
        public int DefaultSpawnCount { get; set; } = 1;

        /// <summary>
        /// ?쒖씠???ㅽ뀦 媛꾧꺽 (珥??낅땲??
        /// </summary>
        public float StepInterval { get; set; } = 10f;

        /// <summary>
        /// ?ㅽ뀦???ㅽ룿 ??利앷??됱엯?덈떎.
        /// </summary>
        public int SpawnCountIncrease { get; set; } = 1;

        /// <summary>
        /// ?ㅽ뀦??泥대젰 諛곗쑉 利앷??됱엯?덈떎.
        /// </summary>
        public float HealthMultiplierIncrease { get; set; } = 0.1f;

        /// <summary>
        /// ?ㅽ뀦???ㅽ룿 媛꾧꺽 諛곗쑉?낅땲?? (?? 0.9 = 10% 媛먯냼)
        /// </summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.9f;

        /// <summary>
        /// 湲곕낯 紐ъ뒪??ID?낅땲??
        /// </summary>
        public long DefaultMonsterId { get; set; } = DevHelperSet.DevIdHelper.DEV_DEFAULT_MONSTER_ID;

        /// <summary>
        /// 湲곕낯 寃쎈줈 ?몃뜳?ㅼ엯?덈떎.
        /// </summary>
        public int DefaultPathIndex { get; set; } = 0;
    }
}
