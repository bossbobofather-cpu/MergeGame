using System.Collections.Generic;
using MyProject.MergeGame;
using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 웨이브 모듈입니다.
    /// 웨이브 스폰 및 라이프사이클을 관리합니다.
    /// </summary>
    public sealed class WaveModule : HostModuleBase<WaveModuleConfig>
    {
        public const string MODULE_ID = "wave";

        private int _currentWaveNumber;
        private WavePhase _currentPhase = WavePhase.Idle;
        private float _phaseTimer;
        private float _spawnTimer;
        private int _spawnedCount;
        private int _totalToSpawn;
        private int _remainingMonsters;
        private int _currentSpawnIndex;
        private WaveInfo _currentWaveInfo;
        private readonly List<string> _pendingSpawns = new();

        /// <inheritdoc />
        public override string ModuleId => MODULE_ID;

        /// <inheritdoc />
        public override bool IsRequired => false; // 옵션 모듈

        /// <inheritdoc />
        public override int Priority => 80;

        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
        public int CurrentWaveNumber => _currentWaveNumber;

        /// <summary>
        /// 현재 웨이브 페이즈입니다.
        /// </summary>
        public WavePhase CurrentPhase => _currentPhase;

        /// <summary>
        /// 웨이브 진행 중 여부입니다.
        /// </summary>
        public bool IsWaveInProgress => _currentPhase == WavePhase.Spawning || _currentPhase == WavePhase.InProgress;

        /// <summary>
        /// 스폰된 몬스터 수입니다.
        /// </summary>
        public int SpawnedCount => _spawnedCount;

        /// <summary>
        /// 남은 몬스터 수입니다.
        /// </summary>
        public int RemainingMonsters => _remainingMonsters;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // 내부 이벤트 구독 (모듈 전용)
            SubscribeInnerEvent<WaveStartRequestInnerEvent>(OnWaveStartRequest);
            SubscribeInnerEvent<MonsterDiedInnerEvent>(OnMonsterDied);
            SubscribeInnerEvent<WaveInfoRequestInnerEvent>(OnWaveInfoRequest);
            SubscribeInnerEvent<WaveMonsterCountRequestInnerEvent>(OnWaveMonsterCountRequest);

            // 공유 내부 이벤트 구독 (다른 모듈에서 호출)
            SubscribeInnerEvent<GetWaveStatusRequest>(OnGetWaveStatusRequest);
            SubscribeInnerEvent<RequestWaveStart>(OnRequestWaveStart);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            // 내부 이벤트 구독 해제 (모듈 전용)
            UnsubscribeInnerEvent<WaveStartRequestInnerEvent>(OnWaveStartRequest);
            UnsubscribeInnerEvent<MonsterDiedInnerEvent>(OnMonsterDied);
            UnsubscribeInnerEvent<WaveInfoRequestInnerEvent>(OnWaveInfoRequest);
            UnsubscribeInnerEvent<WaveMonsterCountRequestInnerEvent>(OnWaveMonsterCountRequest);

            // 공유 내부 이벤트 구독 해제
            UnsubscribeInnerEvent<GetWaveStatusRequest>(OnGetWaveStatusRequest);
            UnsubscribeInnerEvent<RequestWaveStart>(OnRequestWaveStart);
        }

        /// <inheritdoc />
        protected override void OnTick(long tick, float deltaTime)
        {
            switch (_currentPhase)
            {
                case WavePhase.Idle:
                    ProcessIdlePhase(tick, deltaTime);
                    break;

                case WavePhase.Spawning:
                    ProcessSpawningPhase(tick, deltaTime);
                    break;

                case WavePhase.InProgress:
                    ProcessInProgressPhase(tick, deltaTime);
                    break;

                case WavePhase.Completed:
                    ProcessCompletedPhase(tick, deltaTime);
                    break;
            }
        }

        #region 웨이브 제어

        /// <summary>
        /// 웨이브를 시작합니다.
        /// </summary>
        public bool StartWave(long tick)
        {
            if (_currentPhase != WavePhase.Idle && _currentPhase != WavePhase.Completed)
            {
                return false;
            }

            if (Config.MaxWaveCount > 0 && _currentWaveNumber >= Config.MaxWaveCount)
            {
                return false;
            }

            _currentWaveNumber++;
            _currentWaveInfo = GetWaveInfo(_currentWaveNumber);
            _spawnedCount = 0;
            _currentSpawnIndex = 0;
            _totalToSpawn = _currentWaveInfo.MonsterIds.Count;
            _remainingMonsters = _totalToSpawn;
            _pendingSpawns.Clear();
            _pendingSpawns.AddRange(_currentWaveInfo.MonsterIds);

            SetPhase(tick, WavePhase.Spawning);
            _spawnTimer = 0f;

            return true;
        }

        /// <summary>
        /// 웨이브를 강제 종료합니다.
        /// </summary>
        public void ForceEndWave(long tick)
        {
            if (_currentPhase == WavePhase.Idle)
            {
                return;
            }

            _remainingMonsters = 0;
            SetPhase(tick, WavePhase.Completed);
        }

        /// <summary>
        /// 웨이브를 리셋합니다.
        /// </summary>
        public void Reset(long tick)
        {
            _currentWaveNumber = 0;
            _spawnedCount = 0;
            _totalToSpawn = 0;
            _remainingMonsters = 0;
            _currentSpawnIndex = 0;
            _currentWaveInfo = null;
            _pendingSpawns.Clear();

            SetPhase(tick, WavePhase.Idle);
        }

        #endregion

        #region 페이즈 처리

        private void ProcessIdlePhase(long tick, float deltaTime)
        {
            if (!Config.AutoStartWaves)
            {
                return;
            }

            if (_currentWaveNumber == 0)
            {
                _phaseTimer += deltaTime;
                if (_phaseTimer >= Config.WaveStartDelay)
                {
                    StartWave(tick);
                }
            }
        }

        private void ProcessSpawningPhase(long tick, float deltaTime)
        {
            _spawnTimer += deltaTime;

            var spawnInterval = _currentWaveInfo?.SpawnInterval ?? Config.DefaultSpawnInterval;

            while (_spawnTimer >= spawnInterval && _currentSpawnIndex < _pendingSpawns.Count)
            {
                _spawnTimer -= spawnInterval;
                SpawnMonster(tick, _pendingSpawns[_currentSpawnIndex]);
                _currentSpawnIndex++;
                _spawnedCount++;
            }

            // 모든 몬스터 스폰 완료
            if (_currentSpawnIndex >= _pendingSpawns.Count)
            {
                SetPhase(tick, WavePhase.InProgress);
            }
        }

        private void ProcessInProgressPhase(long tick, float deltaTime)
        {
            // 남은 몬스터가 없으면 웨이브 완료
            if (_remainingMonsters <= 0)
            {
                SetPhase(tick, WavePhase.Completed);
            }
        }

        private void ProcessCompletedPhase(long tick, float deltaTime)
        {
            if (!Config.AutoStartWaves)
            {
                return;
            }

            _phaseTimer += deltaTime;
            if (_phaseTimer >= Config.WaveIntervalDelay)
            {
                // 최대 웨이브 수 체크
                if (Config.MaxWaveCount <= 0 || _currentWaveNumber < Config.MaxWaveCount)
                {
                    StartWave(tick);
                }
            }
        }

        private void SetPhase(long tick, WavePhase newPhase)
        {
            if (_currentPhase == newPhase)
            {
                return;
            }

            _currentPhase = newPhase;
            _phaseTimer = 0f;

            // 상태 변경 내부 이벤트 발행
            PublishInnerEvent(new WaveStateChangedInnerEvent(tick, _currentWaveNumber, _currentPhase));
        }

        #endregion

        #region 웨이브 정보

        private WaveInfo GetWaveInfo(int waveNumber)
        {
            // 사전 정의된 웨이브가 있으면 사용
            if (Config.PredefinedWaves != null && waveNumber <= Config.PredefinedWaves.Count)
            {
                return Config.PredefinedWaves[waveNumber - 1];
            }

            // 없으면 자동 생성
            return GenerateWaveInfo(waveNumber);
        }

        private WaveInfo GenerateWaveInfo(int waveNumber)
        {
            var monsterCount = Config.BaseMonsterCount + (waveNumber - 1) * Config.MonstersPerWaveIncrease;

            var waveInfo = new WaveInfo
            {
                WaveNumber = waveNumber,
                SpawnInterval = Config.DefaultSpawnInterval,
                PathIndex = 0
            };

            // 기본 몬스터 ID로 채우기 (실제 몬스터 ID는 Host에서 결정)
            for (var i = 0; i < monsterCount; i++)
            {
                waveInfo.MonsterIds.Add("monster_default");
            }

            return waveInfo;
        }

        private void SpawnMonster(long tick, string monsterId)
        {
            var pathIndex = _currentWaveInfo?.PathIndex ?? 0;

            // 몬스터 스폰 요청 이벤트 발행 (Host에서 실제 생성 담당)
            PublishInnerEvent(new MonsterSpawnRequestInnerEvent(
                tick,
                monsterId,
                pathIndex,
                _currentWaveNumber
            ));
        }

        #endregion

        #region 내부 이벤트 핸들러

        private void OnWaveStartRequest(WaveStartRequestInnerEvent evt)
        {
            if (_currentPhase != WavePhase.Idle && _currentPhase != WavePhase.Completed)
            {
                evt.CanStart = false;
                evt.FailReason = "웨이브가 진행 중입니다.";
                return;
            }

            if (Config.MaxWaveCount > 0 && _currentWaveNumber >= Config.MaxWaveCount)
            {
                evt.CanStart = false;
                evt.FailReason = "최대 웨이브에 도달했습니다.";
                return;
            }

            evt.CanStart = true;
        }

        private void OnMonsterDied(MonsterDiedInnerEvent evt)
        {
            if (evt.WaveNumber == _currentWaveNumber && _remainingMonsters > 0)
            {
                _remainingMonsters--;
            }
        }

        private void OnWaveInfoRequest(WaveInfoRequestInnerEvent evt)
        {
            evt.WaveInfo = GetWaveInfo(evt.WaveNumber);
        }

        private void OnWaveMonsterCountRequest(WaveMonsterCountRequestInnerEvent evt)
        {
            evt.TotalSpawned = _spawnedCount;
            evt.RemainingCount = _remainingMonsters;
        }

        #endregion

        #region 공유 내부 이벤트 핸들러

        private void OnGetWaveStatusRequest(GetWaveStatusRequest evt)
        {
            evt.CurrentWaveNumber = _currentWaveNumber;
            evt.Phase = _currentPhase;
            evt.TotalMonsters = _totalToSpawn;
            evt.RemainingMonsters = _remainingMonsters;
        }

        private void OnRequestWaveStart(RequestWaveStart evt)
        {
            if (_currentPhase != WavePhase.Idle && _currentPhase != WavePhase.Completed)
            {
                evt.Success = false;
                evt.FailReason = "웨이브가 진행 중입니다.";
                return;
            }

            if (Config.MaxWaveCount > 0 && _currentWaveNumber >= Config.MaxWaveCount)
            {
                evt.Success = false;
                evt.FailReason = "최대 웨이브에 도달했습니다.";
                return;
            }

            evt.Success = StartWave(Context.CurrentTick);
        }

        #endregion
    }
}
