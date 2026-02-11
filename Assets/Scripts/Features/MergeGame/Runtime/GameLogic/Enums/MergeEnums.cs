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
    /// 타워 공격 방식입니다.
    /// </summary>
    public enum TowerAttackType
    {
        /// <summary>
        /// 즉시 피해를 적용하는 히트스캔 방식입니다.
        /// </summary>
        HitScan,

        /// <summary>
        /// 투사체를 이용하는 방식입니다.
        /// </summary>
        Projectile
    }

    /// <summary>
    /// 투사체의 발사/충돌 방식입니다.
    /// </summary>
    public enum ProjectileType
    {
        /// <summary>
        /// 타겟을 향해 직선으로 발사합니다.
        /// </summary>
        Direct,

        /// <summary>
        /// 지정 지점으로 던지고 충돌 시점에 범위 타격을 수행합니다.
        /// </summary>
        Throw
    }

    /// <summary>
    /// 타워 기본 공격의 타겟팅 타입입니다.
    /// </summary>
    public enum TowerTargetingType
    {
        Nearest,
        Random,
        LowestHp,
        Area,
        None
    }
}
