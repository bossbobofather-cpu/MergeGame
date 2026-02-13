namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public interface ITargetingStrategy
    {
        TargetData FindTargets(AbilitySystemComponent owner, TargetContext context);
    }
}

