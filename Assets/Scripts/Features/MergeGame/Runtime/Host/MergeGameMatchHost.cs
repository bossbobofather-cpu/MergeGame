using System;
using MyProject.MergeGame.Commands;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 1:1 매치를 위한 서버(권위) 오케스트레이터입니다.
    /// 두 개의 MergeGameHost를 묶어서 "상대에게 영향을 주는" 룰(테트리스식 공격)을 라우팅합니다.
    /// 
    /// - Host는 여전히 단일 게임(플레이어 1명)만 책임집니다.
    /// - MatchHost는 "호스트 A의 이벤트"를 보고 "호스트 B에 커맨드"를 주입하는 식으로 연결합니다.
    /// 
    /// Mirror/NGO 같은 네트워크는 이 MatchHost 위에 어댑터 계층으로 얹는 것을 권장합니다.
    /// </summary>
    public sealed class MergeGameMatchHost : IDisposable
    {
        private const long SERVER_UID = 0;

        private readonly MergeGameHost _hostA;
        private readonly MergeGameHost _hostB;

        private readonly int _killsPerGarbage;
        private int _killsA;
        private int _killsB;

        private bool _matchEnded;

        public MergeGameMatchHost(MergeGameHost hostA, MergeGameHost hostB, int killsPerGarbage = 10)
        {
            _hostA = hostA ?? throw new ArgumentNullException(nameof(hostA));
            _hostB = hostB ?? throw new ArgumentNullException(nameof(hostB));

            _killsPerGarbage = Math.Max(1, killsPerGarbage);

            // NOTE:
            // EventRaised는 FlushEvents()에서 호출됩니다.
            // 서버 환경에서는 MatchHost가 FlushEvents를 한 곳에서만 호출하면 됩니다.
            // (클라이언트 View와 동시에 같은 Host를 FlushEvents 하면 이벤트를 선점하게 됩니다.)
            _hostA.EventRaised += OnHostAEvent;
            _hostB.EventRaised += OnHostBEvent;
        }

        public void Dispose()
        {
            _hostA.EventRaised -= OnHostAEvent;
            _hostB.EventRaised -= OnHostBEvent;
        }

        private void OnHostAEvent(MergeHostEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            // 패배자가 결정되면 매치를 종료합니다.
            if (!_matchEnded && evt is MergeGameOverEvent over)
            {
                _matchEnded = true;

                // A가 패배했다면 B는 승리 처리합니다.
                if (!over.IsVictory)
                {
                    _hostB.SendCommand(new EndMergeGameCommand(SERVER_UID));
                }

                return;
            }

            if (_matchEnded)
            {
                return;
            }

            if (evt is MonsterDiedEvent)
            {
                _killsA++;
                TrySendGarbage(fromA: true);
            }
        }

        private void OnHostBEvent(MergeHostEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (!_matchEnded && evt is MergeGameOverEvent over)
            {
                _matchEnded = true;

                // B가 패배했다면 A는 승리 처리합니다.
                if (!over.IsVictory)
                {
                    _hostA.SendCommand(new EndMergeGameCommand(SERVER_UID));
                }

                return;
            }

            if (_matchEnded)
            {
                return;
            }

            if (evt is MonsterDiedEvent)
            {
                _killsB++;
                TrySendGarbage(fromA: false);
            }
        }

        private void TrySendGarbage(bool fromA)
        {
            if (fromA)
            {
                var sendCount = _killsA / _killsPerGarbage;
                if (sendCount <= 0)
                {
                    return;
                }

                _killsA -= sendCount * _killsPerGarbage;

                // 상대 Host에 몬스터를 주입합니다.
                _hostB.SendCommand(new InjectMonstersCommand(SERVER_UID, monsterId: "monster_default", count: sendCount, pathIndex: 0));
            }
            else
            {
                var sendCount = _killsB / _killsPerGarbage;
                if (sendCount <= 0)
                {
                    return;
                }

                _killsB -= sendCount * _killsPerGarbage;

                _hostA.SendCommand(new InjectMonstersCommand(SERVER_UID, monsterId: "monster_default", count: sendCount, pathIndex: 0));
            }
        }
    }
}
