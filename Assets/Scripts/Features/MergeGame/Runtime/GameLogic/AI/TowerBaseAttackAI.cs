using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Models;
using MyProject.MergeGame.Systems;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.AI
{
    /// <summary>
    /// 기본 공격(Ability.BaseAttack)을 발동하는 타워 AI입니다.
    /// 더티 플래그가 켜진 경우에만 활성화를 시도합니다.
    /// </summary>
    public sealed class TowerBaseAttackAI : IMergeTowerAI
    {
        private readonly FGameplayTag _baseAttackTag;
        /// <summary>
        /// TowerBaseAttackAI 함수를 처리합니다.
        /// </summary>

        public TowerBaseAttackAI(string baseAttackTag = "Ability.BaseAttack")
        {
            // 핵심 로직을 처리합니다.
            _baseAttackTag = new FGameplayTag(baseAttackTag);
        }

        public void Tick(
            long tick,
            int playerIndex,
            float deltaTime,
            MergeTower tower,
            MergeHostState state,
            MergeCombatSystem combatSystem,
            List<MergeGameEvent> events)
        {
            if (tower == null || combatSystem == null || events == null)
            {
                return;
            }

            // 더티가 꺼져 있으면 활성화 시도를 생략합니다.
            if (!tower.ASC.ConsumeAbilityDirty())
            {
                return;
            }

            var specs = tower.ASC.GetAbilitySpecs();
            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (spec == null || spec.Ability == null)
                {
                    continue;
                }

                if (!spec.AbilityTag.Equals(_baseAttackTag))
                {
                    continue;
                }

                if (tower.ASC.TryActivateAbility(spec, combatSystem.TargetContext, out var targetData, applyAppliedEffectsOnActivate: false))
                {
                    combatSystem.ExecuteTowerAttack(tick, playerIndex, tower, spec.Ability, targetData, events);
                }
            }
        }
    }
}
