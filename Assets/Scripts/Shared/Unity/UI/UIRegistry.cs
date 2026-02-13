using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// UI 타입과 프리팹을 매핑하는 레지스트리입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "UI/UI Registry")]
    public sealed class UIRegistry : ScriptableObject
    {
        [SerializeField] private List<Entry> _entries = new();

        /// <summary>
        /// 타입으로 UI 프리팹을 찾습니다.
        /// </summary>
        public bool TryGetPrefab<T>(out T prefab) where T : UIBase
        {
            if (TryGetPrefab(typeof(T), out var ui))
            {
                prefab = ui as T;
                return prefab != null;
            }

            prefab = null;
            return false;
        }

        /// <summary>
        /// 타입으로 UI 프리팹을 찾습니다.
        /// </summary>
        public bool TryGetPrefab(Type type, out UIBase prefab)
        {
            // 핵심 로직을 처리합니다.
            if (type == null)
            {
                prefab = null;
                return false;
            }

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry == null || entry.Prefab == null)
                {
                    continue;
                }

                if (entry.Matches(type))
                {
                    prefab = entry.Prefab;
                    return true;
                }
            }

            prefab = null;
            return false;
        }

        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private UIBase _prefab;

            public UIBase Prefab => _prefab;
            /// <summary>
            /// Matches 함수를 처리합니다.
            /// </summary>

            public bool Matches(Type type)
            {
                // 핵심 로직을 처리합니다.
                if (type == null || _prefab == null)
                {
                    return false;
                }

                var target = _prefab.GetType();
                return target == type;
            }
        }
        /// <summary>
        /// OnValidate 함수를 처리합니다.
        /// </summary>

        private void OnValidate()
        {
            // 핵심 로직을 처리합니다.
            RemoveNullEntries();
            RemoveDuplicateTypes();
        }
        /// <summary>
        /// RemoveNullEntries 함수를 처리합니다.
        /// </summary>

        private void RemoveNullEntries()
        {
            // 핵심 로직을 처리합니다.
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i] == null)
                {
                    _entries.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// RemoveDuplicateTypes 함수를 처리합니다.
        /// </summary>

        private void RemoveDuplicateTypes()
        {
            // 핵심 로직을 처리합니다.
            var seen = new HashSet<string>();
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry == null || entry.Prefab == null)
                {
                    continue;
                }

                var typeName = entry.Prefab.GetType().AssemblyQualifiedName;
                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                if (seen.Contains(typeName))
                {
                    _entries.RemoveAt(i);
                    continue;
                }

                seen.Add(typeName);
            }
        }
    }
}
