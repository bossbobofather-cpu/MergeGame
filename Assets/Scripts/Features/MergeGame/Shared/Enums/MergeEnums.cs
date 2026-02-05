namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 세션 단계입니다.
    /// </summary>
    public enum MergeSessionPhase
    {
        /// <summary>
        /// 초기 상태입니다.
        /// </summary>
        None,

        /// <summary>
        /// 게임 진행 중입니다.
        /// </summary>
        Playing,

        /// <summary>
        /// 일시 정지 상태입니다.
        /// </summary>
        Paused,

        /// <summary>
        /// 게임 오버 상태입니다.
        /// </summary>
        GameOver
    }

    /// <summary>
    /// 웨이브 단계입니다.
    /// </summary>
    public enum WavePhase
    {
        /// <summary>
        /// 대기 상태입니다.
        /// </summary>
        Idle,

        /// <summary>
        /// 몬스터 스폰 중입니다.
        /// </summary>
        Spawning,

        /// <summary>
        /// 웨이브 진행 중입니다.
        /// </summary>
        InProgress,

        /// <summary>
        /// 웨이브 완료입니다.
        /// </summary>
        Completed
    }
}
