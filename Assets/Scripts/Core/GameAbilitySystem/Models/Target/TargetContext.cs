using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?€ê²ŸíŒ…???„ìš”???”ë“œ ì¡°íšŒ ?¨ìˆ˜ë¥??œê³µ?˜ëŠ” ì»¨í…?¤íŠ¸?…ë‹ˆ??
    /// Host/?œë²„ ?˜ê²½?ì„œ ì£¼ì…?˜ì—¬ ?¬ìš©?©ë‹ˆ??
    /// </summary>
    public sealed class TargetContext
    {
        public TargetContext(
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getEnemies,
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getAllies,
            Func<AbilitySystemComponent, Point2D> getPosition,
            Random random = null)
        {
            GetEnemies = getEnemies;
            GetAllies = getAllies;
            GetPosition = getPosition;
            Random = random ?? new Random();
        }

        /// <summary>
        /// ?¹ì • ì£¼ì²´ ê¸°ì??¼ë¡œ ??ëª©ë¡??ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetEnemies { get; }

        /// <summary>
        /// ?¹ì • ì£¼ì²´ ê¸°ì??¼ë¡œ ?„êµ° ëª©ë¡??ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetAllies { get; }

        /// <summary>
        /// ì£¼ì²´???„ì¹˜ ?•ë³´ë¥?ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public Func<AbilitySystemComponent, Point2D> GetPosition { get; }

        /// <summary>
        /// ?œë¤ ? íƒ???„í•œ RNG?…ë‹ˆ??
        /// </summary>
        public Random Random { get; }

        public IReadOnlyList<AbilitySystemComponent> ResolveEnemies(AbilitySystemComponent owner)
        {
            return GetEnemies != null ? GetEnemies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public IReadOnlyList<AbilitySystemComponent> ResolveAllies(AbilitySystemComponent owner)
        {
            return GetAllies != null ? GetAllies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public Point2D ResolvePosition(AbilitySystemComponent owner)
        {
            return GetPosition != null ? GetPosition(owner) : default;
        }
    }
}

