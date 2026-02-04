using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?€ê²ŸíŒ… ê²°ê³¼ë¥??´ëŠ” ?°ì´?°ì…?ˆë‹¤.
    /// </summary>
    public sealed class TargetData
    {
        private readonly HashSet<AbilitySystemComponent> _targetSet = new();
        private readonly List<AbilitySystemComponent> _targets = new();

        public TargetData(AbilitySystemComponent source, Point2D? hitLocation = null)
        {
            Source = source;
            HitLocation = hitLocation;
        }

        /// <summary>
        /// ?€ê²ŸíŒ…???˜í–‰??ì£¼ì²´?…ë‹ˆ??
        /// </summary>
        public AbilitySystemComponent Source { get; }

        /// <summary>
        /// ?€ê²?ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public IReadOnlyList<AbilitySystemComponent> Targets => _targets;

        /// <summary>
        /// ë²”ìœ„ ê³µê²© ?±ì—???¬ìš©??ê¸°ì? ?„ì¹˜?…ë‹ˆ??
        /// </summary>
        public Point2D? HitLocation { get; }

        /// <summary>
        /// ?€ê²Ÿì„ ì¶”ê??©ë‹ˆ??
        /// </summary>
        public void AddTarget(AbilitySystemComponent target)
        {
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

