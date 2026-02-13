using System;

namespace MyProject.MergeGame.Unity.Network
{
    public enum MergeGamePlayerState : ushort
    {
        None,
        Ready,
        Started,
        GameOver,
    }

    /// <summary>
    /// 매치에 참가한 플레이어의 서버 상태를 보관합니다.
    /// </summary>
    public class MergeGamePlayer : IDisposable
    {
        private long _uid;
        private int _index;
        private MergeGamePlayerState _state;

        public long Uid => _uid;
        public int Index => _index;
        public MergeGamePlayerState State => _state;

        /// <summary>
        /// 플레이어 정보를 생성합니다.
        /// </summary>
        public MergeGamePlayer(long uid, int index)
        {
            _uid = uid;
            _index = index;
            _state = MergeGamePlayerState.None;
        }

        /// <summary>
        /// 보관 중인 플레이어 상태를 초기값으로 되돌립니다.
        /// </summary>
        public void Dispose()
        {
            _uid = 0;
            _index = -1;
            _state = MergeGamePlayerState.None;
        }

        /// <summary>
        /// 플레이어 상태를 갱신합니다.
        /// </summary>
        public void SetState(MergeGamePlayerState state)
        {
            _state = state;
        }
    }
}
