using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using MyProject.MergeGame.Models;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 머지 이펙트 결과입니다.
    /// </summary>
    public sealed class MergeEffectResult
    {
        private readonly List<MergeHostEvent> _events = new();

        /// <summary>
        /// 발생한 이벤트 목록입니다.
        /// </summary>
        public IReadOnlyList<MergeHostEvent> Events => _events;

        /// <summary>
        /// 추가 골드입니다.
        /// </summary>
        public int BonusGold { get; set; }

        /// <summary>
        /// 이벤트를 추가합니다.
        /// </summary>
        public void AddEvent(MergeHostEvent evt)
        {
            _events.Add(evt);
        }
    }

    /// <summary>
    /// 머지 이펙트 인터페이스입니다.
    /// </summary>
    public interface IMergeEffect
    {
        /// <summary>
        /// 이펙트 ID입니다.
        /// </summary>
        string EffectId { get; }

        /// <summary>
        /// 머지 이펙트를 적용합니다.
        /// </summary>
        /// <param name="tick">현재 틱</param>
        /// <param name="state">게임 상태</param>
        /// <param name="source">소스 캐릭터 (흡수되는 쪽)</param>
        /// <param name="target">타겟 캐릭터 (남는 쪽)</param>
        /// <param name="result">결과 캐릭터</param>
        /// <param name="isSourceEffect">소스 이펙트인지 여부</param>
        /// <param name="effectResult">이펙트 결과</param>
        void Apply(
            long tick,
            MergeHostState state,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result,
            bool isSourceEffect,
            MergeEffectResult effectResult);
    }

    /// <summary>
    /// 머지 이펙트 시스템입니다.
    /// </summary>
    public sealed class MergeEffectSystem
    {
        private readonly Dictionary<string, IMergeEffect> _effects = new();
        private readonly MergeHostState _state;

        public MergeEffectSystem(MergeHostState state)
        {
            _state = state;
            RegisterDefaultEffects();
        }

        /// <summary>
        /// 이펙트를 등록합니다.
        /// </summary>
        public void RegisterEffect(IMergeEffect effect)
        {
            if (effect == null || string.IsNullOrEmpty(effect.EffectId))
            {
                return;
            }

            _effects[effect.EffectId] = effect;
        }

        /// <summary>
        /// 머지 이펙트를 적용하고 결과를 반환합니다.
        /// </summary>
        public MergeEffectResult ApplyMergeEffects(
            long tick,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result)
        {
            var effectResult = new MergeEffectResult();

            // 소스 캐릭터의 머지 이펙트 적용
            if (!string.IsNullOrEmpty(source.OnMergeSourceEffectId) &&
                _effects.TryGetValue(source.OnMergeSourceEffectId, out var sourceEffect))
            {
                sourceEffect.Apply(tick, _state, source, target, result, true, effectResult);

                effectResult.AddEvent(new MergeEffectTriggeredEvent(
                    tick,
                    source.OnMergeSourceEffectId,
                    source.Position.X,
                    source.Position.Y,
                    true,
                    source.Uid,
                    target.Uid,
                    result.Uid
                ));
            }

            // 타겟 캐릭터의 머지 이펙트 적용
            if (!string.IsNullOrEmpty(target.OnMergeTargetEffectId) &&
                _effects.TryGetValue(target.OnMergeTargetEffectId, out var targetEffect))
            {
                targetEffect.Apply(tick, _state, source, target, result, false, effectResult);

                effectResult.AddEvent(new MergeEffectTriggeredEvent(
                    tick,
                    target.OnMergeTargetEffectId,
                    target.Position.X,
                    target.Position.Y,
                    false,
                    source.Uid,
                    target.Uid,
                    result.Uid
                ));
            }

            return effectResult;
        }

        private void RegisterDefaultEffects()
        {
            RegisterEffect(new ExplosionOnMergeEffect());
            RegisterEffect(new GoldBonusOnMergeEffect());
            RegisterEffect(new StatBonusOnMergeEffect());
            RegisterEffect(new HealAlliesOnMergeEffect());
        }
    }

    #region 기본 머지 이펙트 구현

    /// <summary>
    /// 머지 시 주변 몬스터에게 데미지를 주는 이펙트입니다.
    /// </summary>
    public sealed class ExplosionOnMergeEffect : IMergeEffect
    {
        public string EffectId => "explosion";

        private readonly float _radius;
        private readonly float _damageMultiplier;

        public ExplosionOnMergeEffect(float radius = 3f, float damageMultiplier = 1.5f)
        {
            _radius = radius;
            _damageMultiplier = damageMultiplier;
        }

        public void Apply(
            long tick,
            MergeHostState state,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result,
            bool isSourceEffect,
            MergeEffectResult effectResult)
        {
            var center = isSourceEffect ? source.Position : target.Position;
            var baseDamage = result.ASC.Get(AttributeId.AttackDamage);
            var damage = baseDamage * _damageMultiplier;
            var radiusSq = _radius * _radius;

            foreach (var monster in state.Monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    continue;
                }

                var distSq = Point2D.DistanceSquared(center, monster.Position);
                if (distSq <= radiusSq)
                {
                    monster.TakeDamage(damage);

                    effectResult.AddEvent(new MonsterDamagedEvent(
                        tick,
                        monster.Uid,
                        damage,
                        monster.ASC.Get(AttributeId.Health),
                        0
                    ));
                }
            }
        }
    }

    /// <summary>
    /// 머지 시 골드 보너스를 주는 이펙트입니다.
    /// </summary>
    public sealed class GoldBonusOnMergeEffect : IMergeEffect
    {
        public string EffectId => "gold_bonus";

        private readonly int _baseGold;
        private readonly int _goldPerGrade;

        public GoldBonusOnMergeEffect(int baseGold = 10, int goldPerGrade = 5)
        {
            _baseGold = baseGold;
            _goldPerGrade = goldPerGrade;
        }

        public void Apply(
            long tick,
            MergeHostState state,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result,
            bool isSourceEffect,
            MergeEffectResult effectResult)
        {
            var bonusGold = _baseGold + result.Grade * _goldPerGrade;
            effectResult.BonusGold += bonusGold;
        }
    }

    /// <summary>
    /// 머지 결과 캐릭터에게 스탯 보너스를 주는 이펙트입니다.
    /// </summary>
    public sealed class StatBonusOnMergeEffect : IMergeEffect
    {
        public string EffectId => "stat_bonus";

        private readonly float _attackDamageBonus;
        private readonly float _attackSpeedBonus;

        public StatBonusOnMergeEffect(float attackDamageBonus = 5f, float attackSpeedBonus = 0.1f)
        {
            _attackDamageBonus = attackDamageBonus;
            _attackSpeedBonus = attackSpeedBonus;
        }

        public void Apply(
            long tick,
            MergeHostState state,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result,
            bool isSourceEffect,
            MergeEffectResult effectResult)
        {
            result.ASC.Add(AttributeId.AttackDamage, _attackDamageBonus);
            result.ASC.Add(AttributeId.AttackSpeed, _attackSpeedBonus);
        }
    }

    /// <summary>
    /// 머지 시 모든 아군 캐릭터를 회복시키는 이펙트입니다.
    /// </summary>
    public sealed class HealAlliesOnMergeEffect : IMergeEffect
    {
        public string EffectId => "heal_allies";

        private readonly float _healAmount;

        public HealAlliesOnMergeEffect(float healAmount = 10f)
        {
            _healAmount = healAmount;
        }

        public void Apply(
            long tick,
            MergeHostState state,
            MergeCharacter source,
            MergeCharacter target,
            MergeCharacter result,
            bool isSourceEffect,
            MergeEffectResult effectResult)
        {
            // 캐릭터는 일반적으로 HP가 없지만 확장성을 위해 구현
            foreach (var character in state.Characters.Values)
            {
                var maxHealth = character.ASC.Get(AttributeId.MaxHealth);
                if (maxHealth > 0)
                {
                    var currentHealth = character.ASC.Get(AttributeId.Health);
                    var newHealth = Math.Min(currentHealth + _healAmount, maxHealth);
                    character.ASC.Set(AttributeId.Health, newHealth);
                }
            }
        }
    }

    #endregion
}
