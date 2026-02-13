using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public sealed class GameplayAbilitySpec
    {
        private readonly GameplayAbility _ability;
        private readonly FGameplayAbilitySpecHandle _handle;
        private int _level;
        private int _activeCount;
        /// <summary>
        /// GameplayAbilitySpec 함수를 처리합니다.
        /// </summary>

        public GameplayAbilitySpec(GameplayAbility ability, FGameplayAbilitySpecHandle handle, int level = 1)
        {
            // 핵심 로직을 처리합니다.
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
            _handle = handle;
            _level = level;
            _activeCount = 0;
        }
        /// <summary>
        /// GameplayAbilitySpec 함수를 처리합니다.
        /// </summary>

        public GameplayAbilitySpec(GameplayAbility ability, int handleId)
            : this(ability, new FGameplayAbilitySpecHandle { Id = handleId })
                // 핵심 로직을 처리합니다.
        {
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayAbility Ability => _ability;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public FGameplayAbilitySpecHandle Handle => _handle;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public FGameplayTag AbilityTag => _ability.AbilityTag;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _ability.ActivationRequiredTags;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _ability.ActivationBlockedTags;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public string DisplayName => _ability.DisplayName;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Math.Max(1, value);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void IncrementActiveCount()
        {
            // 핵심 로직을 처리합니다.
            _activeCount++;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void DecrementActiveCount()
        {
            // 핵심 로직을 처리합니다.
            _activeCount = Math.Max(0, _activeCount - 1);
        }
    }
}

