﻿using System;
using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Models;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.AI
{
    /// <summary>
    /// 경로(Path 1..N)를 따라 루프 이동하는 몬스터 AI입니다.
    /// 마지막 경로 끝에 도달하면 다시 0번 경로로 돌아갑니다.
    /// </summary>
    public sealed class MonsterPathMoveAI : IMergeMonsterAI
    {
        public void Tick(
            long tick,
            int playerIndex,
            float deltaTime,
            MergeMonster monster,
            MergeHostState state,
            List<MergeGameEvent> events)
        {
            if (monster == null || state == null || events == null)
            {
                return;
            }

            if (!monster.IsAlive)
            {
                return;
            }

            var pathCount = state.GetPlayerState(playerIndex)?.Paths.Count ?? 0;
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

            // 전체 경로 길이를 계산해 루프 이동 거리를 제한합니다.
            var loopLength = 0f;
            for (var i = 0; i < pathCount; i++)
            {
                var p = state.GetMonsterPath(playerIndex, i);
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

                var path = state.GetMonsterPath(playerIndex, pathIndex);
                if (path == null || path.TotalLength <= 0f)
                {
                    pathIndex = GetNextValidPathIndex(state, playerIndex, pathIndex);
                    progress = 0f;
                    continue;
                }

                var distToEnd = (1f - progress) * path.TotalLength;
                if (distToEnd <= 0f)
                {
                    pathIndex = GetNextValidPathIndex(state, playerIndex, pathIndex);
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
                    pathIndex = GetNextValidPathIndex(state, playerIndex, pathIndex);
                    progress = 0f;
                }
            }

            var finalPath = state.GetMonsterPath(playerIndex, pathIndex);
            if (finalPath == null || finalPath.TotalLength <= 0f)
            {
                return;
            }

            monster.PathIndex = pathIndex;
            monster.PathProgress = progress;
            monster.Position = finalPath.GetPositionAtProgress(progress);

            events.Add(new MonsterMovedEvent(
                tick,
                playerIndex,
                monster.Uid,
                monster.Position.X,
                monster.Position.Y,
                monster.Position.Z,
                monster.PathProgress
            ));
        }

        private static int GetNextValidPathIndex(MergeHostState state, int playerIndex, int currentPathIndex)
        {
            var count = state.GetPlayerState(playerIndex)?.Paths.Count ?? 0;
            if (count <= 0)
            {
                return currentPathIndex;
            }

            var idx = currentPathIndex;
            for (var i = 0; i < count; i++)
            {
                idx = (idx + 1) % count;
                var path = state.GetMonsterPath(playerIndex, idx);
                if (path != null && path.TotalLength > 0f)
                {
                    return idx;
                }
            }

            return currentPathIndex;
        }
    }
}
