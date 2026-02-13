using System.Collections.Generic;

namespace Noname.GameHost
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public readonly struct GameCommandOutcome<TResult, TEvent>
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public IReadOnlyList<TEvent> PreEvents { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public IReadOnlyList<TEvent> PostEvents { get; }

        public GameCommandOutcome(TResult result, IReadOnlyList<TEvent> postEvents = null)
        {
            PreEvents = null;
            Result = result;
            PostEvents = postEvents;
        }

        public GameCommandOutcome(
            IReadOnlyList<TEvent> preEvents,
            TResult result,
            IReadOnlyList<TEvent> postEvents = null)
        {
            PreEvents = preEvents;
            Result = result;
            PostEvents = postEvents;
        }
    }
}
