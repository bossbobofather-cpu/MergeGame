namespace Noname.GameAbilitySystem
{
    public interface IEffectDurationPolicy
    {
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration);
    }
}
