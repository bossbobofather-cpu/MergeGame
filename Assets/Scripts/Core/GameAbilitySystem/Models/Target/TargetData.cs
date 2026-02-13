using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 타겟팅 결과를 담는 컨테이너입니다.
    /// </summary>
    public sealed class TargetData
    {
        private readonly HashSet<AbilitySystemComponent> _targetSet = new();
        private readonly List<AbilitySystemComponent> _targets = new();
        /// <summary>
        /// TargetData 함수를 처리합니다.
        /// </summary>

        public TargetData(AbilitySystemComponent source, Point3D? hitLocation = null)
        {
            // 핵심 로직을 처리합니다.
            Source = source;
            HitLocation = hitLocation;
        }

        /// <summary>
        /// 타겟팅 기준이 되는 소스입니다.
        /// </summary>
        public AbilitySystemComponent Source { get; }

        /// <summary>
        /// 선택된 타겟 목록입니다.
        /// </summary>
        public IReadOnlyList<AbilitySystemComponent> Targets => _targets;

        /// <summary>
        /// 범위 공격 등의 충돌 지점입니다.
        /// </summary>
        public Point3D? HitLocation { get; }

        /// <summary>
        /// 타겟을 추가합니다. 중복은 무시됩니다.
        /// </summary>
        public void AddTarget(AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (target == null)
            {
                return;
            }

            if (_targetSet.Add(target))
            {
                _targets.Add(target);
            }
        }
    }
}
