using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// 주석 정리
    /// </summary>
    public sealed class GameplayAbilitySpec
    {
        private readonly GameplayAbility _ability;
        private readonly FGameplayAbilitySpecHandle _handle;
        private int _level;
        private int _activeCount;

        public GameplayAbilitySpec(GameplayAbility ability, FGameplayAbilitySpecHandle handle, int level = 1)
        {
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
            _handle = handle;
            _level = level;
            _activeCount = 0;
        }

        public GameplayAbilitySpec(GameplayAbility ability, int handleId)
            : this(ability, new FGameplayAbilitySpecHandle { Id = handleId })
        {
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayAbility Ability => _ability;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public FGameplayAbilitySpecHandle Handle => _handle;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public FGameplayTag AbilityTag => _ability.AbilityTag;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _ability.ActivationRequiredTags;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _ability.ActivationBlockedTags;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public string DisplayName => _ability.DisplayName;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Math.Max(1, value);
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public void IncrementActiveCount()
        {
            _activeCount++;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public void DecrementActiveCount()
        {
            _activeCount = Math.Max(0, _activeCount - 1);
        }
    }
}

