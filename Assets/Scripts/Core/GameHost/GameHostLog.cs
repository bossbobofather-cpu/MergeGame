using System;
using System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
    /// GameHost 怨듭슜 濡쒓렇 ?쇱슦?곗엯?덈떎.
    /// Unity/Server ?섍꼍??留욊쾶 ?몃━寃뚯씠?몃? 援먯껜?????덉뒿?덈떎.
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
