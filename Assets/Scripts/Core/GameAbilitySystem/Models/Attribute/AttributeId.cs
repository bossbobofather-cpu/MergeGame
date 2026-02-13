namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public struct AttributeId
    {
        public string Name { get; set; }

        public AttributeId(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Equals 메서드입니다.
        /// </summary>

        public override bool Equals(object obj)
        {
            return obj is AttributeId other && Name == other.Name;
        }
        /// <summary>
        /// GetHashCode 메서드입니다.
        /// </summary>

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
        /// <summary>
        /// ToString 메서드입니다.
        /// </summary>

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
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

