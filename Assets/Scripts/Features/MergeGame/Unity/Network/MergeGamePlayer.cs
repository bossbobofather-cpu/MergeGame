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
    /// 게임 플레이어 정보
    /// </summary>
    public class MergeGamePlayer : IDisposable
    {
        private long _uid;
        private int _index;
        private MergeGamePlayerState _state;
        public long Uid => _uid;
        public int Index => _index;
        public MergeGamePlayerState State => _state;
        public MergeGamePlayer(long uid, int index)
        {
            _uid = uid;
            _index = index;
            _state = MergeGamePlayerState.None;
        }

        public void Dispose()
        {
            _uid = 0;
            _index = -1;
            _state = MergeGamePlayerState.None;
        }

        public void SetState(MergeGamePlayerState state)
        {
            _state = state;
        }
    }
}