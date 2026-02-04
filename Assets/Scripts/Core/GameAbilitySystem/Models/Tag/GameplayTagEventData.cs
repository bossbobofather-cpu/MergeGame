namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ê²Œì„?Œë ˆ???´ë²¤???„ë‹¬???°ì´?°ì…?ˆë‹¤.
    /// </summary>
    public struct GameplayTagEventData
    {
        /// <summary>
        /// ?´ë²¤???œê·¸?…ë‹ˆ??
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// ?´ë²¤???°ì´?°ì…?ˆë‹¤.
        /// </summary>
        public object Payload;

        /// <summary>
        /// ?´ë²¤???œê·¸?€ ?°ì´?°ë¡œ ?ì„±?©ë‹ˆ??
        /// </summary>
        public GameplayTagEventData(FGameplayTag eventTag, object payload = null)
        {
            // ?„ë‹¬ ?•ë³´ë¥?ê·¸ë?ë¡?ë³´ê?
            EventTag = eventTag;
            Payload = payload;
        }
    }
}
