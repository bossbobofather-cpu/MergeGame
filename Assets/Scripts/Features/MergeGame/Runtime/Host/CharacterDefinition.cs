namespace MyProject.MergeGame
{
    /// <summary>
    /// 캐릭터 정의 데이터입니다.
    /// Host는 이 데이터를 기반으로 캐릭터 상태(ASC 등)를 초기화합니다.
    /// </summary>
    public sealed class CharacterDefinition
    {
        public string CharacterId { get; set; }
        public string CharacterType { get; set; }
        public int InitialGrade { get; set; } = 1;
        public float BaseAttackDamage { get; set; } = 10f;
        public float BaseAttackSpeed { get; set; } = 1f;
        public float BaseAttackRange { get; set; } = 5f;

        /// <summary>
        /// 머지 시 소스 캐릭터에 적용할 이펙트 ID입니다.
        /// </summary>
        public string OnMergeSourceEffectId { get; set; }

        /// <summary>
        /// 머지 시 타겟 캐릭터에 적용할 이펙트 ID입니다.
        /// </summary>
        public string OnMergeTargetEffectId { get; set; }
    }
}
