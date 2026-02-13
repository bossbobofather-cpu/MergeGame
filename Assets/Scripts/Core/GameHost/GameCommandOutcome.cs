using System.Collections.Generic;

namespace Noname.GameHost
{
    /// <summary>
    /// 而ㅻ㎤??泥섎━ 寃곌낵? ?????대깽?몃? ?④퍡 ?ы븿?섎뒗 援ъ“泥댁엯?덈떎.
    /// </summary>
    public readonly struct GameCommandOutcome<TResult, TEvent>
    {
        /// <summary>
        /// 寃곌낵 諛쒗뻾 ?꾩뿉 諛쒗뻾???대깽??紐⑸줉?낅땲??
        /// </summary>
        public IReadOnlyList<TEvent> PreEvents { get; }

        /// <summary>
        /// 而ㅻ㎤??泥섎━ 寃곌낵?낅땲??
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// 寃곌낵 諛쒗뻾 ?꾩뿉 諛쒗뻾???대깽??紐⑸줉?낅땲??
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
