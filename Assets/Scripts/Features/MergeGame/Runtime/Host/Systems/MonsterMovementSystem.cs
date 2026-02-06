using System;
using System.Collections.Generic;
using MyProject.MergeGame.Models;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 몬스터의 경로 이동을 처리합니다.
    /// 기존 "Goal 도착 = 종료" 규칙이 아닌, 경로(패스)들을 순환(loop)하는 이동 규칙을 사용합니다.
    /// 
    /// - 몬스터는 현재 PathIndex의 경로를 따라 이동합니다.
    /// - PathProgress(0~1)는 "현재 경로" 내부 진행률입니다.
    /// - progress가 1을 넘으면 다음 PathIndex로 넘어가며, 마지막 경로 이후에는 0번 경로로 되돌아갑니다.
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
        /// 몬스터 이동을 업데이트하고 View/Client용 이벤트를 반환합니다.
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

            // 이번 틱에 이동해야 하는 월드 거리
            var remainingDistance = moveSpeed * deltaTime;
            if (remainingDistance <= 0f)
            {
                return;
            }

            // deltaTime이 크게 튀어도(프레임 드랍/일시정지 등) 무한 루프/과도한 while을 피하기 위해
            // 경로 전체 길이로 한번 모듈러 처리합니다.
            var loopLength = 0f;
            for (var i = 0; i < pathCount; i++)
            {
                var p = _state.GetMonsterPath(i);
                if (p != null && p.TotalLength > 0f)
                {
                    loopLength += p.TotalLength;
                }
            }

            if (loopLength > 0f)
            {
                remainingDistance = remainingDistance % loopLength;
            }
            else
            {
                return;
            }

            var pathIndex = monster.PathIndex;
            var progress = Math.Clamp(monster.PathProgress, 0f, 1f);

            // NOTE:
            // 경로 길이가 서로 다를 수 있으므로, progress 델타가 아니라 "거리" 기준으로 경로를 넘깁니다.
            // (progress 기반 wrap은 다음 경로 길이에 따라 실제 거리와 불일치할 수 있습니다.)
            var guard = 0;
            while (remainingDistance > 0f && guard < 64)
            {
                guard++;

                var path = _state.GetMonsterPath(pathIndex);
                if (path == null || path.TotalLength <= 0f)
                {
                    // 현재 경로가 비정상이라면 다음 유효 경로로 스킵합니다.
                    pathIndex = GetNextValidPathIndex(pathIndex);
                    progress = 0f;
                    continue;
                }

                var distToEnd = (1f - progress) * path.TotalLength;

                // progress가 1.0 근처에서 부동소수 오차로 0이 될 수 있으니 방어합니다.
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

            // 다음 인덱스부터 순환하며 유효한 경로를 찾습니다.
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
