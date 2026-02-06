using System.Collections.Generic;
using MyProject.MergeGame.Models;

namespace MyProject.MergeGame.AI
{
    /// <summary>
    /// 몬스터 AI 인터페이스입니다.
    /// Host에서 몬스터의 행동을 정의합니다.
    /// </summary>
    public interface IMergeMonsterAI
    {
        void Tick(
            long tick,
            float deltaTime,
            MergeMonster monster,
            MergeHostState state,
            List<MergeHostEvent> events);
    }
}
