namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public interface ITargetingStrategy
    {
        TargetData FindTargets(AbilitySystemComponent owner, TargetContext context);
    }
}

