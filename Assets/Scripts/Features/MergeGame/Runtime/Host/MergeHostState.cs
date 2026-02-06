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

            public void Clear()
            {
                TowerUid = 0;
                TowerGrade = 0;
            }

            public void SetTower(long towerUid, int grade)
            {
                TowerUid = towerUid;
                TowerGrade = grade;
            }

            /// <summary>
            /// 레거시 API 호환용입니다.
            /// </summary>
            public void SetUnit(long unitUid, int grade)
            {
                SetTower(unitUid, grade);
            }
        }

        // 세션 상태
        private MergeSessionPhase _sessionPhase = MergeSessionPhase.None;
        private float _elapsedTime;

        // 슬롯 및 타워
        private readonly List<SlotInfo> _slots = new();
        private readonly Dictionary<long, MergeTower> _towers = new();

        // 몬스터
        private readonly Dictionary<long, MergeMonster> _monsters = new();
        private readonly List<MonsterPath> _paths = new();

        // 플레이어 상태
        private int _playerHp;
        private int _playerMaxHp;
        private int _playerGold;

        // 웨이브 상태
        private int _currentWaveNumber;
        private WavePhase _wavePhase = WavePhase.Idle;
        private float _waveTimer;
        private int _monstersRemainingInWave;
        private int _monstersKilledInWave;

        // 점수
        private int _score;
        private int _maxGrade;

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
        /// 슬롯 목록입니다.
        /// </summary>
        public IReadOnlyList<SlotInfo> Slots => _slots;

        /// <summary>
        /// 타워 목록입니다.
        /// </summary>
        public IReadOnlyDictionary<long, MergeTower> Towers => _towers;

        /// <summary>
        /// 몬스터 목록입니다.
        /// </summary>
        public IReadOnlyDictionary<long, MergeMonster> Monsters => _monsters;

        /// <summary>
        /// 경로 목록입니다.
        /// </summary>
        public IReadOnlyList<MonsterPath> Paths => _paths;

        /// <summary>
        /// 플레이어 HP입니다.
        /// </summary>
        public int PlayerHp => _playerHp;

        /// <summary>
        /// 플레이어 최대 HP입니다.
        /// </summary>
        public int PlayerMaxHp => _playerMaxHp;

        /// <summary>
        /// 플레이어 골드입니다.
        /// </summary>
        public int PlayerGold => _playerGold;

        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
        public int CurrentWaveNumber => _currentWaveNumber;

        /// <summary>
        /// 현재 웨이브 단계입니다.
        /// </summary>
        public WavePhase CurrentWavePhase => _wavePhase;

        /// <summary>
        /// 웨이브 타이머입니다.
        /// </summary>
        public float WaveTimer => _waveTimer;

        /// <summary>
        /// 웨이브에 남은 몬스터 수입니다.
        /// </summary>
        public int MonstersRemainingInWave => _monstersRemainingInWave;

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int Score => _score;

        /// <summary>
        /// 최고 등급입니다.
        /// </summary>
        public int MaxGrade => _maxGrade;

        /// <summary>
        /// 사용 중인 슬롯 개수입니다.
        /// </summary>
        public int UsedSlotCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _slots.Count; i++)
                {
                    if (!_slots[i].IsEmpty)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// 현재 살아있는 몬스터 수입니다.
        /// </summary>
        public int AliveMonsterCount => _monsters.Count;

        #endregion

        #region Initialization

        /// <summary>
        /// MapModule의 슬롯 데이터를 기반으로 초기화합니다.
        /// </summary>
        public void InitializeSlots(IReadOnlyList<MapSlot> mapSlots)
        {
            _slots.Clear();
            for (var i = 0; i < mapSlots.Count; i++)
            {
                var mapSlot = mapSlots[i];
                _slots.Add(new SlotInfo(mapSlot.Index, mapSlot.Position));
            }
        }

        /// <summary>
        /// 플레이어 상태를 초기화합니다.
        /// </summary>
        public void InitializePlayer(int maxHp, int startGold)
        {
            _playerMaxHp = maxHp;
            _playerHp = maxHp;
            _playerGold = startGold;
        }

        /// <summary>
        /// 경로를 추가합니다.
        /// </summary>
        public void AddPath(MonsterPath path)
        {
            _paths.Add(path);
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
        public SlotInfo GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return null;
            }
            return _slots[index];
        }

        /// <summary>
        /// 빈 슬롯 인덱스를 반환합니다. 없으면 -1입니다.
        /// </summary>
        public int FindEmptySlotIndex()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
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
        /// 타워를 추가합니다.
        /// </summary>
        public void AddTower(MergeTower tower)
        {
            if (tower == null) return;

            _towers[tower.Uid] = tower;

            var slot = GetSlot(tower.SlotIndex);
            if (slot != null)
            {
                slot.SetTower(tower.Uid, tower.Grade);
            }

            UpdateMaxGrade(tower.Grade);
        }

        /// <summary>
        /// 타워를 가져옵니다.
        /// </summary>
        public MergeTower GetTower(long uid)
        {
            return _towers.TryGetValue(uid, out var tower) ? tower : null;
        }

        /// <summary>
        /// 슬롯에 있는 타워를 가져옵니다.
        /// </summary>
        public MergeTower GetTowerAtSlot(int slotIndex)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty) return null;

            return GetTower(slot.TowerUid);
        }

        /// <summary>
        /// 타워를 제거합니다.
        /// </summary>
        public void RemoveTower(long uid)
        {
            if (!_towers.TryGetValue(uid, out var tower)) return;

            var slot = GetSlot(tower.SlotIndex);
            if (slot != null)
            {
                slot.Clear();
            }

            tower.Dispose();
            _towers.Remove(uid);
        }

        /// <summary>
        /// 타워를 생성하고 반환합니다.
        /// </summary>
        public MergeTower CreateTower(
            string towerId,
            string towerType,
            int grade,
            int slotIndex,
            Point3D position,
            TowerAttackType attackType,
            ProjectileType projectileType,
            float projectileSpeed,
            float throwRadius,
            string onMergeSourceEffectId = null,
            string onMergeTargetEffectId = null)
        {
            var uid = GenerateTowerUid();
            var tower = new MergeTower(
                uid,
                towerId,
                towerType,
                grade,
                slotIndex,
                position,
                attackType,
                projectileType,
                projectileSpeed,
                throwRadius,
                onMergeSourceEffectId,
                onMergeTargetEffectId
            );

            _towers[uid] = tower;
            UpdateMaxGrade(grade);

            return tower;
        }

        /// <summary>
        /// 슬롯 인덱스로 타워를 가져옵니다.
        /// </summary>
        public MergeTower GetTowerBySlot(int slotIndex)
        {
            return GetTowerAtSlot(slotIndex);
        }

        /// <summary>
        /// 타워를 다른 슬롯으로 이동합니다.
        /// </summary>
        public bool MoveTower(long uid, int newSlotIndex)
        {
            if (!_towers.TryGetValue(uid, out var tower)) return false;

            var newSlot = GetSlot(newSlotIndex);
            if (newSlot == null || !newSlot.IsEmpty) return false;

            var oldSlot = GetSlot(tower.SlotIndex);
            if (oldSlot != null)
            {
                oldSlot.Clear();
            }

            tower.SlotIndex = newSlotIndex;
            tower.Position = newSlot.Position;
            newSlot.SetTower(uid, tower.Grade);

            return true;
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
        /// 몬스터를 추가합니다.
        /// </summary>
        public void AddMonster(MergeMonster monster)
        {
            if (monster == null) return;

            _monsters[monster.Uid] = monster;
            _monstersRemainingInWave++;
        }

        /// <summary>
        /// 몬스터를 가져옵니다.
        /// </summary>
        public MergeMonster GetMonster(long uid)
        {
            return _monsters.TryGetValue(uid, out var monster) ? monster : null;
        }

        /// <summary>
        /// 몬스터를 제거합니다.
        /// </summary>
        public void RemoveMonster(long uid, bool countAsKilled = true)
        {
            if (!_monsters.TryGetValue(uid, out var monster)) return;

            monster.Dispose();
            _monsters.Remove(uid);

            if (countAsKilled)
            {
                _monstersKilledInWave++;
            }
            _monstersRemainingInWave = Math.Max(0, _monstersRemainingInWave - 1);
        }

        /// <summary>
        /// 몬스터를 생성하고 반환합니다.
        /// </summary>
        public MergeMonster CreateMonster(
            string monsterId,
            int pathIndex,
            Point3D startPosition,
            int damageToPlayer,
            int goldReward)
        {
            var uid = GenerateMonsterUid();
            var monster = new MergeMonster(
                uid,
                monsterId,
                pathIndex,
                startPosition,
                damageToPlayer,
                goldReward
            );

            _monsters[uid] = monster;
            _monstersRemainingInWave++;

            return monster;
        }

        /// <summary>
        /// 경로를 인덱스로 가져옵니다.
        /// </summary>
        public MonsterPath GetMonsterPath(int pathIndex)
        {
            if (pathIndex < 0 || pathIndex >= _paths.Count)
            {
                return null;
            }
            return _paths[pathIndex];
        }

        /// <summary>
        /// 경로를 인덱스로 설정합니다.
        /// </summary>
        public void SetMonsterPath(int pathIndex, MonsterPath path)
        {
            // 필요한 만큼 리스트를 확장
            while (_paths.Count <= pathIndex)
            {
                _paths.Add(null);
            }
            _paths[pathIndex] = path;
        }

        /// <summary>
        /// 모든 몬스터를 조회합니다.
        /// </summary>
        public IEnumerable<MergeMonster> GetAllMonsters()
        {
            return _monsters.Values;
        }

        /// <summary>
        /// 사망한 몬스터 목록을 반환합니다.
        /// </summary>
        public List<MergeMonster> GetDeadMonsters()
        {
            var dead = new List<MergeMonster>();
            foreach (var monster in _monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    dead.Add(monster);
                }
            }
            return dead;
        }

        /// <summary>
        /// 목표에 도달한 몬스터 목록을 반환합니다.
        /// </summary>
        public List<MergeMonster> GetGoalReachedMonsters()
        {
            var reached = new List<MergeMonster>();
            foreach (var monster in _monsters.Values)
            {
                if (monster.ReachedGoal)
                {
                    reached.Add(monster);
                }
            }
            return reached;
        }

        #endregion

        #region Player State

        /// <summary>
        /// 플레이어 HP를 설정합니다.
        /// </summary>
        public void SetPlayerHp(int hp, int maxHp)
        {
            _playerHp = hp;
            _playerMaxHp = maxHp;
        }

        /// <summary>
        /// 플레이어 골드를 설정합니다.
        /// </summary>
        public void SetPlayerGold(int gold)
        {
            _playerGold = gold;
        }

        /// <summary>
        /// 플레이어 골드를 증가시킵니다. (AddGold 별칭)
        /// </summary>
        public void AddPlayerGold(int amount)
        {
            _playerGold += amount;
        }

        /// <summary>
        /// 플레이어가 데미지를 받습니다.
        /// </summary>
        public int DamagePlayer(int damage)
        {
            var actualDamage = Math.Min(damage, _playerHp);
            _playerHp -= actualDamage;
            return actualDamage;
        }

        /// <summary>
        /// 플레이어 HP를 회복합니다.
        /// </summary>
        public int HealPlayer(int amount)
        {
            var actualHeal = Math.Min(amount, _playerMaxHp - _playerHp);
            _playerHp += actualHeal;
            return actualHeal;
        }

        /// <summary>
        /// 골드를 증가시킵니다.
        /// </summary>
        public void AddGold(int amount)
        {
            _playerGold += amount;
        }

        /// <summary>
        /// 골드를 소모합니다.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (_playerGold < amount) return false;

            _playerGold -= amount;
            return true;
        }

        /// <summary>
        /// 플레이어 생존 여부입니다.
        /// </summary>
        public bool IsPlayerAlive => _playerHp > 0;

        #endregion

        #region Wave Management

        /// <summary>
        /// 웨이브를 시작합니다.
        /// </summary>
        public void StartWave(int waveNumber, int totalMonsters)
        {
            _currentWaveNumber = waveNumber;
            _wavePhase = WavePhase.Spawning;
            _waveTimer = 0f;
            _monstersRemainingInWave = 0;
            _monstersKilledInWave = 0;
        }

        /// <summary>
        /// 현재 웨이브 번호를 설정합니다.
        /// </summary>
        public void SetCurrentWaveNumber(int waveNumber)
        {
            _currentWaveNumber = waveNumber;
        }

        /// <summary>
        /// 웨이브 단계를 설정합니다.
        /// </summary>
        public void SetWavePhase(WavePhase phase)
        {
            _wavePhase = phase;
        }

        /// <summary>
        /// 웨이브 타이머를 업데이트합니다.
        /// </summary>
        public void UpdateWaveTimer(float deltaTime)
        {
            _waveTimer += deltaTime;
        }

        /// <summary>
        /// 웨이브를 완료 처리합니다.
        /// </summary>
        public void CompleteWave()
        {
            _wavePhase = WavePhase.Completed;
        }

        /// <summary>
        /// 웨이브 클리어 여부입니다.
        /// </summary>
        public bool IsWaveClear => _wavePhase == WavePhase.InProgress && _monsters.Count == 0;

        #endregion

        #region Score

        /// <summary>
        /// 점수를 증가시킵니다.
        /// </summary>
        public void AddScore(int amount)
        {
            _score += amount;
        }

        /// <summary>
        /// 최고 등급을 갱신합니다.
        /// </summary>
        public void UpdateMaxGrade(int grade)
        {
            if (grade > _maxGrade)
            {
                _maxGrade = grade;
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

            // 슬롯 초기화
            foreach (var slot in _slots)
            {
                slot.Clear();
            }
            _slots.Clear();

            // 타워 정리
            foreach (var tower in _towers.Values)
            {
                tower.Dispose();
            }
            _towers.Clear();

            // 몬스터 정리
            foreach (var monster in _monsters.Values)
            {
                monster.Dispose();
            }
            _monsters.Clear();

            // 경로 정리
            _paths.Clear();

            // 플레이어 상태 초기화
            _playerHp = 0;
            _playerMaxHp = 0;
            _playerGold = 0;

            // 웨이브 상태 초기화
            _currentWaveNumber = 0;
            _wavePhase = WavePhase.Idle;
            _waveTimer = 0;
            _monstersRemainingInWave = 0;
            _monstersKilledInWave = 0;

            // 점수 초기화
            _score = 0;
            _maxGrade = 0;

            // UID 초기화
            _nextTowerUid = 1;
            _nextMonsterUid = 1;
        }

        public void Dispose()
        {
            Reset();
        }

        #endregion
    }

    // WavePhase는 Shared/Enums/MergeEnums.cs에 정의됨
}
