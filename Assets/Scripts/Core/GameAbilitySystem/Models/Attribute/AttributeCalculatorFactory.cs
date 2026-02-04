namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?์ฑ ๊ณ์ฐ๊ธ??ฉํ ๋ฆฌ์…?๋ค.
    /// </summary>
    public static class AttributeCalculatorFactory
    {
        /// <summary>
        /// ๊ณ์ฐ๊ธ??€?…์— ?ด๋น?๋” ?ธ์ค?ด์ค๋ฅ?๋ฐํ?ฉ๋??
        /// </summary>
        public static IAttributeCalculator Create(AttributeCalculatorType type)
        {
            return type switch
            {
                AttributeCalculatorType.HealByTargetMaxHealthPercent => HealByTargetMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.HealBySourceMaxHealthPercent => HealBySourceMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.FullHeal => FullHealCalculator.Instance,
                _ => null
            };
        }

        /// <summary>
        /// ๋ฌธ์???€?…๋ช…?ผ๋ก ๊ณ์ฐ๊ธ??ธ์ค?ด์ค๋ฅ?๋ฐํ?ฉ๋?? (JSON ??ง?ฌํ”??
        /// </summary>
        public static IAttributeCalculator Create(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName switch
            {
                "HealByTargetMaxHealthPercent" => HealByTargetMaxHealthPercentCalculator.Instance,
                "HealBySourceMaxHealthPercent" => HealBySourceMaxHealthPercentCalculator.Instance,
                "FullHeal" => FullHealCalculator.Instance,
                _ => null
            };
        }

        /// <summary>
        /// ๊ณ์ฐ๊ธ??ธ์ค?ด์ค???€?…๋ช…??๋ฐํ?ฉ๋?? (JSON ์ง๋ ฌ?”์ฉ)
        /// </summary>
        public static string GetTypeName(IAttributeCalculator calculator)
        {
            return calculator switch
            {
                HealByTargetMaxHealthPercentCalculator => "HealByTargetMaxHealthPercent",
                HealBySourceMaxHealthPercentCalculator => "HealBySourceMaxHealthPercent",
                FullHealCalculator => "FullHeal",
                _ => null
            };
        }
    }
}

