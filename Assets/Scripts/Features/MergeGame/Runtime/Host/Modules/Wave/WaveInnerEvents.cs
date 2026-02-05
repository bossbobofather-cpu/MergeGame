using MyProject.MergeGame;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 웨이브 시작 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class WaveStartRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 시작 가능 여부입니다.
        /// </summary>
        public bool CanStart { get; set; }

        /// <summary>
        /// 실패 사유입니다.
        /// </summary>
        public string FailReason { get; set; }

        public WaveStartRequestInnerEvent(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 웨이브 상태 변경 알림 내부 이벤트입니다.
    /// </summary>
    public sealed class WaveStateChangedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 웨이브 페이즈입니다.
        /// </summary>
        public WavePhase Phase { get; }

        public WaveStateChangedInnerEvent(long tick, int waveNumber, WavePhase phase)
            : base(tick)
        {
            WaveNumber = waveNumber;
            Phase = phase;
        }
    }

    /// <summary>
    /// 몬스터 스폰 요청 내부 이벤트입니다.
    /// 외부 시스템(Host)에서 실제 몬스터 생성을 담당합니다.
    /// </summary>
    public sealed class MonsterSpawnRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 스폰할 몬스터 ID입니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 사용할 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 스폰 처리 완료 여부입니다.
        /// </summary>
        public bool Handled { get; set; }

        public MonsterSpawnRequestInnerEvent(long tick, string monsterId, int pathIndex, int waveNumber)
            : base(tick)
        {
            MonsterId = monsterId;
            PathIndex = pathIndex;
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 몬스터 사망 알림 내부 이벤트입니다.
    /// </summary>
    public sealed class MonsterDiedInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 사망한 몬스터 UID입니다.
        /// </summary>
        public long MonsterUid { get; }

        /// <summary>
        /// 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        public MonsterDiedInnerEvent(long tick, long monsterUid, int waveNumber)
            : base(tick)
        {
            MonsterUid = monsterUid;
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 웨이브 정보 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class WaveInfoRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 요청할 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 웨이브 정보입니다.
        /// </summary>
        public WaveInfo WaveInfo { get; set; }

        public WaveInfoRequestInnerEvent(long tick, int waveNumber)
            : base(tick)
        {
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 현재 웨이브 몬스터 수 요청 내부 이벤트입니다.
    /// </summary>
    public sealed class WaveMonsterCountRequestInnerEvent : InnerEventBase
    {
        /// <summary>
        /// 총 스폰 수입니다.
        /// </summary>
        public int TotalSpawned { get; set; }

        /// <summary>
        /// 남은 몬스터 수입니다.
        /// </summary>
        public int RemainingCount { get; set; }

        public WaveMonsterCountRequestInnerEvent(long tick)
            : base(tick)
        {
        }
    }
}
