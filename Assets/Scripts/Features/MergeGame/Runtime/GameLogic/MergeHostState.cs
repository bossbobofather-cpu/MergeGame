using System;
using System.Collections.Generic;
using MyProject.MergeGame.Models;
using MyProject.MergeGame.Modules;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 게임 상태를 관리합니다.
    /// </summary>
    public sealed class MergeHostState : IDisposable
    {
        /// <summary>
        /// 슬롯 정보입니다.
        /// </summary>
        public sealed class SlotInfo
        {
            /// <summary>
            /// 슬롯 인덱스입니다.
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// 슬롯 위치입니다.
            /// </summary>
            public Point3D Position { get; }

            /// <summary>
            /// 타워 UID입니다. 비어있으면 0입니다.
            /// </summary>
            public long TowerUid { get; private set; }

            /// <summary>
            /// 타워 등급입니다.
            /// </summary>
            public int TowerGrade { get; private set; }

            /// <summary>
            /// 슬롯이 비어있는지 여부입니다.
            /// </summary>
            public bool IsEmpty => TowerUid == 0;

            public SlotInfo(int index, Point3D position)
            {
                Index = index;
                Position = position;
                TowerUid = 0;
                TowerGrade = 0;
            }
            /// <summary>
            /// Clear 메서드입니다.
            /// </summary>

            public void Clear()
            {
                TowerUid = 0;
                TowerGrade = 0;
            }
            /// <summary>
            /// SetTower 메서드입니다.
            /// </summary>

            public void SetTower(long towerUid, int grade)
            {
                TowerUid = towerUid;
                TowerGrade = grade;
            }
        }

        /// <summary>
        /// 플레이어별 게임 상태입니다.
        /// </summary>
        public sealed class PlayerState
        {
            public int PlayerIndex { get; }

            // 슬롯 및 타워
            public readonly List<SlotInfo> Slots = new();
            public readonly Dictionary<long, MergeTower> Towers = new();

            // 몬스터
            public readonly Dictionary<long, MergeMonster> Monsters = new();
            public readonly List<MonsterPath> Paths = new();

            // 플레이어 상태
            public int Gold;
            public bool IsGameOver;

            // 점수
            public int Score;
            public int MaxGrade;

            public PlayerState(int playerIndex)
            {
                PlayerIndex = playerIndex;
            }
            /// <summary>
            /// Clear 메서드입니다.
            /// </summary>

            public void Clear()
            {
                foreach (var slot in Slots) slot.Clear();
                Slots.Clear();
                
                foreach (var tower in Towers.Values) tower.Dispose();
                Towers.Clear();

                foreach (var monster in Monsters.Values) monster.Dispose();
                Monsters.Clear();

                Paths.Clear();
                Gold = 0;
                IsGameOver = false;
                Score = 0;
                MaxGrade = 0;
            }
        }

        // 세션 상태
        private MergeSessionPhase _sessionPhase = MergeSessionPhase.None;
        private float _elapsedTime;

        // 플레이어 상태 목록
        private PlayerState[] _players;

        // UID 생성기
        private long _nextTowerUid = 1;
        private long _nextMonsterUid = 1;

        #region Properties

        /// <summary>
        /// 현재 세션 단계입니다.
        /// </summary>
        public MergeSessionPhase SessionPhase => _sessionPhase;

        /// <summary>
        /// 경과 시간(초)입니다.
        /// </summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>
        /// 플레이어 수입니다.
        /// </summary>
        public int PlayerCount => _players?.Length ?? 0;

        /// <summary>
        /// 플레이어 상태를 가져옵니다.
        /// </summary>
        public PlayerState GetPlayerState(int playerIndex)
        {
            if (_players == null || playerIndex < 0 || playerIndex >= _players.Length)
                return null;
            return _players[playerIndex];
        }

        /// <summary>
        /// 슬롯 목록입니다. (특정 플레이어)
        /// </summary>
        public IReadOnlyList<SlotInfo> GetSlots(int playerIndex) => GetPlayerState(playerIndex)?.Slots;

        /// <summary>
        /// 타워 목록입니다. (특정 플레이어)
        /// </summary>
        public IReadOnlyDictionary<long, MergeTower> GetTowers(int playerIndex) => GetPlayerState(playerIndex)?.Towers;

        /// <summary>
        /// 몬스터 목록입니다. (특정 플레이어)
        /// </summary>
        public IReadOnlyDictionary<long, MergeMonster> GetMonsters(int playerIndex) => GetPlayerState(playerIndex)?.Monsters;
        /// <summary>
        /// 플레이어 골드입니다.
        /// </summary>
        public int GetPlayerGold(int playerIndex) => GetPlayerState(playerIndex)?.Gold ?? 0;

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int GetScore(int playerIndex) => GetPlayerState(playerIndex)?.Score ?? 0;

        /// <summary>
        /// 최고 등급입니다.
        /// </summary>
        public int GetMaxGrade(int playerIndex) => GetPlayerState(playerIndex)?.MaxGrade ?? 0;

        /// <summary>
        /// 사용 중인 슬롯 개수입니다.
        /// </summary>
        public int GetUsedSlotCount(int playerIndex)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return 0;

            var count = 0;
            for (var i = 0; i < player.Slots.Count; i++)
            {
                if (!player.Slots[i].IsEmpty)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 현재 살아있는 몬스터 수입니다.
        /// </summary>
        public int GetAliveMonsterCount(int playerIndex) => GetPlayerState(playerIndex)?.Monsters.Count ?? 0;

        /// <summary>
        /// 플레이어가 게임 오버 상태인지 확인합니다.
        /// </summary>
        public bool IsPlayerGameOver(int playerIndex) => GetPlayerState(playerIndex)?.IsGameOver ?? false;

        /// <summary>
        /// 플레이어의 게임 오버 상태를 설정합니다.
        /// </summary>
        public void SetPlayerGameOver(int playerIndex, bool isGameOver)
        {
            var player = GetPlayerState(playerIndex);
            if (player != null) player.IsGameOver = isGameOver;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 플레이어 수를 초기화합니다.
        /// </summary>
        public void InitializePlayers(int playerCount)
        {
            _players = new PlayerState[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                _players[i] = new PlayerState(i);
            }
        }

        /// <summary>
        /// MapModule의 슬롯 데이터를 기반으로 초기화합니다.
        /// </summary>
        public void InitializeSlots(int playerIndex, IReadOnlyList<MapSlot> mapSlots)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;

            player.Slots.Clear();
            for (var i = 0; i < mapSlots.Count; i++)
            {
                var mapSlot = mapSlots[i];
                player.Slots.Add(new SlotInfo(mapSlot.Index, mapSlot.Position));
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// 세션 단계를 변경합니다.
        /// </summary>
        public void SetSessionPhase(MergeSessionPhase phase)
        {
            _sessionPhase = phase;
        }

        /// <summary>
        /// 경과 시간을 누적합니다.
        /// </summary>
        public void AddElapsedTime(float deltaTime)
        {
            _elapsedTime += deltaTime;
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// 슬롯을 가져옵니다.
        /// </summary>
        public SlotInfo GetSlot(int playerIndex, int slotIndex)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return null;

            if (slotIndex < 0 || slotIndex >= player.Slots.Count)
            {
                return null;
            }
            return player.Slots[slotIndex];
        }

        /// <summary>
        /// 빈 슬롯 인덱스를 반환합니다. 없으면 -1입니다.
        /// </summary>
        public int FindEmptySlotIndex(int playerIndex)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return -1;

            for (var i = 0; i < player.Slots.Count; i++)
            {
                if (player.Slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Tower Management

        /// <summary>
        /// 새 타워 UID를 생성합니다.
        /// </summary>
        public long GenerateTowerUid()
        {
            return _nextTowerUid++;
        }

        /// <summary>
        /// 타워를 가져옵니다.
        /// </summary>
        public MergeTower GetTower(int playerIndex, long uid)
        {
            var player = GetPlayerState(playerIndex);
            return player != null && player.Towers.TryGetValue(uid, out var tower) ? tower : null;
        }

        /// <summary>
        /// 슬롯에 있는 타워를 가져옵니다.
        /// </summary>
        public MergeTower GetTowerAtSlot(int playerIndex, int slotIndex)
        {
            var slot = GetSlot(playerIndex, slotIndex);
            if (slot == null || slot.IsEmpty) return null;

            return GetTower(playerIndex, slot.TowerUid);
        }

        /// <summary>
        /// 타워를 제거합니다.
        /// </summary>
        public void RemoveTower(int playerIndex, long uid)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null || !player.Towers.TryGetValue(uid, out var tower)) return;

            var slot = GetSlot(playerIndex, tower.SlotIndex);
            if (slot != null)
            {
                slot.Clear();
            }

            tower.Dispose();
            player.Towers.Remove(uid);
        }

        /// <summary>
        /// 타워를 생성하고 반환합니다.
        /// </summary>
        public MergeTower CreateTower(
            int playerIndex,
            long towerId,
            int grade,
            int slotIndex,
            Point3D position,
            TowerAttackType attackType,
            ProjectileType projectileType,
            float projectileSpeed,
            float throwRadius,
            float trapDelay = 0f,
            List<GameplayEffect> onMergeSourceEffects = null,
            List<GameplayEffect> onMergeTargetEffects = null)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return null;

            var uid = GenerateTowerUid();
            var tower = new MergeTower(
                uid,
                towerId,
                grade,
                slotIndex,
                position,
                attackType,
                projectileType,
                projectileSpeed,
                throwRadius,
                trapDelay,
                onMergeSourceEffects,
                onMergeTargetEffects
            );

            player.Towers[uid] = tower;
            UpdateMaxGrade(playerIndex, grade);

            return tower;
        }

        /// <summary>
        /// 레거시 API 호환용 UID 생성입니다.
        /// </summary>
        public long GenerateUnitUid()
        {
            return GenerateTowerUid();
        }

        #endregion

        #region Monster Management

        /// <summary>
        /// 새 몬스터 UID를 생성합니다.
        /// </summary>
        public long GenerateMonsterUid()
        {
            return _nextMonsterUid++;
        }

        /// <summary>
        /// 몬스터를 가져옵니다.
        /// </summary>
        public MergeMonster GetMonster(int playerIndex, long uid)
        {
            var player = GetPlayerState(playerIndex);
            return player != null && player.Monsters.TryGetValue(uid, out var monster) ? monster : null;
        }

        /// <summary>
        /// 몬스터를 제거합니다.
        /// </summary>
        public void RemoveMonster(int playerIndex, long uid, bool countAsKilled = true)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null || !player.Monsters.TryGetValue(uid, out var monster)) return;

            monster.Dispose();
            player.Monsters.Remove(uid);

            // countAsKilled 파라미터는 향후 통계용으로 보존
        }

        /// <summary>
        /// 몬스터를 생성하고 반환합니다.
        /// </summary>
        public MergeMonster CreateMonster(
            int playerIndex,
            long monsterId,
            int pathIndex,
            Point3D startPosition,
            int damageToPlayer,
            int goldReward,
            bool isInjectedByOpponent = false)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return null;

            var uid = GenerateMonsterUid();
            var monster = new MergeMonster(
                uid,
                monsterId,
                pathIndex,
                startPosition,
                damageToPlayer,
                goldReward,
                isInjectedByOpponent
            );

            player.Monsters[uid] = monster;

            return monster;
        }

        /// <summary>
        /// 경로를 인덱스로 가져옵니다.
        /// </summary>
        public MonsterPath GetMonsterPath(int playerIndex, int pathIndex)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return null;

            if (pathIndex < 0 || pathIndex >= player.Paths.Count)
            {
                return null;
            }

            return player.Paths[pathIndex];
        }

        /// <summary>
        /// 경로를 인덱스로 설정합니다.
        /// </summary>
        public void SetMonsterPath(int playerIndex, int pathIndex, MonsterPath path)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;

            // 필요한 만큼 리스트를 확장
            while (player.Paths.Count <= pathIndex)
            {
                player.Paths.Add(null);
            }

            player.Paths[pathIndex] = path;
        }

        /// <summary>
        /// 모든 몬스터를 조회합니다.
        /// </summary>
        public IEnumerable<MergeMonster> GetAllMonsters(int playerIndex)
        {
            var player = GetPlayerState(playerIndex);
            return player != null ? player.Monsters.Values : new List<MergeMonster>();
        }

        /// <summary>
        /// 사망한 몬스터 목록을 반환합니다.
        /// </summary>
        public List<MergeMonster> GetDeadMonsters(int playerIndex)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return new List<MergeMonster>();

            var dead = new List<MergeMonster>();
            foreach (var monster in player.Monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    dead.Add(monster);
                }
            }
            return dead;
        }

        #endregion

        #region Player State

        /// <summary>
        /// 플레이어 골드를 설정합니다.
        /// </summary>
        public void SetPlayerGold(int playerIndex, int gold)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;
            player.Gold = gold;
        }

        /// <summary>
        /// 플레이어 골드를 증가시킵니다. (AddGold 별칭)
        /// </summary>
        public void AddPlayerGold(int playerIndex, int amount)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;
            player.Gold += amount;
        }

        #endregion

        #region Score

        /// <summary>
        /// 점수를 증가시킵니다.
        /// </summary>
        public void AddScore(int playerIndex, int amount)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;
            player.Score += amount;
        }

        /// <summary>
        /// 최고 등급을 갱신합니다.
        /// </summary>
        public void UpdateMaxGrade(int playerIndex, int grade)
        {
            var player = GetPlayerState(playerIndex);
            if (player == null) return;
            if (grade > player.MaxGrade)
            {
                player.MaxGrade = grade;
            }
        }

        #endregion

        #region Reset

        /// <summary>
        /// 상태를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            _sessionPhase = MergeSessionPhase.None;
            _elapsedTime = 0;

            if (_players != null)
            {
                foreach (var player in _players)
                {
                    player.Clear();
                }
            }

            // UID 초기화
            _nextTowerUid = 1;
            _nextMonsterUid = 1;
        }
        /// <summary>
        /// Dispose 메서드입니다.
        /// </summary>

        public void Dispose()
        {
            Reset();
        }

        #endregion
    }

    // WavePhase는 Shared/Enums/MergeEnums.cs에 정의됨
}

