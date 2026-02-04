using System;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// ?€ê²ŸíŒ… ?„ëµ JSON DTO
    /// </summary>
    [Serializable]
    public class TargetingStrategyDto
    {
        /// <summary>
        /// ?„ëµ ?€?? "Self", "NearestEnemy", "NearestN", "LowestHp", "Area", "Random"
        /// </summary>
        public string Type;

        /// <summary>
        /// ìµœë? ?¬ê±°ë¦?(NearestEnemy, NearestN?ì„œ ?¬ìš©)
        /// </summary>
        public float MaxRange;

        /// <summary>
        /// ìµœë? ?€ê²???(NearestN?ì„œ ?¬ìš©)
        /// </summary>
        public int MaxTargets;

        /// <summary>
        /// ë²”ìœ„ ë°˜ê²½ (Area?ì„œ ?¬ìš©)
        /// </summary>
        public float Radius;
    }
}

