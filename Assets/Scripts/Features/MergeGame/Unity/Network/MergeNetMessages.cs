using Mirror;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 클라이언트 -> 서버로 전달하는 커맨드 타입입니다.
    /// </summary>
    public enum MergeNetCommandType : ushort
    {
        None = 0,

        // NOTE:
        // StartGame(1)은 초기 프로토타입에서 "즉시 시작" 용도로 사용했었습니다.
        // 현재는 Ready 게이트를 두고 시작시키므로, StartGame도 Ready와 동일하게 취급할 수 있습니다.
        StartGame = 1,
        SpawnTower = 2,
        MergeTower = 3,

        /// <summary>
        /// 게임 준비(Ready) 커맨드입니다.
        /// 서버는 두 플레이어의 Ready를 모두 받으면 게임을 시작합니다.
        /// </summary>
        Ready = 4,
    }

    /// <summary>
    /// 클라이언트 -> 서버 커맨드 메시지입니다.
    /// 복잡한 직렬화 대신, 최소한의 공용 필드(Int/Str)를 사용합니다.
    /// </summary>
    public struct CommandMsg : NetworkMessage
    {
        public int PlayerIndex;
        public long SenderUid;
        public MergeNetCommandType CommandType;
        public int Int0;
        public int Int1;
        public string Str0;
    }

    /// <summary>
    /// 서버 -> 클라이언트 스냅샷 메시지입니다.
    /// View는 스냅샷을 source of truth로 삼아 동기화합니다.
    /// </summary>
    public struct SnapshotMsg : NetworkMessage
    {
        public int PlayerIndex;
        public long Tick;
        public int SessionPhase;
        public int WaveNumber;
        public int WavePhase;
        public int MonsterCount;
        public int TowerCount;
        public int UsedSlotCount;
        public float SampleMonsterProgress0;
        public float SampleMonsterProgress1;
    }

    /// <summary>
    /// 서버 -> 클라이언트 이벤트 메시지 타입입니다.
    /// 현재는 로깅 목적이므로 Log만 사용합니다.
    /// </summary>
    public enum MergeNetEventType : ushort
    {
        None = 0,
        Log = 1,
    }

    /// <summary>
    /// 서버 -> 클라이언트 이벤트 메시지입니다.
    /// </summary>
    public struct EventMsg : NetworkMessage
    {
        public int PlayerIndex;
        public long Tick;
        public MergeNetEventType EventType;
        public string Text;
    }
}

