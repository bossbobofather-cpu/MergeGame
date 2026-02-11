using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 난이도 모듈입니다.
    /// 게임 시작부터 연속으로 몬스터를 스폰하며,
    /// 10초마다 난이도 스텝이 순환 적용됩니다.
    /// - Step % 3 == 0: 스폰 수 +1
    /// - Step % 3 == 1: 몬스터 체력 배율 증가
    /// - Step % 3 == 2: 스폰 간격 감소
    /// </summary>
    public sealed class DifficultyModule : HostModuleBase<DifficultyModuleConfig>
    {
        public const string MODULE_ID = "difficulty";

        private float _elapsedTime;
        private float _spawnTimer;
        private int _currentStep;
        private int _spawnCount;
        private float _healthMultiplier;
        private float _spawnInterval;

        /// <inheritdoc />
        public override string ModuleId => MODULE_ID;

        /// <inheritdoc />
        public override bool IsRequired => false;

        /// <inheritdoc />
        public override int Priority => 80;

        /// <summary>
        /// 현재 난이도 스텝입니다.
        /// </summary>
        public int CurrentStep => _currentStep;

        /// <summary>
        /// 현재 스폰 수입니다.
        /// </summary>
        public int SpawnCount => _spawnCount;

        /// <summary>
        /// 현재 체력 배율입니다.
        /// </summary>
        public float HealthMultiplier => _healthMultiplier;

        /// <summary>
        /// 현재 스폰 간격입니다.
        /// </summary>
        public float SpawnInterval => _spawnInterval;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _spawnCount = Config.DefaultSpawnCount;
            _healthMultiplier = 1f;
            _spawnInterval = Config.DefaultSpawnInterval;
            _elapsedTime = 0f;
            _spawnTimer = 0f;
            _currentStep = 0;

            SubscribeInnerEvent<GetDifficultyStatusRequest>(OnGetDifficultyStatusRequest);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            UnsubscribeInnerEvent<GetDifficultyStatusRequest>(OnGetDifficultyStatusRequest);
        }


        /// <inheritdoc />
        protected override void OnTick(long tick, float deltaTime)
        {
            _elapsedTime += deltaTime;

            // 난이도 스텝 체크
            UpdateDifficultySteps(tick);

            // 몬스터 스폰
            _spawnTimer += deltaTime;
            while (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer -= _spawnInterval;
                SpawnMonsters(tick);
            }
        }

        private void UpdateDifficultySteps(long tick)
        {
            if (Config.StepInterval <= 0f)
            {
                return;
            }

            // 첫 스텝은 StepInterval 이후 (예: 10초)
            var expectedStep = (int)((_elapsedTime) / Config.StepInterval);
            // 0초~9.99초: expectedStep=0 → 스텝 없음
            // 10초: expectedStep=1 → 첫 번째 스텝

            while (_currentStep < expectedStep)
            {
                var stepType = _currentStep % 3;
                switch (stepType)
                {
                    case 0:
                        _spawnCount += Config.SpawnCountIncrease;
                        break;
                    case 1:
                        _healthMultiplier += Config.HealthMultiplierIncrease;
                        break;
                    case 2:
                        _spawnInterval *= Config.SpawnIntervalMultiplier;
                        break;
                }

                _currentStep++;

                PublishInnerEvent(new DifficultyStepChangedInnerEvent(
                    tick,
                    _currentStep,
                    _spawnCount,
                    _healthMultiplier,
                    _spawnInterval
                ));
            }
        }

        private void SpawnMonsters(long tick)
        {
            for (int p = 0; p < Context.PlayerCount; p++)
            {
                for (int i = 0; i < _spawnCount; i++)
                {
                    PublishInnerEvent(new MonsterSpawnRequestInnerEvent(
                        tick,
                        p,
                        Config.DefaultMonsterId,
                        Config.DefaultPathIndex,
                        _currentStep,
                        _healthMultiplier
                    ));
                }
            }
        }

        /// <summary>
        /// 난이도 상태를 리셋합니다.
        /// </summary>
        public void Reset()
        {
            _elapsedTime = 0f;
            _spawnTimer = 0f;
            _currentStep = 0;
            _spawnCount = Config.DefaultSpawnCount;
            _healthMultiplier = 1f;
            _spawnInterval = Config.DefaultSpawnInterval;
        }

        private void OnGetDifficultyStatusRequest(GetDifficultyStatusRequest evt)
        {
            evt.CurrentStep = _currentStep;
            evt.SpawnCount = _spawnCount;
            evt.HealthMultiplier = _healthMultiplier;
            evt.SpawnInterval = _spawnInterval;
        }
    }
}
