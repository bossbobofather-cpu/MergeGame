using Noname.GameHost;

namespace MyProject.MergeGame.Commands
{
    /// <summary>
    /// MergeGame용 Command 기본 타입입니다.
    /// </summary>
    public abstract class MergeCommand : GameCommandBase
    {
        protected MergeCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// MergeGame용 Command 처리 결과 기본 타입입니다.
    /// </summary>
    public abstract class MergeCommandResult : GameCommandResultBase
    {
        protected MergeCommandResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
    }

    #region 게임 시작/종료

    /// <summary>
    /// 게임 시작 Command입니다.
    /// </summary>
    public sealed class StartMergeGameCommand : MergeCommand
    {
        public StartMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 시작 Result입니다.
    /// </summary>
    public sealed class StartMergeGameResult : MergeCommandResult
    {
        private StartMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static StartMergeGameResult Ok(long tick, long senderUid)
            => new StartMergeGameResult(tick, senderUid, true, null);

        public static StartMergeGameResult Fail(long tick, long senderUid, string reason)
            => new StartMergeGameResult(tick, senderUid, false, reason);
    }

    /// <summary>
    /// 게임 종료 Command입니다.
    /// </summary>
    public sealed class EndMergeGameCommand : MergeCommand
    {
        public EndMergeGameCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 종료 Result입니다.
    /// </summary>
    public sealed class EndMergeGameResult : MergeCommandResult
    {
        private EndMergeGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static EndMergeGameResult Ok(long tick, long senderUid)
            => new EndMergeGameResult(tick, senderUid, true, null);

        public static EndMergeGameResult Fail(long tick, long senderUid, string reason)
            => new EndMergeGameResult(tick, senderUid, false, reason);
    }

    #endregion

    #region 유닛 관리

    /// <summary>
    /// 유닛 스폰 Command입니다.
    /// </summary>
    public sealed class SpawnUnitCommand : MergeCommand
    {
        /// <summary>
        /// 스폰할 슬롯 인덱스입니다. -1이면 빈 슬롯에 자동 배치합니다.
        /// </summary>
        public int SlotIndex { get; }

        public SpawnUnitCommand(long senderUid, int slotIndex = -1) : base(senderUid)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 유닛 스폰 Result입니다.
    /// </summary>
    public sealed class SpawnUnitResult : MergeCommandResult
    {
        /// <summary>
        /// 생성된 유닛 UID입니다.
        /// </summary>
        public long UnitUid { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        private SpawnUnitResult(long tick, long senderUid, bool success, string reason, long unitUid, int slotIndex)
            : base(tick, senderUid, success, reason)
        {
            UnitUid = unitUid;
            SlotIndex = slotIndex;
        }

        public static SpawnUnitResult Ok(long tick, long senderUid, long unitUid, int slotIndex)
            => new SpawnUnitResult(tick, senderUid, true, null, unitUid, slotIndex);

        public static SpawnUnitResult Fail(long tick, long senderUid, string reason)
            => new SpawnUnitResult(tick, senderUid, false, reason, 0, -1);
    }

    /// <summary>
    /// 유닛 머지 Command입니다.
    /// </summary>
    public sealed class MergeUnitCommand : MergeCommand
    {
        /// <summary>
        /// 드래그 시작 슬롯 인덱스입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 드래그 종료 슬롯 인덱스입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        public MergeUnitCommand(long senderUid, int fromSlotIndex, int toSlotIndex) : base(senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }
    }

    /// <summary>
    /// 유닛 머지 Result입니다.
    /// </summary>
    public sealed class MergeUnitResult : MergeCommandResult
    {
        /// <summary>
        /// 머지 결과로 생성된 유닛 UID입니다.
        /// </summary>
        public long NewUnitUid { get; }

        /// <summary>
        /// 머지 결과 유닛의 등급입니다.
        /// </summary>
        public int NewGrade { get; }

        private MergeUnitResult(long tick, long senderUid, bool success, string reason, long newUnitUid, int newGrade)
            : base(tick, senderUid, success, reason)
        {
            NewUnitUid = newUnitUid;
            NewGrade = newGrade;
        }

        public static MergeUnitResult Ok(long tick, long senderUid, long newUnitUid, int newGrade)
            => new MergeUnitResult(tick, senderUid, true, null, newUnitUid, newGrade);

        public static MergeUnitResult Fail(long tick, long senderUid, string reason)
            => new MergeUnitResult(tick, senderUid, false, reason, 0, 0);
    }

    #endregion

    #region 캐릭터 관리

    /// <summary>
    /// 캐릭터 스폰 Command입니다.
    /// </summary>
    public sealed class SpawnCharacterCommand : MergeCommand
    {
        /// <summary>
        /// 스폰할 캐릭터 정의 ID입니다.
        /// </summary>
        public string CharacterId { get; }

        /// <summary>
        /// 스폰할 슬롯 인덱스입니다. -1이면 빈 슬롯에 자동 배치합니다.
        /// </summary>
        public int SlotIndex { get; }

        public SpawnCharacterCommand(long senderUid, string characterId, int slotIndex = -1) : base(senderUid)
        {
            CharacterId = characterId;
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 캐릭터 스폰 Result입니다.
    /// </summary>
    public sealed class SpawnCharacterResult : MergeCommandResult
    {
        /// <summary>
        /// 생성된 캐릭터 UID입니다.
        /// </summary>
        public long CharacterUid { get; }

        /// <summary>
        /// 캐릭터 정의 ID입니다.
        /// </summary>
        public string CharacterId { get; }

        /// <summary>
        /// 캐릭터 타입입니다.
        /// </summary>
        public string CharacterType { get; }

        /// <summary>
        /// 캐릭터 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        private SpawnCharacterResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            long characterUid,
            string characterId,
            string characterType,
            int grade,
            int slotIndex)
            : base(tick, senderUid, success, reason)
        {
            CharacterUid = characterUid;
            CharacterId = characterId;
            CharacterType = characterType;
            Grade = grade;
            SlotIndex = slotIndex;
        }

        public static SpawnCharacterResult Ok(
            long tick,
            long senderUid,
            long characterUid,
            string characterId,
            string characterType,
            int grade,
            int slotIndex)
            => new SpawnCharacterResult(tick, senderUid, true, null, characterUid, characterId, characterType, grade, slotIndex);

        public static SpawnCharacterResult Fail(long tick, long senderUid, string reason)
            => new SpawnCharacterResult(tick, senderUid, false, reason, 0, null, null, 0, -1);
    }

    /// <summary>
    /// 캐릭터 머지 Command입니다.
    /// </summary>
    public sealed class MergeCharacterCommand : MergeCommand
    {
        /// <summary>
        /// 드래그 시작 슬롯 인덱스 (소스 캐릭터)입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 드래그 종료 슬롯 인덱스 (타겟 캐릭터)입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        public MergeCharacterCommand(long senderUid, int fromSlotIndex, int toSlotIndex) : base(senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }
    }

    /// <summary>
    /// 캐릭터 머지 Result입니다.
    /// </summary>
    public sealed class MergeCharacterResult : MergeCommandResult
    {
        /// <summary>
        /// 소스 캐릭터 UID (흡수된 캐릭터)입니다.
        /// </summary>
        public long SourceCharacterUid { get; }

        /// <summary>
        /// 타겟 캐릭터 UID (남는 캐릭터)입니다.
        /// </summary>
        public long TargetCharacterUid { get; }

        /// <summary>
        /// 머지 결과로 생성된 캐릭터 UID입니다.
        /// </summary>
        public long ResultCharacterUid { get; }

        /// <summary>
        /// 결과 캐릭터의 정의 ID입니다.
        /// </summary>
        public string ResultCharacterId { get; }

        /// <summary>
        /// 결과 캐릭터의 타입입니다.
        /// </summary>
        public string ResultCharacterType { get; }

        /// <summary>
        /// 결과 캐릭터의 등급입니다.
        /// </summary>
        public int ResultGrade { get; }

        /// <summary>
        /// 결과 캐릭터가 배치된 슬롯 인덱스입니다.
        /// </summary>
        public int SlotIndex { get; }

        private MergeCharacterResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            long sourceCharacterUid,
            long targetCharacterUid,
            long resultCharacterUid,
            string resultCharacterId,
            string resultCharacterType,
            int resultGrade,
            int slotIndex)
            : base(tick, senderUid, success, reason)
        {
            SourceCharacterUid = sourceCharacterUid;
            TargetCharacterUid = targetCharacterUid;
            ResultCharacterUid = resultCharacterUid;
            ResultCharacterId = resultCharacterId;
            ResultCharacterType = resultCharacterType;
            ResultGrade = resultGrade;
            SlotIndex = slotIndex;
        }

        public static MergeCharacterResult Ok(
            long tick,
            long senderUid,
            long sourceCharacterUid,
            long targetCharacterUid,
            long resultCharacterUid,
            string resultCharacterId,
            string resultCharacterType,
            int resultGrade,
            int slotIndex)
            => new MergeCharacterResult(
                tick, senderUid, true, null,
                sourceCharacterUid, targetCharacterUid, resultCharacterUid,
                resultCharacterId, resultCharacterType, resultGrade, slotIndex);

        public static MergeCharacterResult Fail(long tick, long senderUid, string reason)
            => new MergeCharacterResult(tick, senderUid, false, reason, 0, 0, 0, null, null, 0, -1);
    }

    /// <summary>
    /// 캐릭터 이동 Command입니다.
    /// </summary>
    public sealed class MoveCharacterCommand : MergeCommand
    {
        /// <summary>
        /// 이동할 캐릭터가 있는 슬롯 인덱스입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 이동 목표 슬롯 인덱스입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        public MoveCharacterCommand(long senderUid, int fromSlotIndex, int toSlotIndex) : base(senderUid)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }
    }

    /// <summary>
    /// 캐릭터 이동 Result입니다.
    /// </summary>
    public sealed class MoveCharacterResult : MergeCommandResult
    {
        /// <summary>
        /// 이동한 캐릭터 UID입니다.
        /// </summary>
        public long CharacterUid { get; }

        /// <summary>
        /// 이동 전 슬롯 인덱스입니다.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// 이동 후 슬롯 인덱스입니다.
        /// </summary>
        public int ToSlotIndex { get; }

        private MoveCharacterResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            long characterUid,
            int fromSlotIndex,
            int toSlotIndex)
            : base(tick, senderUid, success, reason)
        {
            CharacterUid = characterUid;
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }

        public static MoveCharacterResult Ok(long tick, long senderUid, long characterUid, int fromSlotIndex, int toSlotIndex)
            => new MoveCharacterResult(tick, senderUid, true, null, characterUid, fromSlotIndex, toSlotIndex);

        public static MoveCharacterResult Fail(long tick, long senderUid, string reason)
            => new MoveCharacterResult(tick, senderUid, false, reason, 0, -1, -1);
    }

    #endregion

    #region 웨이브 관리

    /// <summary>
    /// 웨이브 시작 Command입니다.
    /// </summary>
    public sealed class StartWaveCommand : MergeCommand
    {
        /// <summary>
        /// 시작할 웨이브 번호입니다. -1이면 다음 웨이브를 시작합니다.
        /// </summary>
        public int WaveNumber { get; }

        public StartWaveCommand(long senderUid, int waveNumber = -1) : base(senderUid)
        {
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 웨이브 시작 Result입니다.
    /// </summary>
    public sealed class StartWaveResult : MergeCommandResult
    {
        /// <summary>
        /// 시작된 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// 이 웨이브의 총 몬스터 수입니다.
        /// </summary>
        public int TotalMonsterCount { get; }

        private StartWaveResult(
            long tick,
            long senderUid,
            bool success,
            string reason,
            int waveNumber,
            int totalMonsterCount)
            : base(tick, senderUid, success, reason)
        {
            WaveNumber = waveNumber;
            TotalMonsterCount = totalMonsterCount;
        }

        public static StartWaveResult Ok(long tick, long senderUid, int waveNumber, int totalMonsterCount)
            => new StartWaveResult(tick, senderUid, true, null, waveNumber, totalMonsterCount);

        public static StartWaveResult Fail(long tick, long senderUid, string reason)
            => new StartWaveResult(tick, senderUid, false, reason, 0, 0);
    }

    #endregion

    #region 멀티플레이(상대 공격)

    /// <summary>
    /// 상대(다른 세션/플레이어)로부터 몬스터를 주입(추가 스폰)하는 Command입니다.
    /// 서버/MatchHost가 상대 공격(garbage) 처리를 위해 사용합니다.
    /// </summary>
    public sealed class InjectMonstersCommand : MergeCommand
    {
        /// <summary>
        /// 주입할 몬스터 정의 ID입니다. null/빈 값이면 기본 몬스터를 사용합니다.
        /// </summary>
        public string MonsterId { get; }

        /// <summary>
        /// 주입할 몬스터 수입니다.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 스폰할 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; }

        public InjectMonstersCommand(long senderUid, string monsterId, int count, int pathIndex = 0) : base(senderUid)
        {
            MonsterId = monsterId;
            Count = count;
            PathIndex = pathIndex;
        }
    }

    /// <summary>
    /// 몬스터 주입 Result입니다.
    /// </summary>
    public sealed class InjectMonstersResult : MergeCommandResult
    {
        /// <summary>
        /// 실제로 스폰된 몬스터 수입니다.
        /// </summary>
        public int SpawnedCount { get; }

        private InjectMonstersResult(long tick, long senderUid, bool success, string reason, int spawnedCount)
            : base(tick, senderUid, success, reason)
        {
            SpawnedCount = spawnedCount;
        }

        public static InjectMonstersResult Ok(long tick, long senderUid, int spawnedCount)
            => new InjectMonstersResult(tick, senderUid, true, null, spawnedCount);

        public static InjectMonstersResult Fail(long tick, long senderUid, string reason)
            => new InjectMonstersResult(tick, senderUid, false, reason, 0);
    }

    #endregion
}
