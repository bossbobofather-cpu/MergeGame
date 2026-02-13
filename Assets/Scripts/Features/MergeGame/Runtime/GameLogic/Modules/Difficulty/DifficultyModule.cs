using Noname.GameHost.Module;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// ?쒖씠??紐⑤뱢?낅땲??
    /// 寃뚯엫 ?쒖옉遺???곗냽?쇰줈 紐ъ뒪?곕? ?ㅽ룿?섎ŉ,
    /// 10珥덈쭏???쒖씠???ㅽ뀦???쒗솚 ?곸슜?⑸땲??
    /// - Step % 3 == 0: ?ㅽ룿 ??+1
    /// - Step % 3 == 1: 紐ъ뒪??泥대젰 諛곗쑉 利앷?
    /// - Step % 3 == 2: ?ㅽ룿 媛꾧꺽 媛먯냼
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
        /// ?꾩옱 ?쒖씠???ㅽ뀦?낅땲??
        /// </summary>
        public int CurrentStep => _currentStep;

        /// <summary>
        /// ?꾩옱 ?ㅽ룿 ?섏엯?덈떎.
        /// </summary>
        public int SpawnCount => _spawnCount;

        /// <summary>
        /// ?꾩옱 泥대젰 諛곗쑉?낅땲??
        /// </summary>
        public float HealthMultiplier => _healthMultiplier;

        /// <summary>
        /// ?꾩옱 ?ㅽ룿 媛꾧꺽?낅땲??
        /// </summary>
        public float SpawnInterval => _spawnInterval;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // 핵심 로직을 처리합니다.
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
            // 핵심 로직을 처리합니다.
            UnsubscribeInnerEvent<GetDifficultyStatusRequest>(OnGetDifficultyStatusRequest);
        }


        /// <inheritdoc />
        protected override void OnTick(long tick, float deltaTime)
        {
            // 핵심 로직을 처리합니다.
            _elapsedTime += deltaTime;

            // ?쒖씠???ㅽ뀦 泥댄겕
            UpdateDifficultySteps(tick);

            // 紐ъ뒪???ㅽ룿
            _spawnTimer += deltaTime;
            while (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer -= _spawnInterval;
                SpawnMonsters(tick);
            }
        }
        /// <summary>
        /// UpdateDifficultySteps 함수를 처리합니다.
        /// </summary>

        private void UpdateDifficultySteps(long tick)
        {
            // 핵심 로직을 처리합니다.
            if (Config.StepInterval <= 0f)
            {
                return;
            }

            // 泥??ㅽ뀦? StepInterval ?댄썑 (?? 10珥?
            var expectedStep = (int)((_elapsedTime) / Config.StepInterval);
            // 0珥?9.99珥? expectedStep=0 ???ㅽ뀦 ?놁쓬
            // 10珥? expectedStep=1 ??泥?踰덉㎏ ?ㅽ뀦

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
        /// SpawnMonsters 함수를 처리합니다.
        /// </summary>

        private void SpawnMonsters(long tick)
        {
            // 핵심 로직을 처리합니다.
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
        /// ?쒖씠???곹깭瑜?由ъ뀑?⑸땲??
        /// </summary>
        public void Reset()
        {
            // 핵심 로직을 처리합니다.
            _elapsedTime = 0f;
            _spawnTimer = 0f;
            _currentStep = 0;
            _spawnCount = Config.DefaultSpawnCount;
            _healthMultiplier = 1f;
            _spawnInterval = Config.DefaultSpawnInterval;
        }
        /// <summary>
        /// OnGetDifficultyStatusRequest 함수를 처리합니다.
        /// </summary>

        private void OnGetDifficultyStatusRequest(GetDifficultyStatusRequest evt)
        {
            // 핵심 로직을 처리합니다.
            evt.CurrentStep = _currentStep;
            evt.SpawnCount = _spawnCount;
            evt.HealthMultiplier = _healthMultiplier;
            evt.SpawnInterval = _spawnInterval;
        }
    }
}
