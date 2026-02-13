using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
        /// </summary>
        public int CurrentStep => _currentStep;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SpawnCount => _spawnCount;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float HealthMultiplier => _healthMultiplier;

        /// <summary>
        /// 요약 설명입니다.
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
            UpdateDifficultySteps(tick);
            _spawnTimer += deltaTime;
            while (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer -= _spawnInterval;
                SpawnMonsters(tick);
            }
        }
        /// <summary>
        /// UpdateDifficultySteps 메서드입니다.
        /// </summary>

        private void UpdateDifficultySteps(long tick)
        {
            if (Config.StepInterval <= 0f)
            {
                return;
            }
            var expectedStep = (int)((_elapsedTime) / Config.StepInterval);
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
        /// <summary>
        /// SpawnMonsters 메서드입니다.
        /// </summary>

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
        /// 요약 설명입니다.
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
        /// <summary>
        /// OnGetDifficultyStatusRequest 메서드입니다.
        /// </summary>

        private void OnGetDifficultyStatusRequest(GetDifficultyStatusRequest evt)
        {
            evt.CurrentStep = _currentStep;
            evt.SpawnCount = _spawnCount;
            evt.HealthMultiplier = _healthMultiplier;
            evt.SpawnInterval = _spawnInterval;
        }
    }
}
