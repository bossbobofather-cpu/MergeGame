using System.Collections.Generic;
using Noname.GameAbilitySystem;
using MyProject.MergeGame.Models;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 몬스터의 경로 이동을 처리하는 시스템입니다.
    /// </summary>
    public sealed class MonsterMovementSystem
    {
        private readonly MergeHostState _state;
        private readonly List<MergeHostEvent> _eventBuffer;
        private readonly List<long> _monstersReachedGoal;

        /// <summary>
        /// 목적지에 도달한 몬스터 UID 목록입니다.
        /// </summary>
        public IReadOnlyList<long> MonstersReachedGoal => _monstersReachedGoal;

        public MonsterMovementSystem(MergeHostState state)
        {
            _state = state;
            _eventBuffer = new List<MergeHostEvent>();
            _monstersReachedGoal = new List<long>();
        }

        /// <summary>
        /// 몬스터 이동을 업데이트하고 발생한 이벤트를 반환합니다.
        /// </summary>
        public IReadOnlyList<MergeHostEvent> Tick(long currentTick, float deltaTime)
        {
            _eventBuffer.Clear();
            _monstersReachedGoal.Clear();

            foreach (var monster in _state.Monsters.Values)
            {
                if (!monster.IsAlive || monster.ReachedGoal)
                {
                    continue;
                }

                ProcessMonsterMovement(currentTick, monster, deltaTime);
            }

            return _eventBuffer;
        }

        private void ProcessMonsterMovement(long tick, MergeMonster monster, float deltaTime)
        {
            var path = _state.GetMonsterPath(monster.PathIndex);
            if (path == null || path.TotalLength <= 0)
            {
                return;
            }

            // 이동 속도로 진행도 계산
            var moveSpeed = monster.ASC.Get(AttributeId.MoveSpeed);
            if (moveSpeed <= 0)
            {
                return;
            }

            // 진행도 증가량 = (이동 속도 * deltaTime) / 총 경로 길이
            var progressDelta = moveSpeed * deltaTime / path.TotalLength;
            monster.PathProgress += progressDelta;

            // 목적지 도달 체크
            if (monster.PathProgress >= 1f)
            {
                monster.PathProgress = 1f;
                monster.Position = path.GetEndPosition();
                _monstersReachedGoal.Add(monster.Uid);

                // 목적지 도달 이벤트
                _eventBuffer.Add(new MonsterReachedGoalEvent(
                    tick,
                    monster.Uid,
                    monster.DamageToPlayer
                ));
            }
            else
            {
                // 위치 업데이트
                monster.Position = path.GetPositionAtProgress(monster.PathProgress);

                // 이동 이벤트
                _eventBuffer.Add(new MonsterMovedEvent(
                    tick,
                    monster.Uid,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.PathProgress
                ));
            }
        }
    }
}
