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
    /// 寃뚯엫 ?뚮젅?댁뼱 ?뺣낫
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
        /// MergeGamePlayer 함수를 처리합니다.
        /// </summary>
        public MergeGamePlayer(long uid, int index)
        {
            // 핵심 로직을 처리합니다.
            _uid = uid;
            _index = index;
            _state = MergeGamePlayerState.None;
        }
        /// <summary>
        /// Dispose 함수를 처리합니다.
        /// </summary>

        public void Dispose()
        {
            // 핵심 로직을 처리합니다.
            _uid = 0;
            _index = -1;
            _state = MergeGamePlayerState.None;
        }
        /// <summary>
        /// SetState 함수를 처리합니다.
        /// </summary>

        public void SetState(MergeGamePlayerState state)
        {
            // 핵심 로직을 처리합니다.
            _state = state;
        }
    }
}
