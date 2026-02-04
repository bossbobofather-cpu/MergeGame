namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?€κ²?? μ • ?„λµ ?Έν„°?μ΄?¤μ…?λ‹¤.
    /// </summary>
    public interface ITargetingStrategy
    {
        TargetData FindTargets(AbilitySystemComponent owner, TargetContext context);
    }
}

