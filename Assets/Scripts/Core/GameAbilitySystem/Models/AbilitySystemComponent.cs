using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// AbilitySystemComponent 상태 모델입니다 (순수 C# 모델).
    /// Unity에 의존하지 않으며 Host 환경에서 스레드 안전하게 동작합니다.
    /// </summary>
    public sealed class AbilitySystemComponent : IDisposable
    {
        public struct ActiveGameplayEffect
        {
            public long EffectUid;
            public GameplayEffect Effect;
            public float EndTime;
        }

        private IAbilitySystemOwner _owner;

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
        private bool _abilityDirty = true;

        /// <summary>
        /// 속성 컨테이너입니다. (스레드 안전하지 않음 - 직접 수정 금지)
        /// </summary>
        public AttributeSet Attributes => _attributes;

        /// <summary>
        /// 소유 태그 컨테이너입니다. (스레드 안전하지 않음 - 직접 수정 금지)
        /// </summary>
        public GameplayTagContainer OwnedTags => _ownedTags;

        /// <summary>
        /// 소유한 능력입니다. (스레드 안전하지 않음 - 직접 수정 금지)
        /// </summary>
        public IReadOnlyList<GameplayAbility> Abilities => _abilities;

        public event Action OnChangedTags;
        public event Action OnAddedAbility;
        public event Action<GameplayAbility, TargetData> OnActivateAbility;

        /// <summary>
        /// 능력 활성화 점검이 필요한지 여부를 소비합니다.
        /// true면 플래그를 false로 되돌립니다.
        /// </summary>
        public bool ConsumeAbilityDirty()
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                if (!_abilityDirty)
                {
                    return false;
                }

                _abilityDirty = false;
                return true;
            }
        }
        /// <summary>
        /// MarkAbilityDirty 함수를 처리합니다.
        /// </summary>

        private void MarkAbilityDirty()
        {
            // 핵심 로직을 처리합니다.
            _abilityDirty = true;
        }

        public IAbilitySystemOwner Owner => _owner;
        /// <summary>
        /// AbilitySystemComponent 함수를 처리합니다.
        /// </summary>

        public AbilitySystemComponent()
        {
            // 핵심 로직을 처리합니다.
            _attributes = new AttributeSet();
            _ownedTags = new GameplayTagContainer();
        }
        /// <summary>
        /// this 함수를 처리합니다.
        /// </summary>

        public AbilitySystemComponent(AttributeSet attributeSet, List<GameplayAbility> abilities, GameplayTagContainer ownedTags) : this()
        {
            // 핵심 로직을 처리합니다.
            _attributes = attributeSet;
            _abilities = abilities;

            ApplyStartupAbilities();
            ApplyStartupTags(ownedTags);
        }
        /// <summary>
        /// SetOwner 함수를 처리합니다.
        /// </summary>

        public void SetOwner(IAbilitySystemOwner owner)
        {
            // 핵심 로직을 처리합니다.
            _owner = owner;
        }

        /// <summary>
        /// 활성 효과 개수입니다.
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
        /// 처음부터 보유한 능력을 적용합니다.
        /// </summary>
        private void ApplyStartupAbilities()
        {
            // 핵심 로직을 처리합니다.
            foreach (var ability in _abilities)
            {
                if (ability == null) continue;
                GiveAbility(ability);
            }
        }
        /// <summary>
        /// ApplyStartupTags 함수를 처리합니다.
        /// </summary>

        private void ApplyStartupTags(GameplayTagContainer tagContainer)
        {
            // 핵심 로직을 처리합니다.
            foreach (var tag in tagContainer.Tags)
            {
                AddLooseTag(tag, out _);
            }
        }
        /// <summary>
        /// Tick 함수를 처리합니다.
        /// </summary>

        public void Tick(float deltaSeconds)
        {
            // 핵심 로직을 처리합니다.
            TickActiveEffectsInternal(deltaSeconds);
        }

        /// <summary>
        /// 속성 값을 조회합니다.
        /// </summary>
        public float Get(AttributeId id)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return _attributes.Get(id);
            }
        }

        /// <summary>
        /// 속성 값을 설정합니다.
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                _attributes.Set(id, value);
            }
        }

        /// <summary>
        /// 속성 값을 증감합니다.
        /// </summary>
        public void Add(AttributeId id, float delta)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                var current = _attributes.Get(id);
                _attributes.Set(id, current + delta);
            }
        }

        /// <summary>
        /// 속성 값을 퍼센트로 증감합니다.
        /// 양수 퍼센트는 증가, 음수 퍼센트는 감소를 의미합니다.
        /// </summary>
        public void AddPercent(AttributeId id, float percent)
        {
            // 핵심 로직을 처리합니다.
            if (percent == 0f)
            {
                return;
            }

            lock (_modelLock)
            {
                var current = _attributes.Get(id);
                var bonus = (float)System.Math.Round(current * percent);

                // 최소 변화량 보장 (버프/디버프가 너무 작지 않도록)
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
        /// 능력을 추가합니다.
        /// </summary>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                var spec = new GameplayAbilitySpec(ability, _nextAbilityHandleId++);
                _abilitySpecs.Add(spec);

                MarkAbilityDirty();
                OnAddedAbility?.Invoke();
                OnChangedTags?.Invoke();

                return spec.Handle;
            }
        }

        /// <summary>
        /// 핸들을 통해 능력을 제거합니다.
        /// </summary>
        /// <param name="handle">제거할 능력의 핸들</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveAbility(FGameplayAbilitySpecHandle handle)
        {
            // 핵심 로직을 처리합니다.
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

                    MarkAbilityDirty();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 능력을 제거합니다.
        /// </summary>
        /// <param name="ability">제거할 능력</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveAbility(GameplayAbility ability)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                for (var i = _abilitySpecs.Count - 1; i >= 0; i--)
                {
                    var spec = _abilitySpecs[i];
                    if (!spec.AbilityTag.Equals(ability.AbilityTag)) continue;

                    var lastIndex = _abilitySpecs.Count - 1;
                    if (i < lastIndex)
                    {
                        _abilitySpecs[i] = _abilitySpecs[lastIndex];
                    }

                    _abilitySpecs.RemoveAt(lastIndex);
                    MarkAbilityDirty();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 특정 태그를 가진 능력의 활성화를 시도합니다. 여러 개일 수 있습니다.
        /// </summary>
        /// <param name="abilityTag">능력 태그</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbilityByTag(FGameplayTag abilityTag)
        {
            // 태그가 일치하는 스펙을 모두 시도한다.
            var bSuccess = false;
            foreach (var spec in _abilitySpecs)
            {
                if (spec == null) continue;
                if (!spec.AbilityTag.Equals(abilityTag)) continue;

                bSuccess |= TryActivateAbility(spec);
            }

            return bSuccess;
        }

        /// <summary>
        /// 능력 스펙으로 능력 활성화를 시도합니다.
        /// TargetContext 없이 호출하면 타겟팅 없이 진행합니다.
        /// </summary>
        /// <param name="spec">능력 스펙</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec)
        {
            // 핵심 로직을 처리합니다.
            return TryActivateAbility(spec, null, out _);
        }

        /// <summary>
        /// 능력 스펙으로 능력 활성화를 시도합니다.
        /// </summary>
        /// <param name="spec">능력 스펙</param>
        /// <param name="targetContext">타겟팅 컨텍스트</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec, TargetContext targetContext)
        {
            // 핵심 로직을 처리합니다.
            return TryActivateAbility(spec, targetContext, out _);
        }

        /// <summary>
        /// 능력 스펙으로 능력 활성화를 시도합니다.
        /// </summary>
        /// <param name="spec">능력 스펙</param>
        /// <param name="targetContext">타겟팅 컨텍스트</param>
        /// <param name="targetData">타겟팅 결과 (out)</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec, TargetContext targetContext, out TargetData targetData)
        {
            // 핵심 로직을 처리합니다.
            return TryActivateAbility(spec, targetContext, out targetData, applyAppliedEffectsOnActivate: true);
        }

        /// <summary>
        /// 능력 스펙으로 능력 활성화를 시도합니다.
        /// </summary>
        /// <param name="spec">능력 스펙</param>
        /// <param name="targetContext">타겟팅 컨텍스트</param>
        /// <param name="targetData">타겟팅 결과 (out)</param>
        /// <param name="applyAppliedEffectsOnActivate">
        /// true면 발동 시점에 AppliedEffects를 즉시 적용합니다.
        /// false면 AppliedEffects 적용을 호출 측(예: 투사체 히트 시점)으로 위임합니다.
        /// </param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(
            GameplayAbilitySpec spec,
            TargetContext targetContext,
            out TargetData targetData,
            bool applyAppliedEffectsOnActivate)
        {
            targetData = null;

            if (spec == null)
            {
                return false;
            }

            // ASC에서 판단하는 능력 활성화 가능 여부
            if (!CanActivateAbility(spec))
            {
                return false;
            }

            var ability = spec.Ability;
            if (ability == null)
            {
                return false;
            }

            // 능력 개별적으로도 능력 활성화 가능 여부를 판단해야 한다면 여기서
            // ability.CanActivateAbility()

            // 타겟팅
            if (ability.TargetingStrategy != null && targetContext != null)
            {
                // 능력의 타겟전략에 해당하는 대상 찾기
                // ex) SelfTargetingStrategy : 자기 자신을 타겟으로
                // ex) NearestNEnemiesTargetingStrategy : 가장 가까운 N명의 적
                targetData = ability.TargetingStrategy.FindTargets(this, targetContext);

                // 타겟이 필요한 능력인데 타겟이 없으면 실패
                if (targetData == null || targetData.Targets.Count == 0)
                {
                    return false;
                }
            }

            // 효과 적용
            if (applyAppliedEffectsOnActivate && ability.AppliedEffects != null && targetData != null)
            {
                foreach (var effect in ability.AppliedEffects)
                {
                    if (effect == null) continue;
                    ApplyEffectToTargets(effect, targetData);
                }
            }

            // 쿨다운 효과 적용 (자신에게)
            if (ability.CooldownEffect != null)
            {
                ApplyEffectToSelf(ability.CooldownEffect);
            }

            spec.IncrementActiveCount();

            OnActivateAbility?.Invoke(ability, targetData);
            return true;
        }

        /// <summary>
        /// 자신에게 효과를 적용합니다.
        /// </summary>
        private void ApplyEffectToSelf(GameplayEffect effect)
        {
            // 핵심 로직을 처리합니다.
            if (effect == null) return;
            ApplyEffectToTarget(effect, this);
        }

        /// <summary>
        /// 타겟들에게 효과를 적용합니다.
        /// </summary>
        private void ApplyEffectToTargets(GameplayEffect effect, TargetData targetData)
        {
            // 핵심 로직을 처리합니다.
            if (effect == null || targetData == null) return;

            foreach (var target in targetData.Targets)
            {
                if (target == null) continue;
                ApplyEffectToTarget(effect, target);
            }
        }

        /// <summary>
        /// 여러 이펙트를 타겟들에게 적용합니다.
        /// (투사체 히트 시점 적용 등 외부 호출용)
        /// </summary>
        public void ApplyEffectsToTargets(IReadOnlyList<GameplayEffect> effects, TargetData targetData)
        {
            // 핵심 로직을 처리합니다.
            if (effects == null || targetData == null)
            {
                return;
            }

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                ApplyEffectToTargets(effect, targetData);
            }
        }

        /// <summary>
        /// 단일 타겟에게 효과를 적용합니다.
        /// </summary>
        private void ApplyEffectToTarget(GameplayEffect effect, AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (effect == null || target == null) return;

            // 효과의 태그 체크를 합니다.

            // 필수 태그 체크
            if (effect.RequiredTags != null && !target.OwnedTags.HasAll(effect.RequiredTags))
            {
                return;
            }

            // 차단 태그 체크
            if (effect.BlockedTags != null && target.OwnedTags.HasAny(effect.BlockedTags))
            {
                return;
            }

            // 속성 수정 적용 (this = source, target = 대상)
            foreach (var modifier in effect.Modifiers)
            {
                ApplyModifier(this, target, modifier);
            }

            // 태그 부여
            if (effect.GrantedTags != null)
            {
                foreach (var tag in effect.GrantedTags.Tags)
                {
                    target.AddEffectTag(tag, out _);
                }
            }

            // Duration 효과인 경우 활성 효과로 등록
            // Tick에서 만료 처리 및 태그 제거
            if (effect.DurationType == EffectDurationType.HasDuration && effect.Duration > 0)
            {
                // Duration의 Effect에 Duration Policy가 있는 경우
                // 계산을 통해 수정한다
                target.AddActiveEffect(effect, effect.Duration);
            }

            target.MarkAbilityDirty();
        }

        /// <summary>
        /// 모디파이어를 적용합니다.
        /// </summary>
        /// <param name="source">소스 (효과를 발동한 주체)</param>
        /// <param name="target">타겟 (효과를 받는 주체)</param>
        /// <param name="modifier">적용할 수정자</param>
        private void ApplyModifier(AbilitySystemComponent source, AbilitySystemComponent target, AttributeModifier modifier)
        {
            // 핵심 로직을 처리합니다.
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
        /// Static 모드 수정자를 적용합니다.
        /// </summary>
        private void ApplyStaticModifier(AbilitySystemComponent target, AttributeModifier modifier)
        {
            // 핵심 로직을 처리합니다.
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
        /// Calculated 모드 수정자를 적용합니다.
        /// 계산기가 source/target ASC 정보를 기반으로 속성을 수정합니다.
        /// </summary>
        private void ApplyCalculatedModifier(AbilitySystemComponent source, AbilitySystemComponent target, AttributeModifier modifier)
        {
            // 핵심 로직을 처리합니다.
            var calculator = AttributeCalculatorFactory.Create(modifier.CalculatorType);
            if (calculator == null) return;

            calculator.Apply(source, target);
        }

        /// <summary>
        /// 능력 활성화 조건을 검사합니다. (필수 태그, 차단 태그)
        /// </summary>
        /// <param name="spec">검사할 능력 스펙</param>
        /// <returns>활성화 가능하면 true</returns>
        public bool CanActivateAbility(GameplayAbilitySpec spec)
        {
            // 핵심 로직을 처리합니다.
            if (spec == null) return false;

            // 필수 태그 체크
            if (spec.ActivationRequiredTags != null && !OwnedTags.HasAll(spec.ActivationRequiredTags))
            {
                return false;
            }

            // 차단 태그 체크 (쿨다운 태그도 여기서 체크됨)
            if (spec.ActivationBlockedTags != null && OwnedTags.HasAny(spec.ActivationBlockedTags))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 활성화 가능한 능력 스펙 목록을 반환합니다.
        /// 쿨다운은 ActivationBlockedTags의 태그 체크로 처리합니다.
        /// </summary>
        public void GetActivatableAbilities(List<GameplayAbilitySpec> results)
        {
            // 핵심 로직을 처리합니다.
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
        /// 능력 스펙 목록을 반환합니다.
        /// </summary>
        public IReadOnlyList<GameplayAbilitySpec> GetAbilitySpecs()
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return new List<GameplayAbilitySpec>(_abilitySpecs);
            }
        }

        /// <summary>
        /// 루즈 태그를 추가합니다.
        /// </summary>
        public bool AddLooseTag(FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return AddTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 루즈 태그를 제거합니다.
        /// </summary>
        public bool RemoveLooseTag(FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return RemoveTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 효과 태그를 추가합니다.
        /// </summary>
        public bool AddEffectTag(FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return AddTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 효과 태그를 제거합니다.
        /// </summary>
        public bool RemoveEffectTag(FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                return RemoveTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 태그의 총 개수를 조회합니다.
        /// </summary>
        public int GetTotalTagCount(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
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
        /// 활성 효과를 추가하고 생성된 UID를 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <param name="remainingDuration">남은 지속 시간 (초)</param>
        public long AddActiveEffect(GameplayEffect effect, float remainingDuration)
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                var uid = _nextEffectUid++;

                var CalculateDuration = remainingDuration;

                // 지속 시간 계산 정책이 있다면 계산한다.
                if (effect.DurationPolicy != null)
                {
                    effect.DurationPolicy.CalculateDuration(this, ref CalculateDuration);
                }

                _activeEffects.Add(new ActiveGameplayEffect
                {
                    EffectUid = uid,
                    Effect = effect,
                    EndTime = CalculateDuration  // 카운트다운 방식으로 사용
                });
                MarkAbilityDirty();
                return uid;
            }
        }

        /// <summary>
        /// 활성 효과의 남은 시간을 갱신하고 만료된 효과를 처리합니다.
        /// 매 프레임 호출되어야 합니다.
        /// </summary>
        private void TickActiveEffectsInternal(float deltaTime, List<GameplayEffect> expired = null)
        {
            // 핵심 로직을 처리합니다.
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

                    // 남은 시간 감소
                    active.EndTime -= deltaTime;
                    _activeEffects[i] = active;

                    // 만료 체크
                    if (active.EndTime <= 0)
                    {
                        // 효과가 부여한 태그 제거
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
        /// 인덱스로 활성 효과를 제거합니다. (lock 내부에서만 호출)
        /// </summary>
        private void RemoveActiveEffectAt(int index)
        {
            // 핵심 로직을 처리합니다.
            var lastIndex = _activeEffects.Count - 1;
            if (index < lastIndex)
            {
                _activeEffects[index] = _activeEffects[lastIndex];
            }
            _activeEffects.RemoveAt(lastIndex);
            MarkAbilityDirty();
        }

        /// <summary>
        /// UID로 활성 효과를 제거합니다.
        /// </summary>
        public bool RemoveActiveEffectByUid(long effectUid)
        {
            // 핵심 로직을 처리합니다.
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
                    MarkAbilityDirty();
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// EffectTag로 활성 효과를 제거합니다. (같은 EffectTag의 첫 번째 효과를 제거)
        /// </summary>
        public bool RemoveActiveEffect(FGameplayTag effectTag)
        {
            // 핵심 로직을 처리합니다.
            if (!effectTag.IsValid)
            {
                return false;
            }

            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var activeTag = _activeEffects[i].Effect?.EffectTag;
                    if (!activeTag.HasValue || !activeTag.Value.Equals(effectTag))
                    {
                        continue;
                    }

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                    MarkAbilityDirty();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 문자열 태그로 활성 효과 제거를 시도합니다.
        /// </summary>
        public bool RemoveActiveEffect(string effectTag)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(effectTag))
            {
                return false;
            }

            return RemoveActiveEffect(new FGameplayTag(effectTag));
        }

        /// <summary>
        /// 만료된 효과를 수집하고 제거합니다.
        /// 효과가 부여한 태그도 함께 제거합니다.
        /// </summary>
        public void CollectExpiredEffects(float now, List<GameplayEffect> expired)
        {
            // 핵심 로직을 처리합니다.
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

                    // 효과가 부여한 태그 제거
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
                    MarkAbilityDirty();
                }
            }
        }

        /// <summary>
        /// 활성 효과 목록을 복사하여 반환합니다. (스레드 안전)
        /// </summary>
        public List<GameplayEffect> GetActiveEffects()
        {
            // 핵심 로직을 처리합니다.
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
        /// 활성 효과 목록을 제공된 리스트에 추가합니다. (스레드 안전)
        /// </summary>
        public void GetActiveEffects(List<GameplayEffect> results)
        {
            // 핵심 로직을 처리합니다.
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
        /// 내부 활성 효과 목록에 안전하지 않은 직접 접근을 제공합니다. (lock 내부에서만 호출)
        /// </summary>
        internal IReadOnlyList<ActiveGameplayEffect> GetActiveEffectsUnsafe()
        {
            // 핵심 로직을 처리합니다.
            return _activeEffects;
        }

        /// <summary>
        /// 태그를 추가하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private bool AddTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
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
                    MarkAbilityDirty();
                    OnChangedTags?.Invoke();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 태그를 제거하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private bool RemoveTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            // 핵심 로직을 처리합니다.
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
                MarkAbilityDirty();
                OnChangedTags?.Invoke();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 태그의 총 개수를 조회하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private int GetTotalTagCount(int hash)
        {
            // 핵심 로직을 처리합니다.
            _effectTagCounts.TryGetValue(hash, out var effectCount);
            _looseTagCounts.TryGetValue(hash, out var looseCount);
            return effectCount + looseCount;
        }

        /// <summary>
        /// 게임플레이 이벤트를 처리합니다. 이벤트 태그로 트리거되는 능력을 활성화합니다.
        /// </summary>
        /// <param name="eventData">이벤트 데이터</param>
        /// <returns>활성화 성공 여부</returns>
        public bool HandleGameplayTagEvent(GameplayTagEventData eventData)
        {
            // TODO: 이벤트 기반 능력 활성화 구현 필요
            return false;
        }

        /// <summary>
        /// 현재 상태의 불변 스냅샷을 생성합니다. (스레드 안전)
        /// Host 환경에서 클라이언트로 상태를 전송할 때 사용합니다.
        /// </summary>
        public AbilitySystemSnapshot BuildSnapshot()
        {
            // 핵심 로직을 처리합니다.
            lock (_modelLock)
            {
                // 속성 값 복사
                var attributeDict = new Dictionary<AttributeId, float>();
                foreach (var attr in _attributes.Values)
                {
                    if (attr != null)
                    {
                        attributeDict[attr.AttributeId] = attr.CurrentValue;
                    }
                }

                // 태그 복사
                var tagList = new List<FGameplayTag>(_ownedTags.Tags);

                // 능력 복사
                var abilities = new List<GameplayAbility>(_abilities);

                // 활성 효과 복사
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
        /// <summary>
        /// Dispose 함수를 처리합니다.
        /// </summary>

        public void Dispose()
        {
            // 핵심 로직을 처리합니다.
            OnAddedAbility = null;
            OnChangedTags = null;
            OnActivateAbility = null;
            _owner = null;
        }
    }
}








