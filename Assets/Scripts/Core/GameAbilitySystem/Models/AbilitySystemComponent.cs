using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// AbilitySystemComponent ?íƒœ ëª¨ë¸?…ë‹ˆ??(?œìˆ˜ C# ëª¨ë¸).
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¤ë ˆ???ˆì „?˜ê²Œ ?™ì‘?©ë‹ˆ??
    /// </summary>
    public sealed class AbilitySystemComponent : IDisposable
    {
        public struct ActiveGameplayEffect
        {
            public long EffectUid;
            public GameplayEffect Effect;
            public float EndTime;
        }

        private IAbilitySystemOwner _onwer;

        private readonly object _modelLock = new();
        private readonly AttributeSet _attributes;
        private readonly GameplayTagContainer _ownedTags;
        private readonly List<GameplayAbility> _abilities = new();
        private readonly List<GameplayAbilitySpec> _abilitySpecs = new();
        private readonly Dictionary<int, int> _effectTagCounts = new();
        private readonly Dictionary<int, int> _looseTagCounts = new();
        private readonly List<ActiveGameplayEffect> _activeEffects = new();
        private long _nextEffectUid = 1;
        private int _nextAbilityHandleId = 1;

        /// <summary>
        /// ?ì„± ì»¨í…Œ?´ë„ˆ?…ë‹ˆ?? (?¤ë ˆ???ˆì „?˜ì? ?ŠìŒ - ì§ì ‘ ?˜ì • ê¸ˆì?)
        /// </summary>
        public AttributeSet Attributes => _attributes;

        /// <summary>
        /// ?Œìœ  ?œê·¸ ì»¨í…Œ?´ë„ˆ?…ë‹ˆ?? (?¤ë ˆ???ˆì „?˜ì? ?ŠìŒ - ì§ì ‘ ?˜ì • ê¸ˆì?)
        /// </summary>
        public GameplayTagContainer OwnedTags => _ownedTags;

        /// <summary>
        /// ?Œìœ ???¥ë ¥?¤ì…?ˆë‹¤. (?¤ë ˆ???ˆì „?˜ì? ?ŠìŒ - ì§ì ‘ ?˜ì • ê¸ˆì?)
        /// </summary>
        /// </summary>
        public IReadOnlyList<GameplayAbility> Abilities => _abilities;

        public event Action OnChangedTags;
        public event Action OnAddedAbility;
        public event Action<GameplayAbility, TargetData> OnActivateAbility;

        public IAbilitySystemOwner Owner => _onwer;

        public AbilitySystemComponent()
        {
            _attributes = new AttributeSet();
            _ownedTags = new GameplayTagContainer();
        }

        public AbilitySystemComponent(AttributeSet attributeSet, List<GameplayAbility> abilities, GameplayTagContainer ownedTags) : this()
        {
            _attributes = attributeSet;
            _abilities = abilities;

            ApplyStartupAbilities();
            ApplyStartupTags(ownedTags);
        }

        public void SetOwner(IAbilitySystemOwner owner)
        {
            _onwer = owner;
        }

        /// <summary>
        /// ?œì„± ?¨ê³¼ ê°œìˆ˜?…ë‹ˆ??
        /// </summary>
        public int ActiveEffectCount
        {
            get
            {
                lock (_modelLock)
                {
                    return _activeEffects.Count;
                }
            }
        }

        /// <summary>
        /// ì²˜ìŒë¶€??ë³´ìœ  ???¥ë ¥???ìš©?©ë‹ˆ??
        /// </summary>
        private void ApplyStartupAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (ability == null) continue;
                GiveAbility(ability);
            }
        }

        private void ApplyStartupTags(GameplayTagContainer tagContainer)
        {
            foreach (var tag in tagContainer.Tags)
            {
                AddLooseTag(tag, out _);
            }
        }

        public void Tick(float deltaSeconds)
        {
            TickActiveEffectsInternal(deltaSeconds);
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ì¡°íšŒ?©ë‹ˆ??
        /// </summary>
        public float Get(AttributeId id)
        {
            lock (_modelLock)
            {
                return _attributes.Get(id);
            }
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ?¤ì •?©ë‹ˆ??
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            lock (_modelLock)
            {
                _attributes.Set(id, value);
            }
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ì¦ê°?©ë‹ˆ??
        /// </summary>
        public void Add(AttributeId id, float delta)
        {
            lock (_modelLock)
            {
                var current = _attributes.Get(id);
                _attributes.Set(id, current + delta);
            }
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ?¼ì„¼?¸ë¡œ ì¦ê°?©ë‹ˆ??
        /// ?‘ìˆ˜ ?¼ì„¼?¸ëŠ” ì¦ê?, ?Œìˆ˜ ?¼ì„¼?¸ëŠ” ê°ì†Œë¥??˜ë??©ë‹ˆ??
        /// </summary>
        public void AddPercent(AttributeId id, float percent)
        {
            if (percent == 0f)
            {
                return;
            }

            lock (_modelLock)
            {
                var current = _attributes.Get(id);
                var bonus = (float)System.Math.Round(current * percent);

                // ìµœì†Œ ë³€?”ëŸ‰ ë³´ì¥ (ë²„í”„/?”ë²„?„ê? ?ˆë¬´ ?‘ì? ?Šë„ë¡?
                if (bonus > 0f && bonus < 1f)
                {
                    bonus = 1f;
                }
                else if (bonus < 0f && bonus > -1f)
                {
                    bonus = -1f;
                }

                _attributes.Set(id, current + bonus);
            }
        }

        /// <summary>
        /// ?¥ë ¥??ì¶”ê??©ë‹ˆ??
        /// </summary>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)
        {
            lock (_modelLock)
            {
                var spec = new GameplayAbilitySpec(ability, _nextAbilityHandleId++);
                _abilitySpecs.Add(spec);

                OnChangedTags?.Invoke();

                return spec.Handle;
            }
        }

        /// <summary>
        /// ?¸ë“¤???µí•´ ?¥ë ¥???œê±°?©ë‹ˆ??
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool RemoveAbility(FGameplayAbilitySpecHandle handle)
        {
            lock (_modelLock)
            {
                for (var i = _abilitySpecs.Count - 1; i >= 0; i--)
                {
                    var spec = _abilitySpecs[i];
                    if (spec.Handle != handle) continue;

                    var lastIndex = _abilitySpecs.Count - 1;
                    if (i < lastIndex)
                    {
                        _abilitySpecs[i] = _abilitySpecs[lastIndex];
                    }

                    _abilitySpecs.RemoveAt(lastIndex);

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// ?¥ë ¥???œê±°?©ë‹ˆ??
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool RemoveAbility(GameplayAbility ability)
        {
            lock (_modelLock)
            {
                for (var i = _abilitySpecs.Count - 1; i >= 0; i--)
                {
                    var spec = _abilitySpecs[i];
                    if (spec.AbilityTag.Equals(ability.AbilityTag)) continue;

                    var lastIndex = _abilitySpecs.Count - 1;
                    if (i < lastIndex)
                    {
                        _abilitySpecs[i] = _abilitySpecs[lastIndex];
                    }

                    _abilitySpecs.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// ?¹ì • ?œê·¸ë¥?ê°€ì§??¥ë ¥???œì„±???œë„?©ë‹ˆ?? ?¬ëŸ¬ ê°œì¼ ???ˆìŠµ?ˆë‹¤.
        /// </summary>
        /// <param name="abilityTag">?¥ë ¥ ?œê·¸</param>
        /// <returns>?œì„±???±ê³µ ?¬ë?</returns>
        public bool TryActivateAbilityByTag(FGameplayTag abilityTag)
        {
            // ?œê·¸ê°€ ?¬í•¨???¤í™??ëª¨ë‘ ?œë„?œë‹¤.
            var bSuccess = false;
            foreach (var spec in _abilitySpecs)
            {
                if (spec == null) continue;
                if (!spec.AbilityTag.Equals(abilityTag)) continue;

                if (!spec.AbilityTag.Equals(abilityTag)) continue;

                bSuccess |= TryActivateAbility(spec);
            }

            return bSuccess;
        }

        /// <summary>
        /// ?¥ë ¥ ?¬ì–‘?¼ë¡œ ?¥ë ¥???œì„±???œë„?©ë‹ˆ??
        /// TargetContext ?†ì´ ?¸ì¶œ?˜ë©´ ?€ê²ŸíŒ… ?†ì´ ì§„í–‰?©ë‹ˆ??
        /// </summary>
        /// <param name="spec">?¥ë ¥ ?¬ì–‘</param>
        /// <returns>?œì„±???±ê³µ ?¬ë?</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec)
        {
            return TryActivateAbility(spec, null, out _);
        }

        /// <summary>
        /// ?¥ë ¥ ?¬ì–‘?¼ë¡œ ?¥ë ¥???œì„±???œë„?©ë‹ˆ??
        /// </summary>
        /// <param name="spec">?¥ë ¥ ?¬ì–‘</param>
        /// <param name="targetContext">?€ê²ŸíŒ… ì»¨í…?¤íŠ¸</param>
        /// <returns>?œì„±???±ê³µ ?¬ë?</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec, TargetContext targetContext)
        {
            return TryActivateAbility(spec, targetContext, out _);
        }

        /// <summary>
        /// ?¥ë ¥ ?¬ì–‘?¼ë¡œ ?¥ë ¥???œì„±???œë„?©ë‹ˆ??
        /// </summary>
        /// <param name="spec">?¥ë ¥ ?¬ì–‘</param>
        /// <param name="targetContext">?€ê²ŸíŒ… ì»¨í…?¤íŠ¸</param>
        /// <param name="targetData">?€ê²ŸíŒ… ê²°ê³¼ (out)</param>
        /// <returns>?œì„±???±ê³µ ?¬ë?</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec, TargetContext targetContext, out TargetData targetData)
        {
            targetData = null;

            if (spec == null)
            {
                return false;
            }

            //ASC ?ì„œ ?ë‹¨?˜ëŠ” ?¥ë ¥ ?œì„±??ê°€???¬ë?
            if (!CanActivateAbility(spec))
            {
                return false;
            }

            var ability = spec.Ability;
            if (ability == null)
            {
                return false;
            }

            //?¥ë ¥ ê°œë³„?ìœ¼ë¡œë„ ?¥ë ¥ ?œì„±??ê°€???¬ë?ë¥??ë‹¨?´ì•¼ ?œë‹¤ë©??¬ê¸°??            //ability.CanActivateAbility()

            // ?€ê²ŸíŒ…
            if (ability.TargetingStrategy != null && targetContext != null)
            {
                //?¥ë ¥???€ê²Ÿì „?µì— ?´ë‹¹?˜ëŠ” ?€ê²?ì°¾ê¸°
                //ex) SelfTargetingStrategy : ?ê¸° ?ì‹ ???€ê²Ÿì˜ ??                //ex ) NearestNEnemiesTargetingStrategy : ê°€??ê°€ê¹Œìš´ Nëª…ì˜ ??                targetData = ability.TargetingStrategy.FindTargets(this, targetContext);

                // ?€ê²Ÿì´ ?„ìš”???¥ë ¥?¸ë° ?€ê²Ÿì´ ?†ìœ¼ë©??¤íŒ¨
                if (targetData == null || targetData.Targets.Count == 0)
                {
                    return false;
                }
            }

            // ?¨ê³¼ ?ìš©
            if (ability.AppliedEffects != null && targetData != null)
            {
                foreach (var effect in ability.AppliedEffects)
                {
                    if (effect == null) continue;
                    ApplyEffectToTargets(effect, targetData);
                }
            }

            // ì¿¨ë‹¤???¨ê³¼ ?ìš© (?ì‹ ?ê²Œ)
            if (ability.CooldownEffect != null)
            {
                ApplyEffectToSelf(ability.CooldownEffect);
            }

            spec.IncrementActiveCount();

            OnActivateAbility?.Invoke(ability, targetData);
            return true;
        }

        /// <summary>
        /// ?ì‹ ?ê²Œ ?¨ê³¼ë¥??ìš©?©ë‹ˆ??
        /// </summary>
        private void ApplyEffectToSelf(GameplayEffect effect)
        {
            if (effect == null) return;
            ApplyEffectToTarget(effect, this);
        }

        /// <summary>
        /// ?€ê²Ÿë“¤?ê²Œ ?¨ê³¼ë¥??ìš©?©ë‹ˆ??
        /// </summary>
        private void ApplyEffectToTargets(GameplayEffect effect, TargetData targetData)
        {
            if (effect == null || targetData == null) return;

            foreach (var target in targetData.Targets)
            {
                if (target == null) continue;
                ApplyEffectToTarget(effect, target);
            }
        }

        /// <summary>
        /// ?¨ì¼ ?€ê²Ÿì—ê²??¨ê³¼ë¥??ìš©?©ë‹ˆ??
        /// </summary>
        private void ApplyEffectToTarget(GameplayEffect effect, AbilitySystemComponent target)
        {
            if (effect == null || target == null) return;

            //?¨ê³¼???œê·¸ ì²´í¬ë¥??©ë‹ˆ??

            // ?„ìˆ˜ ?œê·¸ ì²´í¬
            if (effect.RequiredTags != null && !target.OwnedTags.HasAll(effect.RequiredTags))
            {
                return;
            }

            // ì°¨ë‹¨ ?œê·¸ ì²´í¬
            if (effect.BlockedTags != null && target.OwnedTags.HasAny(effect.BlockedTags))
            {
                return;
            }

            // ?ì„± ?˜ì • ?ìš© (this = source, target = ?€??
            foreach (var modifier in effect.Modifiers)
            {
                ApplyModifier(this, target, modifier);
            }

            // ?œê·¸ ë¶€??            if (effect.GrantedTags != null)
            {
                foreach (var tag in effect.GrantedTags.Tags)
                {
                    target.AddEffectTag(tag, out _);
                }
            }

            // Duration ?¨ê³¼??ê²½ìš° ?œì„± ?¨ê³¼ë¡??±ë¡
            // Tick?ì„œ ë§Œë£Œ ì²˜ë¦¬ ???œê·¸ ?œê±°??            if (effect.DurationType == EffectDurationType.HasDuration && effect.Duration > 0)
            {
                // Duration?€ Effect??Duration Polciyê°€ ?ˆëŠ” ê²½ìš°
                //ê³„ì‚°???µí•´ ?˜ì • ?œë‹¤
                target.AddActiveEffect(effect, effect.Duration);
            }
        }

        /// <summary>
        /// ëª¨ë””?Œì´?´ë? ?ìš©?©ë‹ˆ??
        /// </summary>
        /// <param name="source">?œì „??(?¨ê³¼ë¥?ë°œë™??ì£¼ì²´)</param>
        /// <param name="target">?€??(?¨ê³¼ë¥?ë°›ëŠ” ì£¼ì²´)</param>
        /// <param name="modifier">?ìš©???˜ì •??/param>
        private void ApplyModifier(AbilitySystemComponent source, AbilitySystemComponent target, AttributeModifier modifier)
        {
            if (target == null) return;

            switch (modifier.ValueMode)
            {
                case AttributeModifierValueMode.Static:
                    ApplyStaticModifier(target, modifier);
                    break;

                case AttributeModifierValueMode.Calculated:
                    ApplyCalculatedModifier(source, target, modifier);
                    break;
            }
        }

        /// <summary>
        /// Static ëª¨ë“œ ?˜ì •?ë? ?ìš©?©ë‹ˆ??
        /// </summary>
        private void ApplyStaticModifier(AbilitySystemComponent target, AttributeModifier modifier)
        {
            var attrId = modifier.AttributeId;
            var value = modifier.Magnitude;

            switch (modifier.Operation)
            {
                case AttributeModifierOperationType.Add:
                    target.Add(attrId, value);
                    break;
                case AttributeModifierOperationType.Multiply:
                    target.AddPercent(attrId, value);
                    break;
                case AttributeModifierOperationType.Override:
                    target.Set(attrId, value);
                    break;
            }
        }

        /// <summary>
        /// Calculated ëª¨ë“œ ?˜ì •?ë? ?ìš©?©ë‹ˆ??
        /// ê³„ì‚°ê¸°ê? source/target ASC ?•ë³´ë¥?ê¸°ë°˜?¼ë¡œ ?ì„±???˜ì •?©ë‹ˆ??
        /// </summary>
        private void ApplyCalculatedModifier(AbilitySystemComponent source, AbilitySystemComponent target, AttributeModifier modifier)
        {
            var calculator = AttributeCalculatorFactory.Create(modifier.CalculatorType);
            if (calculator == null) return;

            calculator.Apply(source, target);
        }

        /// <summary>
        /// ?¥ë ¥ ?œì„±??ì¡°ê±´??ê²€?¬í•©?ˆë‹¤. (?„ìˆ˜ ?œê·¸, ì°¨ë‹¨ ?œê·¸)
        /// </summary>
        /// <param name="spec">ê²€?¬í•  ?¥ë ¥ ?¬ì–‘</param>
        /// <returns>?œì„±??ê°€?¥í•˜ë©?true</returns>
        public bool CanActivateAbility(GameplayAbilitySpec spec)
        {
            if (spec == null) return false;

            // ?„ìˆ˜ ?œê·¸ ì²´í¬
            if (spec.ActivationRequiredTags != null && !OwnedTags.HasAll(spec.ActivationRequiredTags))
            {
                return false;
            }

            // ì°¨ë‹¨ ?œê·¸ ì²´í¬ (ì¿¨ë‹¤???œê·¸???¬ê¸°??ì²´í¬??
            if (spec.ActivationBlockedTags != null && OwnedTags.HasAny(spec.ActivationBlockedTags))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ?œì„±??ê°€?¥í•œ ?¥ë ¥ ?¬ì–‘ ëª©ë¡??ë°˜í™˜?©ë‹ˆ??
        /// ì¿¨ë‹¤?´ì? ActivationBlockedTags???œê·¸ ì²´í¬ë¡?ì²˜ë¦¬?©ë‹ˆ??
        /// </summary>
        public void GetActivatableAbilities(List<GameplayAbilitySpec> results)
        {
            if (results == null) return;

            lock (_modelLock)
            {
                for (var i = 0; i < _abilitySpecs.Count; i++)
                {
                    var spec = _abilitySpecs[i];
                    if (spec == null) continue;

                    if (!CanActivateAbility(spec))
                    {
                        continue;
                    }

                    results.Add(spec);
                }
            }
        }

        /// <summary>
        /// ?¥ë ¥ ?¬ì–‘ ëª©ë¡??ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public IReadOnlyList<GameplayAbilitySpec> GetAbilitySpecs()
        {
            lock (_modelLock)
            {
                return new List<GameplayAbilitySpec>(_abilitySpecs);
            }
        }

        /// <summary>
        /// ë£¨ì¦ˆ ?œê·¸ë¥?ì¶”ê??©ë‹ˆ??
        /// </summary>
        public bool AddLooseTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return AddTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// ë£¨ì¦ˆ ?œê·¸ë¥??œê±°?©ë‹ˆ??
        /// </summary>
        public bool RemoveLooseTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return RemoveTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// ?¨ê³¼ ?œê·¸ë¥?ì¶”ê??©ë‹ˆ??
        /// </summary>
        public bool AddEffectTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return AddTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// ?¨ê³¼ ?œê·¸ë¥??œê±°?©ë‹ˆ??
        /// </summary>
        public bool RemoveEffectTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return RemoveTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// ?œê·¸??ì´?ê°œìˆ˜ë¥?ì¡°íšŒ?©ë‹ˆ??
        /// </summary>
        public int GetTotalTagCount(FGameplayTag tag)
        {
            if (!tag.IsValid)
            {
                return 0;
            }

            lock (_modelLock)
            {
                return GetTotalTagCount(tag.Hash);
            }
        }

        /// <summary>
        /// ?œì„± ?¨ê³¼ë¥?ì¶”ê??˜ê³  ?ì„±??UIDë¥?ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        /// <param name="effect">?¨ê³¼</param>
        /// <param name="remainingDuration">?¨ì? ì§€???œê°„ (ì´?</param>
        public long AddActiveEffect(GameplayEffect effect, float remainingDuration)
        {
            lock (_modelLock)
            {
                var uid = _nextEffectUid++;

                var CalculateDuration = remainingDuration;

                //ì§€???œê°„ ê³„ì‚° ?•ì±…???ˆë‹¤ë©?ê³„ì‚°?œë‹¤.
                if (effect.DurationPolicy != null)
                {
                    effect.DurationPolicy.CalculateDuration(this, ref CalculateDuration);
                }

                _activeEffects.Add(new ActiveGameplayEffect
                {
                    EffectUid = uid,
                    Effect = effect,
                    EndTime = CalculateDuration  // ì¹´ìš´?¸ë‹¤??ë°©ì‹?¼ë¡œ ?¬ìš©
                });
                return uid;
            }
        }

        /// <summary>
        /// ?œì„± ?¨ê³¼???¨ì? ?œê°„??ê°±ì‹ ?˜ê³  ë§Œë£Œ???¨ê³¼ë¥?ì²˜ë¦¬?©ë‹ˆ??
        /// ë§??„ë ˆ???¸ì¶œ?˜ì–´???©ë‹ˆ??
        /// </summary>
        private void TickActiveEffectsInternal(float deltaTime, List<GameplayEffect> expired = null)
        {
            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var active = _activeEffects[i];
                    if (active.Effect == null)
                    {
                        RemoveActiveEffectAt(i);
                        continue;
                    }

                    // ?¨ì? ?œê°„ ê°ì†Œ
                    active.EndTime -= deltaTime;
                    _activeEffects[i] = active;

                    // ë§Œë£Œ ì²´í¬
                    if (active.EndTime <= 0)
                    {
                        // ?¨ê³¼ê°€ ë¶€?¬í•œ ?œê·¸ ?œê±°
                        if (active.Effect.GrantedTags != null)
                        {
                            foreach (var tag in active.Effect.GrantedTags.Tags)
                            {
                                RemoveTagInternal(_effectTagCounts, tag, out _);
                            }
                        }

                        expired?.Add(active.Effect);
                        RemoveActiveEffectAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// ?¸ë±?¤ë¡œ ?œì„± ?¨ê³¼ë¥??œê±°?©ë‹ˆ?? (lock ?´ë??ì„œë§??¸ì¶œ)
        /// </summary>
        private void RemoveActiveEffectAt(int index)
        {
            var lastIndex = _activeEffects.Count - 1;
            if (index < lastIndex)
            {
                _activeEffects[index] = _activeEffects[lastIndex];
            }
            _activeEffects.RemoveAt(lastIndex);
        }

        /// <summary>
        /// UIDë¡??œì„± ?¨ê³¼ë¥??œê±°?©ë‹ˆ??
        /// </summary>
        public bool RemoveActiveEffectByUid(long effectUid)
        {
            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    if (_activeEffects[i].EffectUid != effectUid)
                    {
                        continue;
                    }

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// EffectIdë¡??œì„± ?¨ê³¼ë¥??œê±°?©ë‹ˆ?? (ê°™ì? EffectId??ì²?ë²ˆì§¸ ?¨ê³¼ë§??œê±°)
        /// </summary>
        public bool RemoveActiveEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
            {
                return false;
            }

            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    if (_activeEffects[i].Effect?.EffectId != effectId)
                    {
                        continue;
                    }

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// ë§Œë£Œ???¨ê³¼ë¥??˜ì§‘?˜ê³  ?œê±°?©ë‹ˆ??
        /// ?¨ê³¼ê°€ ë¶€?¬í•œ ?œê·¸???¨ê»˜ ?œê±°?©ë‹ˆ??
        /// </summary>
        public void CollectExpiredEffects(float now, List<GameplayEffect> expired)
        {
            if (expired == null)
            {
                return;
            }

            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var active = _activeEffects[i];
                    if (active.Effect == null || now < active.EndTime)
                    {
                        continue;
                    }

                    // ?¨ê³¼ê°€ ë¶€?¬í•œ ?œê·¸ ?œê±°
                    if (active.Effect.GrantedTags != null)
                    {
                        foreach (var tag in active.Effect.GrantedTags.Tags)
                        {
                            RemoveEffectTag(tag, out _);
                        }
                    }

                    expired.Add(active.Effect);

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                }
            }
        }

        /// <summary>
        /// ?œì„± ?¨ê³¼ ëª©ë¡??ë³µì‚¬?˜ì—¬ ë°˜í™˜?©ë‹ˆ?? (?¤ë ˆ???ˆì „)
        /// </summary>
        public List<GameplayEffect> GetActiveEffects()
        {
            lock (_modelLock)
            {
                var results = new List<GameplayEffect>(_activeEffects.Count);
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var effect = _activeEffects[i].Effect;
                    if (effect != null)
                    {
                        results.Add(effect);
                    }
                }
                return results;
            }
        }

        /// <summary>
        /// ?œì„± ?¨ê³¼ ëª©ë¡???œê³µ??ë¦¬ìŠ¤?¸ì— ì¶”ê??©ë‹ˆ?? (?¤ë ˆ???ˆì „)
        /// </summary>
        public void GetActiveEffects(List<GameplayEffect> results)
        {
            if (results == null)
            {
                return;
            }

            lock (_modelLock)
            {
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var effect = _activeEffects[i].Effect;
                    if (effect != null)
                    {
                        results.Add(effect);
                    }
                }
            }
        }

        /// <summary>
        /// ?´ë? ?œì„± ?¨ê³¼ ëª©ë¡???ˆì „?˜ì? ?Šì? ?‘ê·¼???œê³µ?©ë‹ˆ?? (lock ?´ë??ì„œë§??¸ì¶œ)
        /// </summary>
        internal IReadOnlyList<ActiveGameplayEffect> GetActiveEffectsUnsafe()
        {
            return _activeEffects;
        }

        /// <summary>
        /// ?œê·¸ë¥?ì¶”ê??˜ëŠ” ?´ë? ë©”ì„œ?œì…?ˆë‹¤. (lock ?´ë??ì„œë§??¸ì¶œ)
        /// </summary>
        private bool AddTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            totalCount = 0;
            if (!tag.IsValid)
            {
                return false;
            }

            var hash = tag.Hash;
            var beforeTotal = GetTotalTagCount(hash);
            counts.TryGetValue(hash, out var count);
            count++;
            counts[hash] = count;

            totalCount = GetTotalTagCount(hash);
            if (beforeTotal == 0)
            {
                if (_ownedTags.AddTag(tag))
                {
                    OnChangedTags?.Invoke();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ?œê·¸ë¥??œê±°?˜ëŠ” ?´ë? ë©”ì„œ?œì…?ˆë‹¤. (lock ?´ë??ì„œë§??¸ì¶œ)
        /// </summary>
        private bool RemoveTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            totalCount = 0;
            if (!tag.IsValid)
            {
                return false;
            }

            var hash = tag.Hash;
            if (!counts.TryGetValue(hash, out var count))
            {
                return false;
            }

            count--;
            if (count <= 0)
            {
                counts.Remove(hash);
            }
            else
            {
                counts[hash] = count;
            }

            totalCount = GetTotalTagCount(hash);
            if (totalCount == 0)
            {
                _ownedTags.RemoveTag(tag);
                OnChangedTags?.Invoke();

                return true;
            }

            return false;
        }

        /// <summary>
        /// ?œê·¸??ì´?ê°œìˆ˜ë¥?ì¡°íšŒ?˜ëŠ” ?´ë? ë©”ì„œ?œì…?ˆë‹¤. (lock ?´ë??ì„œë§??¸ì¶œ)
        /// </summary>
        private int GetTotalTagCount(int hash)
        {
            _effectTagCounts.TryGetValue(hash, out var effectCount);
            _looseTagCounts.TryGetValue(hash, out var looseCount);
            return effectCount + looseCount;
        }

        /// <summary>
        /// ê²Œì„?Œë ˆ???´ë²¤?¸ë? ì²˜ë¦¬?©ë‹ˆ?? ?´ë²¤???œê·¸ë¡??¸ë¦¬ê±°ë˜???¥ë ¥???œì„±?”í•©?ˆë‹¤.
        /// </summary>
        /// <param name="eventData">?´ë²¤???°ì´??/param>
        /// <returns>?œì„±???±ê³µ ?¬ë?</returns>
        public bool HandleGameplayTagEvent(GameplayTagEventData eventData)
        {
            return false;
            // if (!eventData.EventTag.IsValid)
            // {
            //     return false;
            // }

            // // ?´ë²¤?¸ë? ë¸Œë¡œ?œìº?¤íŠ¸?˜ê³  ì¡°ê±´??ë§ëŠ” ?¥ë ¥??ì°¾ëŠ”??
            // var eventTag = eventData.EventTag;
            // _onGameplayEvent?.Invoke(this, eventData);
            // var bSuccess = false;
            // foreach (var spec in _abilities)
            // {
            //     if (spec == null) continue;
            //     if (spec.AbilityType == null) continue;

            //     if (!spec.TryGetConfigs<GameplayEventTriggerConfig>(out var triggerConfigs))
            //     {
            //         continue;
            //     }

            //     var matched = false;
            //     for (var i = 0; i < triggerConfigs.Count; i++)
            //     {
            //         var triggerConfig = triggerConfigs[i];
            //         if (triggerConfig == null || !triggerConfig.ActivateOnEvent)
            //         {
            //             continue;
            //         }

            //         if (!IsEventTagMatch(eventTag, triggerConfig.TriggerTag))
            //         {
            //             continue;
            //         }

            //         matched = true;
            //         break;
            //     }

            //     if (matched)
            //     {
            //         bSuccess |= TryActivateAbility(spec, eventData);
            //     }
            // }

            // return bSuccess;
        }

        /// <summary>
        /// ?„ì¬ ?íƒœ??ë¶ˆë? ?¤ëƒ…?·ì„ ?ì„±?©ë‹ˆ?? (?¤ë ˆ???ˆì „)
        /// Host ?˜ê²½?ì„œ ?´ë¼?´ì–¸?¸ë¡œ ?íƒœë¥??„ì†¡?????¬ìš©?©ë‹ˆ??
        /// </summary>
        public AbilitySystemSnapshot BuildSnapshot()
        {
            lock (_modelLock)
            {
                // ?ì„± ê°?ë³µì‚¬
                var attributeDict = new Dictionary<AttributeId, float>();
                foreach (var attr in _attributes.Values)
                {
                    if (attr != null)
                    {
                        attributeDict[attr.AttributeId] = attr.CurrentValue;
                    }
                }

                // ?œê·¸ ë³µì‚¬
                var tagList = new List<FGameplayTag>(_ownedTags.Tags);

                // ?¥ë ¥ ë³µì‚¬
                var abilities = new List<GameplayAbility>(_abilities);

                // ?œì„± ?¨ê³¼ ë³µì‚¬
                var effectList = new List<ActiveGameplayEffectSnapshot>(_activeEffects.Count);
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var active = _activeEffects[i];
                    effectList.Add(new ActiveGameplayEffectSnapshot(
                        active.EffectUid,
                        active.Effect,
                        active.EndTime
                    ));
                }

                return new AbilitySystemSnapshot(attributeDict, tagList, abilities, effectList);
            }
        }

        public void Dispose()
        {
            OnAddedAbility = null;
            OnChangedTags = null;
            OnActivateAbility = null;
            _onwer = null;
        }

    }
}

