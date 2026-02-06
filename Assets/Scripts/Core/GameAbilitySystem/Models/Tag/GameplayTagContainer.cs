using System.Collections.Generic;
using System.Linq;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// 주석 정리
    /// </summary>
    public sealed class GameplayTagContainer
    {
        private readonly List<FGameplayTag> _tags = new();
        private readonly HashSet<int> _explicitTags = new();
        private readonly HashSet<int> _expandedTags = new();

        /// <summary>
        /// 주석 정리
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;

        public GameplayTagContainer()
        {

        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public bool HasAny(GameplayTagContainer other)
        {
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
        /// 주석 정리
        /// </summary>
        public bool HasAll(GameplayTagContainer other)
        {
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
        /// 주석 정리
        /// </summary>
        public bool AddTag(FGameplayTag tag)
        {
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
        /// 주석 정리
        /// </summary>
        public void RemoveTag(FGameplayTag tag)
        {
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
        /// 주석 정리
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
            _explicitTags.Clear();
            _expandedTags.Clear();
        }

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            return includeParents ? _expandedTags.Contains(tag.Hash) : _explicitTags.Contains(tag.Hash);
        }

        private void RebuildCache()
        {
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

        private void AddToCache(FGameplayTag tag)
        {
            if (_explicitTags.Add(tag.Hash))
            {
                AddParentsToCache(tag);
            }
        }

        private void AddParentsToCache(FGameplayTag tag)
        {
            _expandedTags.Add(tag.Hash);

            // 주석 정리
            var lastDotIndex = tag.Value.LastIndexOf('.');
            while (lastDotIndex > 0)
            {
                var parent = tag.Value.Substring(0, lastDotIndex);
                _expandedTags.Add(parent.GetHashCode());
                lastDotIndex = parent.LastIndexOf('.');
            }
        }
    }
}

