namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?¨ê³¼ ì§€?ì‹œê°??•ì±… ?€?…ì…?ˆë‹¤.
    /// </summary>
    public enum EffectDurationPolicyType
    {
        /// <summary>?•ì±… ?†ìŒ (ê¸°ë³¸ Duration ?¬ìš©)</summary>
        None,

        /// <summary>ê³µê²© ?ë„ ê¸°ë°˜ ì¿¨ë‹¤??ê³„ì‚°</summary>
        BaseAttackCooldown,

        // ?¥í›„ ì¶”ê? ê°€??
        // CooldownReduction,    // ì¿¨ë‹¤??ê°ì†Œ ?ìš©
        // SkillHaste,           // ?¤í‚¬ ê°€???ìš©
    }
}

