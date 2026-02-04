using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?¥ë ¥ ?¬ì–‘ ?•ë³´ë¥??´ëŠ” êµ¬ì¡°?…ë‹ˆ??
    /// ASC??ë¶€?¬ëœ ?¥ë ¥???°í????íƒœë¥?ê´€ë¦¬í•©?ˆë‹¤.
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
        /// ?¥ë ¥ ?•ì˜?…ë‹ˆ??
        /// </summary>
        public GameplayAbility Ability => _ability;

        /// <summary>
        /// ?¥ë ¥ ?¸ë“¤?…ë‹ˆ??
        /// </summary>
        public FGameplayAbilitySpecHandle Handle => _handle;

        /// <summary>
        /// ?¥ë ¥ ?œê·¸?…ë‹ˆ??
        /// </summary>
        public FGameplayTag AbilityTag => _ability.AbilityTag;

        /// <summary>
        /// ?œì„±?”ì— ?„ìˆ˜ë¡??„ìš”???œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _ability.ActivationRequiredTags;

        /// <summary>
        /// ?œì„±?”ë? ì°¨ë‹¨?˜ëŠ” ?œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _ability.ActivationBlockedTags;

        /// <summary>
        /// ?œì‹œ ?´ë¦„?…ë‹ˆ??
        /// </summary>
        public string DisplayName => _ability.DisplayName;

        /// <summary>
        /// ?¥ë ¥ ?ˆë²¨?…ë‹ˆ??
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Math.Max(1, value);
        }

        /// <summary>
        /// ?œì„±??ì¤‘ì¸ ?Ÿìˆ˜?…ë‹ˆ??
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// ?œì„±??ì¹´ìš´?¸ë? ì¦ê??œí‚µ?ˆë‹¤.
        /// </summary>
        public void IncrementActiveCount()
        {
            _activeCount++;
        }

        /// <summary>
        /// ?œì„±??ì¹´ìš´?¸ë? ê°ì†Œ?œí‚µ?ˆë‹¤.
        /// </summary>
        public void DecrementActiveCount()
        {
            _activeCount = Math.Max(0, _activeCount - 1);
        }
    }
}

