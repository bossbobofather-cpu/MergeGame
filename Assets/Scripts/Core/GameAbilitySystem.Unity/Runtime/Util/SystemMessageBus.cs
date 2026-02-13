using System;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// 시스템 메시지를 전파하는 버스입니다.
    /// </summary>
    public static class SystemMessageBus
    {
        /// <summary>
        /// 메시지가 발행될 때 호출됩니다.
        /// </summary>
        public static event Action<string> MessagePublished;

        /// <summary>
        /// 시스템 메시지를 발행합니다.
        /// </summary>
        /// <param name="message">메시지 내용</param>
        public static void Publish(string message)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrWhiteSpace(message))
            {
                // 빈 메시지는 무시한다.
                return;
            }

            MessagePublished?.Invoke(message);
        }
    }
}

