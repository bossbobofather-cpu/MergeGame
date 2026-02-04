namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?ì„± ê³„ì‚°ê¸??€?…ì…?ˆë‹¤.
    /// Calculated ValueMode?ì„œ ?¬ìš©??ê³„ì‚°ê¸°ë? ì§€?•í•©?ˆë‹¤.
    /// </summary>
    public enum AttributeCalculatorType
    {
        /// <summary>
        /// ?†ìŒ (Static ëª¨ë“œ?ì„œ ?¬ìš©)
        /// </summary>
        None = 0,

        /// <summary>
        /// ?€?ì˜ ìµœë? ì²´ë ¥ ë¹„ìœ¨ë§Œí¼ ?Œë³µ?©ë‹ˆ??
        /// </summary>
        HealByTargetMaxHealthPercent,

        /// <summary>
        /// ?œì „?ì˜ ìµœë? ì²´ë ¥ ë¹„ìœ¨ë§Œí¼ ?Œë³µ?©ë‹ˆ??
        /// </summary>
        HealBySourceMaxHealthPercent,

        /// <summary>
        /// ?€?ì˜ ?„ì¬ ì²´ë ¥??ìµœë? ì²´ë ¥?¼ë¡œ ?¤ì •?©ë‹ˆ?? (?„ì „ ?Œë³µ)
        /// </summary>
        FullHeal,
    }
}

