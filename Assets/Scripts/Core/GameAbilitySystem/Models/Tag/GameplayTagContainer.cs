using System.Collections.Generic;
using System.Linq;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public sealed class GameplayTagContainer
    {
        private readonly List<FGameplayTag> _tags = new();
        private readonly HashSet<int> _explicitTags = new();
        private readonly HashSet<int> _expandedTags = new();

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;
        /// <summary>
        /// GameplayTagContainer 함수를 처리합니다.
        /// </summary>

        public GameplayTagContainer()
        {
            // 핵심 로직을 처리합니다.

        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool HasTag(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool HasTagExact(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool HasAny(GameplayTagContainer other)
        {
            // 핵심 로직을 처리합니다.
            if (other == null)
            {
                return false;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (HasTag(other._tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool HasAll(GameplayTagContainer other)
        {
            // 핵심 로직을 처리합니다.
            if (other == null)
            {
                return true;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (!HasTag(other._tags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool AddTag(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            if (_explicitTags.Contains(tag.Hash))
            {
                return false;
            }

            _tags.Add(tag);
            AddToCache(tag);
            return true;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void RemoveTag(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            for (var i = _tags.Count - 1; i >= 0; i--)
            {
                if (_tags[i].Equals(tag))
                {
                    _tags.RemoveAt(i);
                }
            }

            RebuildCache();
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void Clear()
        {
            // 핵심 로직을 처리합니다.
            _tags.Clear();
            _explicitTags.Clear();
            _expandedTags.Clear();
        }
        /// <summary>
        /// HasTagInternal 함수를 처리합니다.
        /// </summary>

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            return includeParents ? _expandedTags.Contains(tag.Hash) : _explicitTags.Contains(tag.Hash);
        }
        /// <summary>
        /// RebuildCache 함수를 처리합니다.
        /// </summary>

        private void RebuildCache()
        {
            // 핵심 로직을 처리합니다.
            _explicitTags.Clear();
            _expandedTags.Clear();

            for (var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                if (string.IsNullOrEmpty(tag.Value))
                {
                    continue;
                }

                if (_explicitTags.Add(tag.Hash))
                {
                    AddParentsToCache(tag);
                }
            }
        }
        /// <summary>
        /// AddToCache 함수를 처리합니다.
        /// </summary>

        private void AddToCache(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            if (_explicitTags.Add(tag.Hash))
            {
                AddParentsToCache(tag);
            }
        }
        /// <summary>
        /// AddParentsToCache 함수를 처리합니다.
        /// </summary>

        private void AddParentsToCache(FGameplayTag tag)
        {
            // 핵심 로직을 처리합니다.
            _expandedTags.Add(tag.Hash);

            // 二쇱꽍 ?뺣━
            var lastDotIndex = tag.Value.LastIndexOf('.');
            while (lastDotIndex > 0)
            {
                var parent = tag.Value.Substring(0, lastDotIndex);
                _expandedTags.Add(GameplayTagUtility.Fnv1a32(parent));
                lastDotIndex = parent.LastIndexOf('.');
            }
        }
    }
}

