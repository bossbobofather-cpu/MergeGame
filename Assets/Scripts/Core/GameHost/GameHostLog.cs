using System;
using System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public static class GameHostLog
    {
        public static Action<string> Info = message => Debug.WriteLine(message);
        public static Action<string> Warning = message => Debug.WriteLine(message);
        public static Action<string> Error = message => Debug.WriteLine(message);

        public static void LogInfo(string message) => Info?.Invoke(message);
        public static void LogWarning(string message) => Warning?.Invoke(message);
        public static void LogError(string message) => Error?.Invoke(message);
    }
}
