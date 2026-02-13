using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public static class GameplayTagUtility
    {
        /// <summary>
        /// IsValidTagString 함수를 처리합니다.
        /// </summary>
        public static bool IsValidTagString(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (value[0] == '.' || value[value.Length - 1] == '.')
                return false;

            var previousDot = false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '.')
                {
                    if (previousDot) return false;
                    previousDot = true;
                    continue;
                }
                if (!IsAllowedTagChar(c)) return false;
                previousDot = false;
            }
            return !previousDot;
        }
        /// <summary>
        /// EnumerateParents 함수를 처리합니다.
        /// </summary>

        public static IEnumerable<string> EnumerateParents(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(value)) yield break;
            var index = value.Length;
            while ((index = value.LastIndexOf('.', index - 1)) >= 0)
                yield return value.Substring(0, index);
        }
        /// <summary>
        /// EnumerateTagAndParents 함수를 처리합니다.
        /// </summary>

        public static IEnumerable<string> EnumerateTagAndParents(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(value)) yield break;
            yield return value;
            foreach (var parent in EnumerateParents(value))
                yield return parent;
        }
        /// <summary>
        /// IsDescendant 함수를 처리합니다.
        /// </summary>

        public static bool IsDescendant(string child, string parent)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent))
                return false;
            if (parent.Length >= child.Length)
                return false;
            return child.StartsWith(parent, StringComparison.Ordinal) && child[parent.Length] == '.';
        }
        /// <summary>
        /// Fnv1a32 함수를 처리합니다.
        /// </summary>

        public static int Fnv1a32(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(value))
                return 0;
            
            const uint offset = 2166136261;
            const uint prime = 16777619;
            uint hash = offset;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }

            return unchecked((int)hash);
        }
        /// <summary>
        /// IsAllowedTagChar 함수를 처리합니다.
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAllowedTagChar(char c)
        {
            // 핵심 로직을 처리합니다.
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
        }

    }
}

