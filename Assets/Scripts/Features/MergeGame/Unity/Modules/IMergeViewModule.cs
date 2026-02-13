using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame View 紐⑤뱢 ?명꽣?섏씠?ㅼ엯?덈떎.
    /// ?쒕쾭 ?곌껐 諛??댁젣, ?몄뒪???대깽?몄? ?ㅻ깄?룹쓣 ?섏떊?????덉뒿?덈떎.
    /// </summary>
    public interface IMergeViewModule
    {
        /// <summary>
        /// ?쒕쾭 ?곌껐 ???몄텧 ?⑸땲??
        /// </summary>
        void OnConnectedEvent();

        /// <summary>
        /// ?쒕쾭 ?곌껐 ?댁젣 ???몄텧?⑸땲??
        /// </summary>
        void OnDisconnectedEvent();

        /// <summary>
        /// ?몄뒪???대깽???섏떊 ???몄텧?⑸땲??
        /// </summary>
        void OnEventMsg(MergeGameEvent evt);

        /// <summary>
        /// 而ㅻ㎤??寃곌낵 ?섏떊 ???몄텧?⑸땲??
        /// </summary>
        void OnCommandResultMsg(MergeCommandResult result);

        /// <summary>
        /// ?ㅻ깄??媛깆떊 ???몄텧?⑸땲??
        /// </summary>
        void OnSnapshotMsg(MergeHostSnapshot snapshot);
    }
}
