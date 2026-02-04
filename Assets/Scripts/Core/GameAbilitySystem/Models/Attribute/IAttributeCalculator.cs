namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?μ„± κ³„μ‚°κΈ??Έν„°?μ΄?¤μ…?λ‹¤.
    /// source?€ target??ASC ?•λ³΄λ¥?κΈ°λ°?Όλ΅ ?μ„± κ°’μ„ κ³„μ‚°?κ³  ?μ©?©λ‹??
    /// </summary>
    public interface IAttributeCalculator
    {
        /// <summary>
        /// ?μ„± κ°’μ„ κ³„μ‚°?κ³  ?μ©?©λ‹??
        /// </summary>
        /// <param name="source">?¨κ³Ό ?μ „?μ ASC (null?????μ)</param>
        /// <param name="target">?¨κ³Ό ?€?μ ASC</param>
        void Apply(AbilitySystemComponent source, AbilitySystemComponent target);
    }
}

