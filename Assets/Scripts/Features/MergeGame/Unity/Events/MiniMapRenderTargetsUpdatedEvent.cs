using System.Collections.Generic;
using Noname.GameHost.GameEvent;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Events
{
    /// <summary>
    /// 미니맵 렌더 텍스처 단건 정보입니다.
    /// </summary>
    public readonly struct MiniMapRenderTargetInfo
    {
        public int PlayerIndex { get; }
        public Texture Texture { get; }

        public MiniMapRenderTargetInfo(int playerIndex, Texture texture)
        {
            PlayerIndex = playerIndex;
            Texture = texture;
        }
    }

    /// <summary>
    /// 미니맵 렌더 타겟 목록이 갱신되었음을 알리는 씬 이벤트입니다.
    /// </summary>
    public sealed class MiniMapRenderTargetsUpdatedEvent : SceneGameEventContext
    {
        public IReadOnlyList<MiniMapRenderTargetInfo> Targets { get; }
        public int Version { get; }
        /// <summary>
        /// MiniMapRenderTargetsUpdatedEvent 함수를 처리합니다.
        /// </summary>

        public MiniMapRenderTargetsUpdatedEvent(object source, IReadOnlyList<MiniMapRenderTargetInfo> targets, int version)
            : base(source)
        {
            // 핵심 로직을 처리합니다.
            Targets = targets;
            Version = version;
        }
    }
}
