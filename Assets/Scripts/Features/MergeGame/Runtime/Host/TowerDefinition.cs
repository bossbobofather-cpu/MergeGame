namespace MyProject.MergeGame
{
    /// <summary>
    /// 타워 정의 데이터입니다.
    /// Host는 이 데이터를 기반으로 타워 상태(ASC 등)를 초기화합니다.
    /// </summary>
    public sealed class TowerDefinition
    {
        public string TowerId { get; set; }
        public string TowerType { get; set; }
        public int InitialGrade { get; set; } = 1;
        public float BaseAttackDamage { get; set; } = 10f;
        public float BaseAttackSpeed { get; set; } = 1f;
        public float BaseAttackRange { get; set; } = 5f;

        /// <summary>
        /// 머지 시 소스 타워에 적용할 이펙트 ID입니다.
        /// </summary>
        public string OnMergeSourceEffectId { get; set; }

        /// <summary>
        /// 머지 시 타겟 타워에 적용할 이펙트 ID입니다.
        /// </summary>
        public string OnMergeTargetEffectId { get; set; }
    }
}
