using System;
using Mirror;

namespace MyProject.MergeGame.Unity.Network
{
    /// <summary>
    /// 클라이언트 -> 서버로 전달하는 커맨드 타입입니다.
    /// </summary>
    public enum MergeNetCommandType : ushort
    {
        None = 0,
        ReadyGame = 2,
        ExitGame = 3,           //게임의 종료는 Host가 판단한다. Client는 게임 도중 나가기에 대한 커맨드만
        SpawnTower = 10,
        MergeTower = 11,
        InjectMonsters = 20,
    }

    public struct NetAuthenticateMessage : NetworkMessage
    {
        public long UserId;
    }

    /// <summary>
    /// 서버 -> 클라이언트 인증 응답 메시지
    /// </summary>
    public struct NetAuthResponseMessage : NetworkMessage
    {
        public bool Success;
        public string Message;
    }

    /// <summary>
    /// 클라이언트 -> 서버 커맨드 메시지입니다.
    /// </summary>
    public struct NetCommandMessage : NetworkMessage
    {
        public long SenderUid;
        public MergeNetCommandType CommandType;

        public ArraySegment<byte> Payload;
    }

    /// <summary>
    /// 서버 -> 클라이언트 커맨드 결과 메시지입니다.
    /// </summary>
    public struct NetCommandResultMessage : NetworkMessage
    {
        public long SenderUid;
        public MergeNetCommandType CommandType;

        public ArraySegment<byte> Payload;
    }

    /// <summary>
    /// 서버 -> 클라이언트 스냅샷 메시지입니다.
    /// View는 스냅샷을 source of truth로 삼아 동기화합니다.
    /// </summary>
    public struct NetSnapshotMessage : NetworkMessage
    {
        public int PlayerIndex;

        public ArraySegment<byte> Payload;
    }

    /// <summary>
    /// 서버 -> 클라이언트 이벤트 메시지 타입입니다.
    /// 현재는 로깅 목적이므로 Log만 사용합니다.
    /// </summary>
    public enum MergeNetEventType : ushort
    {
        None = 0,
        PlayerAssigned = 1,
        ConnectedInfo = 2,
        GameStarted = 10,
        GameOver = 11,
        MapInitialized = 12,
        TowerSpawned = 20,
        TowerMerged = 21,
        TowerRemoved = 22,
        TowerAttacked = 23,
        EffectTriggered = 24,
        MonsterSpawned = 30,
        MonsterDamaged = 31,
        MonsterDied = 32,
        MonsterMoved = 33,
        MonsterInjected = 34,

        DifficultyStepChangedEvent = 40,

        ScoreChanged = 50,
        PlayerGoldChanged = 51,
    }

    /// <summary>
    /// 서버 -> 클라이언트 이벤트 메시지입니다.
    /// </summary>
    public struct NetEventMessage : NetworkMessage
    {
        public int PlayerIndex;
        public long Tick;
        public MergeNetEventType EventType;

        public ArraySegment<byte> Payload;
    }
}



