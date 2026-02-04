using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// 게임플레이 태그를 관리하는 컨테이너입니다.
    /// </summary>
    [Serializable]
    public sealed class GameplayTagContainerView
    {
        [SerializeField] private List<FGameplayTagView> _tags = new();

        /// <summary>
        /// 보관 중인 태그 목록입니다.
        /// </summary>
        public IReadOnlyList<FGameplayTagView> Tags => _tags;
    }
}

