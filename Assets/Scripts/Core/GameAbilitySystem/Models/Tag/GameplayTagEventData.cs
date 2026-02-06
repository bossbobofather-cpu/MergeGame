namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public struct GameplayTagEventData
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// 주석 정리
        /// </summary>
        public object Payload;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagEventData(FGameplayTag eventTag, object payload = null)
        {
            // 주석 정리
            EventTag = eventTag;
            Payload = payload;
        }
    }
}
