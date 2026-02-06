namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public interface ITargetingStrategy
    {
        TargetData FindTargets(AbilitySystemComponent owner, TargetContext context);
    }
}

