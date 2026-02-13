namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public struct GameplayTagEventData
    {
        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public object Payload;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayTagEventData(FGameplayTag eventTag, object payload = null)
        {
            // 二쇱꽍 ?뺣━
            EventTag = eventTag;
            Payload = payload;
        }
    }
}
