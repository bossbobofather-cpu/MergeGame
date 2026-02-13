using System.Collections.Generic;
using System.Linq;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class GameplayTagContainer
    {
        private readonly List<FGameplayTag> _tags = new();
        private readonly HashSet<int> _explicitTags = new();
        private readonly HashSet<int> _expandedTags = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;
        /// <summary>
        /// GameplayTagContainer 메서드입니다.
        /// </summary>

        public GameplayTagContainer()
        {
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
            _explicitTags.Clear();
            _expandedTags.Clear();
        }
        /// <summary>
        /// HasTagInternal 메서드입니다.
        /// </summary>

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            return includeParents ? _expandedTags.Contains(tag.Hash) : _explicitTags.Contains(tag.Hash);
        }
        /// <summary>
        /// RebuildCache 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// AddToCache 메서드입니다.
        /// </summary>

        private void AddToCache(FGameplayTag tag)
        {
            if (_explicitTags.Add(tag.Hash))
            {
                AddParentsToCache(tag);
            }
        }
        /// <summary>
        /// AddParentsToCache 메서드입니다.
        /// </summary>

        private void AddParentsToCache(FGameplayTag tag)
        {
            _expandedTags.Add(tag.Hash);
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

