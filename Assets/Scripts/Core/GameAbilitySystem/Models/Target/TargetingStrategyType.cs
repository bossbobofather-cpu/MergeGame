namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?�겟팅 ?�략 ?�?�입?�다.
    /// </summary>
    public enum TargetingStrategyType
    {
        /// <summary>?�략 ?�음</summary>
        None,

        /// <summary>?�기 ?�신</summary>
        Self,

        /// <summary>?�덤 ??/summary>
        Random,

        /// <summary>가??가까운 ??(maxRange ?�용)</summary>
        NearestEnemy,

        /// <summary>가??가까운 N�?(maxTargets, maxRange ?�용)</summary>
        NearestN,

        /// <summary>체력 가????? ??/summary>
        LowestHp,

        /// <summary>범위 ??모든 ??(radius ?�용)</summary>
        Area,
    }
}

