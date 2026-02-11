using System;
using System.Collections.Generic;
using MyProject.MergeGame.AI;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Models
{
    /// <summary>
    /// 슬롯에 배치되는 타워 엔티티입니다.
    /// AbilitySystemComponent를 통해 능력/속성을 관리합니다.
    /// </summary>
    public sealed class MergeTower : IAbilitySystemOwner, IDisposable
    {
        /// <summary>
        /// 타워 고유 ID입니다.
        /// </summary>
        public long Uid { get; }

        /// <summary>
        /// 타워 정의 ID입니다.
        /// </summary>
        public long TowerId { get; }

        /// <summary>
        /// 타워 등급입니다.
        /// </summary>
        public int Grade { get; private set; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>
        /// 타워 위치입니다.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent ASC { get; }

        /// <summary>
        /// 타워 AI입니다.
        /// </summary>
        public IMergeTowerAI AI { get; private set; }

        /// <summary>
        /// 공격 방식입니다.
        /// </summary>
        public TowerAttackType AttackType { get; }

        /// <summary>
        /// 투사체 타입입니다.
        /// </summary>
        public ProjectileType ProjectileType { get; }

        /// <summary>
        /// 투사체 이동 속도입니다.
        /// </summary>
        public float ProjectileSpeed { get; }

        /// <summary>
        /// Throw 타입의 반경입니다.
        /// </summary>
        public float ThrowRadius { get; }

        /// <summary>
        /// Throw 착지 후 트랩 감지 대기 시간입니다.
        /// </summary>
        public float TrapDelay { get; }

        /// <summary>
        /// 공격 쿨다운 잔여 시간입니다.
        /// (현재는 Ability 기반 쿨다운을 사용하므로 미사용)
        /// </summary>
        public float AttackCooldownRemaining { get; set; }

        /// <summary>
        /// 머지 소스(흡수되는 쪽)에서 발동할 이펙트 목록입니다.
        /// </summary>
        public List<GameplayEffect> OnMergeSourceEffects { get; }

        /// <summary>
        /// 머지 타겟(남는 쪽)에서 발동할 이펙트 목록입니다.
        /// </summary>
        public List<GameplayEffect> OnMergeTargetEffects { get; }

        public MergeTower(
            long uid,
            long towerId,
            int grade,
            int slotIndex,
            Point3D position,
            TowerAttackType attackType,
            ProjectileType projectileType,
            float projectileSpeed,
            float throwRadius,
            float trapDelay = 0f,
            List<GameplayEffect> onMergeSourceEffects = null,
            List<GameplayEffect> onMergeTargetEffects = null)
        {
            Uid = uid;
            TowerId = towerId;
            Grade = grade;
            SlotIndex = slotIndex;
            Position = position;
            AttackType = attackType;
            ProjectileType = projectileType;
            ProjectileSpeed = projectileSpeed;
            ThrowRadius = throwRadius;
            TrapDelay = trapDelay;
            OnMergeSourceEffects = onMergeSourceEffects != null ? new List<GameplayEffect>(onMergeSourceEffects) : new List<GameplayEffect>();
            OnMergeTargetEffects = onMergeTargetEffects != null ? new List<GameplayEffect>(onMergeTargetEffects) : new List<GameplayEffect>();

            ASC = new AbilitySystemComponent();
            ASC.SetOwner(this);
        }

        /// <summary>
        /// AI를 지정합니다.
        /// </summary>
        public void SetAI(IMergeTowerAI ai)
        {
            AI = ai;
        }

        /// <summary>
        /// 등급을 변경합니다.
        /// </summary>
        public void SetGrade(int grade)
        {
            Grade = grade;
        }

        /// <summary>
        /// 같은 타입/등급인지 확인합니다. (머지 가능 여부)
        /// </summary>
        public bool CanMergeWith(MergeTower other)
        {
            if (other == null) return false;
            if (Uid == other.Uid) return false;
            return TowerId == other.TowerId && Grade == other.Grade;
        }

        public void Dispose()
        {
            ASC?.Dispose();
        }
    }
}
