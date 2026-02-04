using System;
using System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
    /// GameHost 공용 로그 라우터입니다.
    /// Unity/Server 환경에 맞게 델리게이트를 교체할 수 있습니다.
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
