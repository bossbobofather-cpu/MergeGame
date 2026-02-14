using System.Collections.Generic;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 타워 정의 데이터입니다.
    /// Host는 이 데이터를 기반으로 타워 상태(ASC 등)를 초기화합니다.
    /// </summary>
    public sealed class TowerDefinition
    {
        /// <summary>
        /// TowerId 속성입니다.
        /// </summary>
        public long TowerId { get; set; }
        /// <summary>
        /// InitialGrade 속성입니다.
        /// </summary>
        public int InitialGrade { get; set; } = 1;
        /// <summary>
        /// BaseAttackDamage 속성입니다.
        /// </summary>
        public float BaseAttackDamage { get; set; } = 10f;
        /// <summary>
        /// BaseAttackSpeed 속성입니다.
        /// </summary>
        public float BaseAttackSpeed { get; set; } = 1f;
        /// <summary>
        /// BaseAttackRange 속성입니다.
        /// </summary>
        public float BaseAttackRange { get; set; } = 5f;

        /// <summary>
        /// AttackType 속성입니다.
        /// </summary>
        public TowerAttackType AttackType { get; set; } = TowerAttackType.HitScan;
        /// <summary>
        /// ProjectileType 속성입니다.
        /// </summary>
        public ProjectileType ProjectileType { get; set; } = ProjectileType.Direct;
        /// <summary>
        /// ProjectileSpeed 속성입니다.
        /// </summary>
        public float ProjectileSpeed { get; set; } = 8f;
        /// <summary>
        /// ThrowRadius 속성입니다.
        /// </summary>
        public float ThrowRadius { get; set; } = 1.5f;
        /// <summary>
        /// TrapDelay 속성입니다.
        /// </summary>
        public float TrapDelay { get; set; } = 0f;

        /// <summary>
        /// TargetingType 속성입니다.
        /// </summary>
        public TowerTargetingType TargetingType { get; set; } = TowerTargetingType.Nearest;

        /// <summary>
        /// 머지 시 소스 타워에서 발동할 이펙트 목록입니다.
        /// </summary>
        public List<GameplayEffect> OnMergeSourceEffects { get; set; } = new();

        /// <summary>
        /// 머지 시 타겟 타워에서 발동할 이펙트 목록입니다.
        /// </summary>
        public List<GameplayEffect> OnMergeTargetEffects { get; set; } = new();
    }
}
