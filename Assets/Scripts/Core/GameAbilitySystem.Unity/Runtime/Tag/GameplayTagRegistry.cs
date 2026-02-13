using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// 게임플레이 태그 레지스트리입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/TagRegistry")]
    public sealed class GameplayTagRegistry : ScriptableObject
    {
        [SerializeField] private List<string> _tags = new();

        /// <summary>
        /// 등록된 태그 전체 목록을 가져옵니다.
        /// </summary>
        /// <param name="includeParents">부모 태그 포함 여부</param>
        /// <returns>정렬된 태그 목록</returns>
        public List<string> GetAllTags(bool includeParents = true)
        {
            // 중복 제거를 위해 집합을 사용한다.
            var set = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                if (!GameplayTagUtility.IsValidTagString(tag))
                {
                    continue;
                }

                set.Add(tag);
                if (includeParents)
                {
                    foreach (var parent in GameplayTagUtility.EnumerateParents(tag))
                    {
                        set.Add(parent);
                    }
                }
            }

            var list = new List<string>(set);
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        /// <summary>
        /// 태그가 정의되어 있는지 확인합니다.
        /// </summary>
        /// <param name="value">확인할 태그 문자열</param>
        /// <param name="includeParents">부모 태그 포함 여부</param>
        /// <returns>정의 여부</returns>
        public bool IsTagDefined(string value, bool includeParents = true)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!GameplayTagUtility.IsValidTagString(value))
            {
                return false;
            }

            if (includeParents)
            {
                for (var i = 0; i < _tags.Count; i++)
                {
                    var tag = _tags[i];
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        continue;
                    }

                    if (!GameplayTagUtility.IsValidTagString(tag))
                    {
                        continue;
                    }

                    if (string.Equals(tag, value, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    foreach (var parent in GameplayTagUtility.EnumerateParents(tag))
                    {
                        if (string.Equals(parent, value, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            for (var i = 0; i < _tags.Count; i++)
            {
                if (string.Equals(_tags[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

