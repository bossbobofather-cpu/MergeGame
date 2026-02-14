using System.Collections.Generic;
using Noname.GameAbilitySystem;
using MyProject.MergeGame.Models;
using MyProject.MergeGame.Events;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 머지 이펙트 결과입니다.
    /// </summary>
    public sealed class MergeEffectResult
    {
        private readonly List<MergeGameEvent> _events = new();

        /// <summary>
        /// 발생한 이벤트 목록입니다.
        /// </summary>
        public IReadOnlyList<MergeGameEvent> Events => _events;

        /// <summary>
        /// 추가 골드입니다.
        /// </summary>
        public int BonusGold { get; set; }

        /// <summary>
        /// 이벤트를 추가합니다.
        /// </summary>
        public void AddEvent(MergeGameEvent evt)
        {
            _events.Add(evt);
        }
    }

    /// <summary>
    /// 머지 이펙트 시스템입니다.
    /// 타워의 OnMergeSourceEffects/OnMergeTargetEffects에 설정된 GameplayEffect를
    /// GAS를 통해 결과 타워에 적용합니다.
    /// </summary>
    public sealed class MergeEffectSystem
    {
        private readonly List<GameplayEffect> _singleEffectBuffer = new(1);

        /// <summary>
        /// MergeEffectSystem 생성자입니다.
        /// </summary>
        public MergeEffectSystem()
        {
        }

        /// <summary>
        /// 머지 이펙트를 적용하고 결과를 반환합니다.
        /// </summary>
        public MergeEffectResult ApplyMergeEffects(
            long tick,
            int playerIndex,
            MergeTower source,
            MergeTower target,
            MergeTower result)
        {
            var effectResult = new MergeEffectResult();

            ApplyConfiguredEffects(
                tick,
                playerIndex,
                source,
                target,
                result,
                source?.OnMergeSourceEffects,
                true,
                effectResult);

            ApplyConfiguredEffects(
                tick,
                playerIndex,
                source,
                target,
                result,
                target?.OnMergeTargetEffects,
                false,
                effectResult);

            return effectResult;
        }

        private void ApplyConfiguredEffects(
            long tick,
            int playerIndex,
            MergeTower source,
            MergeTower target,
            MergeTower result,
            IReadOnlyList<GameplayEffect> effects,
            bool isSourceEffect,
            MergeEffectResult effectResult)
        {
            if (result?.ASC == null || effects == null || effects.Count == 0)
            {
                return;
            }

            var sourceAsc = result.ASC;
            var targetData = new TargetData(sourceAsc, result.Position);
            targetData.AddTarget(result.ASC);

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                _singleEffectBuffer.Clear();
                _singleEffectBuffer.Add(effect);
                sourceAsc.ApplyEffectsToTargets(_singleEffectBuffer, targetData);

                var effectKey = (long)effect.EffectTag.Hash;
                var effectPosition = isSourceEffect ? source?.Position ?? result.Position : target?.Position ?? result.Position;

                effectResult.AddEvent(new EffectTriggeredEvent(
                    tick,
                    playerIndex,
                    effectKey,
                    effectPosition.X,
                    effectPosition.Y,
                    effectPosition.Z,
                    isSourceEffect,
                    source?.Uid ?? 0,
                    target?.Uid ?? 0,
                    result.Uid
                ));
            }
        }
    }
}
