namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ASC(AbilitySystemComponent)???€???‘ê·¼???œê³µ?˜ëŠ” ?¸í„°?˜ì´?¤ì…?ˆë‹¤.
    /// êµ¬ì²´?ì¸ ?´ë˜?¤ì— ?˜ì¡´?˜ì? ?Šê³  ASC ê¸°ëŠ¥???¬ìš©?˜ê¸° ?„í•´ ?„ì…?˜ì—ˆ?µë‹ˆ??
    /// </summary>
    public interface IAbilitySystemOwner
    {
        /// <summary>
        /// AbilitySystemComponent ?¸ìŠ¤?´ìŠ¤ë¥?ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        AbilitySystemComponent ASC { get; }
    }
}

