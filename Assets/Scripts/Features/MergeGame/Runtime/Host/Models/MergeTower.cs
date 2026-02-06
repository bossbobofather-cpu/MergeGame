using System;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Models
{
    /// <summary>
    /// 보드에 배치된 타워 정보입니다.
    /// AbilitySystemComponent를 소유하여 능력/속성을 관리합니다.
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
        public string TowerId { get; }

        /// <summary>
        /// 타워 타입입니다 (머지 매칭용: warrior, mage 등).
        /// </summary>
        public string TowerType { get; }

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
        public Point2D Position { get; set; }

        /// <summary>
        /// AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent ASC { get; }

        /// <summary>
        /// 공격 쿨타임 남은 시간입니다.
        /// </summary>
        public float AttackCooldownRemaining { get; set; }

        /// <summary>
        /// 이 타워가 머지의 소스(흡수되는 쪽)일 때 발동하는 이펙트 ID입니다.
        /// </summary>
        public string OnMergeSourceEffectId { get; }

        /// <summary>
        /// 이 타워가 머지의 타겟(남는 쪽)일 때 발동하는 이펙트 ID입니다.
        /// </summary>
        public string OnMergeTargetEffectId { get; }

        public MergeTower(
            long uid,
            string towerId,
            string towerType,
            int grade,
            int slotIndex,
            Point2D position,
            string onMergeSourceEffectId = null,
            string onMergeTargetEffectId = null)
        {
            Uid = uid;
            TowerId = towerId;
            TowerType = towerType;
            Grade = grade;
            SlotIndex = slotIndex;
            Position = position;
            OnMergeSourceEffectId = onMergeSourceEffectId;
            OnMergeTargetEffectId = onMergeTargetEffectId;

            ASC = new AbilitySystemComponent();
            ASC.SetOwner(this);
        }

        /// <summary>
        /// 등급을 설정합니다.
        /// </summary>
        public void SetGrade(int grade)
        {
            Grade = grade;
        }

        /// <summary>
        /// 같은 타입과 등급인지 확인합니다 (머지 가능 여부).
        /// </summary>
        public bool CanMergeWith(MergeTower other)
        {
            if (other == null) return false;
            if (Uid == other.Uid) return false;
            return TowerType == other.TowerType && Grade == other.Grade;
        }

        public void Dispose()
        {
            ASC?.Dispose();
        }
    }
}
