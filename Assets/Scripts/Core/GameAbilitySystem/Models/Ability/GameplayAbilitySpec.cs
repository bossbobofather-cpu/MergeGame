using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class GameplayAbilitySpec
    {
        private readonly GameplayAbility _ability;
        private readonly FGameplayAbilitySpecHandle _handle;
        private int _level;
        private int _activeCount;
        /// <summary>
        /// GameplayAbilitySpec 메서드입니다.
        /// </summary>

        public GameplayAbilitySpec(GameplayAbility ability, FGameplayAbilitySpecHandle handle, int level = 1)
        {
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
            _handle = handle;
            _level = level;
            _activeCount = 0;
        }
        /// <summary>
        /// GameplayAbilitySpec 메서드입니다.
        /// </summary>

        public GameplayAbilitySpec(GameplayAbility ability, int handleId)
            : this(ability, new FGameplayAbilitySpecHandle { Id = handleId })
        {
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayAbility Ability => _ability;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle => _handle;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public FGameplayTag AbilityTag => _ability.AbilityTag;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _ability.ActivationRequiredTags;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _ability.ActivationBlockedTags;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string DisplayName => _ability.DisplayName;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Math.Max(1, value);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void IncrementActiveCount()
        {
            _activeCount++;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void DecrementActiveCount()
        {
            _activeCount = Math.Max(0, _activeCount - 1);
        }
    }
}

