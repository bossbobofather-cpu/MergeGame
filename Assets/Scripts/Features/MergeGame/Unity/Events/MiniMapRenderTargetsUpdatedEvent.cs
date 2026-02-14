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
        /// <summary>
        /// PlayerIndex 속성입니다.
        /// </summary>
        public int PlayerIndex { get; }
        /// <summary>
        /// Texture 속성입니다.
        /// </summary>
        public Texture Texture { get; }

        /// <summary>
        /// MiniMapRenderTargetInfo 생성자입니다.
        /// </summary>
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
        /// <summary>
        /// Targets 속성입니다.
        /// </summary>
        public IReadOnlyList<MiniMapRenderTargetInfo> Targets { get; }
        /// <summary>
        /// Version 속성입니다.
        /// </summary>
        public int Version { get; }
        /// <summary>
        /// MiniMapRenderTargetsUpdatedEvent 메서드입니다.
        /// </summary>

        public MiniMapRenderTargetsUpdatedEvent(object source, IReadOnlyList<MiniMapRenderTargetInfo> targets, int version)
            : base(source)
        {
            Targets = targets;
            Version = version;
        }
    }
}
