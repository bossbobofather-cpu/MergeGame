namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public struct GameplayTagEventData
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public object Payload;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayTagEventData(FGameplayTag eventTag, object payload = null)
        {
            EventTag = eventTag;
            Payload = payload;
        }
    }
}
