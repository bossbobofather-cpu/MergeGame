using System;
using System.Collections.Generic;
using MyProject.MergeGame.Models;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 몬스터를 경로(Path 1..N) 순환 방식으로 이동시키는 시스템입니다.
    /// (마지막 경로 끝에 도달하면 다시 0번 경로로 돌아옵니다.)
    /// </summary>
    public sealed class MonsterMovementSystem
    {
        private readonly MergeHostState _state;
        private readonly List<MergeHostEvent> _eventBuffer;

        public MonsterMovementSystem(MergeHostState state)
        {
            _state = state;
            _eventBuffer = new List<MergeHostEvent>();
        }

        /// <summary>
        /// 몬스터 이동을 갱신하고 View/Client용 이벤트를 반환합니다.
        /// </summary>
        public IReadOnlyList<MergeHostEvent> Tick(long currentTick, float deltaTime)
        {
            _eventBuffer.Clear();

            foreach (var monster in _state.Monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    continue;
                }

                ProcessMonsterMovement(currentTick, monster, deltaTime);
            }

            return _eventBuffer;
        }

        private void ProcessMonsterMovement(long tick, MergeMonster monster, float deltaTime)
        {
            var pathCount = _state.Paths.Count;
            if (pathCount <= 0)
            {
                return;
            }

            var moveSpeed = monster.ASC.Get(AttributeId.MoveSpeed);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var remainingDistance = moveSpeed * deltaTime;
            if (remainingDistance <= 0f)
            {
                return;
            }

            var loopLength = 0f;
            for (var i = 0; i < pathCount; i++)
            {
                var p = _state.GetMonsterPath(i);
                if (p != null && p.TotalLength > 0f)
                {
                    loopLength += p.TotalLength;
                }
            }

            if (loopLength <= 0f)
            {
                return;
            }

            remainingDistance = remainingDistance % loopLength;

            var pathIndex = monster.PathIndex;
            var progress = Math.Clamp(monster.PathProgress, 0f, 1f);

            var guard = 0;
            while (remainingDistance > 0f && guard < 64)
            {
                guard++;

                var path = _state.GetMonsterPath(pathIndex);
                if (path == null || path.TotalLength <= 0f)
                {
                    pathIndex = GetNextValidPathIndex(pathIndex);
                    progress = 0f;
                    continue;
                }

                var distToEnd = (1f - progress) * path.TotalLength;
                if (distToEnd <= 0f)
                {
                    pathIndex = GetNextValidPathIndex(pathIndex);
                    progress = 0f;
                    continue;
                }

                if (remainingDistance < distToEnd)
                {
                    progress += remainingDistance / path.TotalLength;
                    remainingDistance = 0f;
                }
                else
                {
                    remainingDistance -= distToEnd;
                    pathIndex = GetNextValidPathIndex(pathIndex);
                    progress = 0f;
                }
            }

            var finalPath = _state.GetMonsterPath(pathIndex);
            if (finalPath == null || finalPath.TotalLength <= 0f)
            {
                return;
            }

            monster.PathIndex = pathIndex;
            monster.PathProgress = progress;
            monster.Position = finalPath.GetPositionAtProgress(progress);

            _eventBuffer.Add(new MonsterMovedEvent(
                tick,
                monster.Uid,
                monster.Position.X,
                monster.Position.Y,
                monster.Position.Z,
                monster.PathProgress
            ));
        }

        private int GetNextValidPathIndex(int currentPathIndex)
        {
            var count = _state.Paths.Count;
            if (count <= 0)
            {
                return currentPathIndex;
            }

            var idx = currentPathIndex;
            for (var i = 0; i < count; i++)
            {
                idx = (idx + 1) % count;
                var p = _state.GetMonsterPath(idx);
                if (p != null && p.TotalLength > 0f)
                {
                    return idx;
                }
            }

            return currentPathIndex;
        }
    }
}
