using System.Collections.Generic;
using MyProject.MergeGame.Models;
using MyProject.MergeGame.Systems;

namespace MyProject.MergeGame.AI
{
    /// <summary>
    /// 타워 AI 인터페이스입니다.
    /// Host에서 타워의 행동을 정의합니다.
    /// </summary>
    public interface IMergeTowerAI
    {
        void Tick(
            long tick,
            float deltaTime,
            MergeTower tower,
            MergeHostState state,
            MergeCombatSystem combatSystem,
            List<MergeHostEvent> events);
    }
}
