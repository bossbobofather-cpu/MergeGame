using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using MyProject.MergeGame.Models;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 캐릭터의 자동 공격을 처리하는 전투 시스템입니다.
    /// </summary>
    public sealed class MergeCombatSystem
    {
        private readonly MergeHostState _state;
        private readonly TargetContext _targetContext;
        private readonly ITargetingStrategy _targetingStrategy;
        private readonly List<MergeHostEvent> _eventBuffer;
        private readonly List<AbilitySystemComponent> _monsterAscList;

        public MergeCombatSystem(MergeHostState state, float defaultAttackRange = 10f)
        {
            _state = state;
            _eventBuffer = new List<MergeHostEvent>();
            _monsterAscList = new List<AbilitySystemComponent>();

            _targetingStrategy = new NearestEnemyTargetingStrategy(defaultAttackRange);

            _targetContext = new TargetContext(
                getEnemies: GetEnemiesForCharacter,
                getAllies: GetAlliesForCharacter,
                getPosition: GetPositionForAsc
            );
        }

        /// <summary>
        /// 전투 시스템을 업데이트하고 발생한 이벤트를 반환합니다.
        /// </summary>
        public IReadOnlyList<MergeHostEvent> Tick(long currentTick, float deltaTime)
        {
            _eventBuffer.Clear();

            foreach (var character in _state.Characters.Values)
            {
                ProcessCharacterAttack(currentTick, character, deltaTime);
            }

            return _eventBuffer;
        }

        private void ProcessCharacterAttack(long tick, MergeCharacter character, float deltaTime)
        {
            // 쿨타임 감소
            character.AttackCooldownRemaining -= deltaTime;

            if (character.AttackCooldownRemaining > 0)
            {
                return;
            }

            // 공격 대상 찾기
            var targetData = _targetingStrategy.FindTargets(character.ASC, _targetContext);
            if (targetData.Targets.Count == 0)
            {
                return;
            }

            // 공격력과 공격 속도 가져오기
            var attackDamage = character.ASC.Get(AttributeId.AttackDamage);
            var attackSpeed = character.ASC.Get(AttributeId.AttackSpeed);

            if (attackDamage <= 0 || attackSpeed <= 0)
            {
                return;
            }

            // 쿨타임 리셋 (1.0 / 공격속도)
            character.AttackCooldownRemaining = 1f / attackSpeed;

            // 각 대상에게 데미지 적용
            foreach (var targetAsc in targetData.Targets)
            {
                var monster = FindMonsterByAsc(targetAsc);
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                // 데미지 적용
                monster.TakeDamage(attackDamage);

                // 공격 이벤트 발행
                _eventBuffer.Add(new CharacterAttackedEvent(
                    tick,
                    character.Uid,
                    monster.Uid,
                    attackDamage,
                    character.Position.X,
                    character.Position.Y,
                    monster.Position.X,
                    monster.Position.Y
                ));

                // 몬스터 데미지 이벤트 발행
                _eventBuffer.Add(new MonsterDamagedEvent(
                    tick,
                    monster.Uid,
                    attackDamage,
                    monster.ASC.Get(AttributeId.Health),
                    character.Uid
                ));
            }
        }

        /// <summary>
        /// 캐릭터 기준으로 적(몬스터) 목록을 반환합니다.
        /// </summary>
        private IReadOnlyList<AbilitySystemComponent> GetEnemiesForCharacter(AbilitySystemComponent owner)
        {
            _monsterAscList.Clear();

            foreach (var monster in _state.Monsters.Values)
            {
                if (monster.IsAlive)
                {
                    _monsterAscList.Add(monster.ASC);
                }
            }

            return _monsterAscList;
        }

        /// <summary>
        /// 캐릭터 기준으로 아군(다른 캐릭터) 목록을 반환합니다.
        /// </summary>
        private IReadOnlyList<AbilitySystemComponent> GetAlliesForCharacter(AbilitySystemComponent owner)
        {
            var allies = new List<AbilitySystemComponent>();

            foreach (var character in _state.Characters.Values)
            {
                if (character.ASC != owner)
                {
                    allies.Add(character.ASC);
                }
            }

            return allies;
        }

        /// <summary>
        /// ASC의 위치를 반환합니다.
        /// </summary>
        private Point2D GetPositionForAsc(AbilitySystemComponent asc)
        {
            // 캐릭터에서 찾기
            foreach (var character in _state.Characters.Values)
            {
                if (character.ASC == asc)
                {
                    return character.Position;
                }
            }

            // 몬스터에서 찾기
            foreach (var monster in _state.Monsters.Values)
            {
                if (monster.ASC == asc)
                {
                    return monster.Position;
                }
            }

            return Point2D.zero;
        }

        /// <summary>
        /// ASC로 몬스터를 찾습니다.
        /// </summary>
        private MergeMonster FindMonsterByAsc(AbilitySystemComponent asc)
        {
            foreach (var monster in _state.Monsters.Values)
            {
                if (monster.ASC == asc)
                {
                    return monster;
                }
            }

            return null;
        }
    }
}
