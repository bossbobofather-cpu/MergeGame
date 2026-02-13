using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public static class User
    {
        public const long InvalidUserId = 0;
        public const long OriginalEditorUserId = 99999;

        static readonly Regex CloneFolderPattern = new Regex(@"clone_(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static long UserId { get; private set; } = InvalidUserId;
        /// <summary>
        /// InitializeFromRuntime 메서드입니다.
        /// </summary>

        public static void InitializeFromRuntime()
        {
            UserId = ResolveRuntimeUserId(out bool isClone, out int cloneIndex, out string sourcePath);
            if (isClone)
            {
                Debug.Log($"[MergeGameUser] Clone runtime detected. Path='{sourcePath}', CloneIndex={cloneIndex}, UserId={UserId}");
                return;
            }

            Debug.Log($"[MergeGameUser] Original runtime detected. UserId={UserId}");
        }
        /// <summary>
        /// ResolveRuntimeUserId 메서드입니다.
        /// </summary>

        public static long ResolveRuntimeUserId()
        {
            return ResolveRuntimeUserId(out _, out _, out _);
        }

        static long ResolveRuntimeUserId(out bool isClone, out int cloneIndex, out string sourcePath)
        {
            if (TryResolveCloneIndex(out cloneIndex, out sourcePath))
            {
                isClone = true;
                return cloneIndex + 1L;
            }

            isClone = false;
            cloneIndex = -1;
            sourcePath = string.Empty;
            return OriginalEditorUserId;
        }

        static bool TryResolveCloneIndex(out int cloneIndex, out string sourcePath)
        {
            var paths = new[]
            {
                Application.dataPath,
                Environment.CurrentDirectory
            };

            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                Match match = CloneFolderPattern.Match(path.Replace('\\', '/'));
                if (!match.Success)
                {
                    continue;
                }

                if (!int.TryParse(match.Groups[1].Value, out cloneIndex))
                {
                    continue;
                }

                sourcePath = path;
                return true;
            }

            cloneIndex = -1;
            sourcePath = string.Empty;
            return false;
        }
    }
}
