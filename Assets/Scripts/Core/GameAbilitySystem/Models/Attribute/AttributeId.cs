namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public struct AttributeId
    {
        public string Name { get; set; }

        public AttributeId(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Equals 함수를 처리합니다.
        /// </summary>

        public override bool Equals(object obj)
        {
            // 핵심 로직을 처리합니다.
            return obj is AttributeId other && Name == other.Name;
        }
        /// <summary>
        /// GetHashCode 함수를 처리합니다.
        /// </summary>

        public override int GetHashCode()
        {
            // 핵심 로직을 처리합니다.
            return Name != null ? Name.GetHashCode() : 0;
        }
        /// <summary>
        /// ToString 함수를 처리합니다.
        /// </summary>

        public override string ToString()
        {
            // 핵심 로직을 처리합니다.
            return Name ?? string.Empty;
        }

        // 二쇱꽍 ?뺣━
        public static readonly AttributeId Level = new("Level");
        public static readonly AttributeId MoveSpeed = new AttributeId("MoveSpeed");
        public static readonly AttributeId JumpSpeed = new AttributeId("JumpSpeed");
        public static readonly AttributeId Health = new AttributeId("Health");
        public static readonly AttributeId AttackRange = new AttributeId("AttackRange");
        public static readonly AttributeId AttackSpeed = new AttributeId("AttackSpeed");
        public static readonly AttributeId AttackDamage = new AttributeId("AttackDamage");
        public static readonly AttributeId Defense = new("Defense");
        public static readonly AttributeId MaxHealth = new AttributeId("MaxHealth");
        public static readonly AttributeId ExtraTargetCount = new("ExtraTargetCount");
        public static readonly AttributeId Exp = new AttributeId("Exp");
        public static readonly AttributeId Gold = new AttributeId("Gold");
        public static readonly AttributeId Experience = new("Experience");
        public static readonly AttributeId ExpReward = new("ExpReward");
    }
}

