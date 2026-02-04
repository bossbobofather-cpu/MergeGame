using System.Collections.Generic;
using System.Linq;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ê²Œì„?Œë ˆ???œê·¸ë¥?ê´€ë¦¬í•˜??ì»¨í…Œ?´ë„ˆ?…ë‹ˆ??(?œìˆ˜ C# ëª¨ë¸).
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¬ìš© ê°€?¥í•©?ˆë‹¤.
    /// </summary>
    public sealed class GameplayTagContainer
    {
        private readonly List<FGameplayTag> _tags = new();
        private readonly HashSet<int> _explicitTags = new();
        private readonly HashSet<int> _expandedTags = new();

        /// <summary>
        /// ë³´ê? ì¤‘ì¸ ?œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;

        public GameplayTagContainer()
        {

        }

        /// <summary>
        /// ë¶€ëª??œê·¸ê¹Œì? ?¬í•¨?˜ì—¬ ë³´ìœ  ?¬ë?ë¥??•ì¸?©ë‹ˆ??
        /// </summary>
        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// ?•í™•???¼ì¹˜?˜ëŠ” ?œê·¸ë§??•ì¸?©ë‹ˆ??
        /// </summary>
        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// ?¤ë¥¸ ì»¨í…Œ?´ë„ˆ???œê·¸ ì¤??˜ë‚˜?¼ë„ ?¬í•¨?˜ëŠ”ì§€ ?•ì¸?©ë‹ˆ??
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
        /// ?¤ë¥¸ ì»¨í…Œ?´ë„ˆ???œê·¸ë¥?ëª¨ë‘ ?¬í•¨?˜ëŠ”ì§€ ?•ì¸?©ë‹ˆ??
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
        /// ?œê·¸ë¥?ì¶”ê??©ë‹ˆ??
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
        /// ?œê·¸ë¥??œê±°?©ë‹ˆ??
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
        /// ?œê·¸ë¥?ëª¨ë‘ ?œê±°?©ë‹ˆ??
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

            // ë¶€ëª??œê·¸ ì¶”ê? (?? "Ability.Attack.Fire" -> "Ability.Attack", "Ability")
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

