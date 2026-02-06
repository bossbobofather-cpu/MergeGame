using System;
using System.Collections.Generic;
using MyProject.Common.Bootstrap;
using MyProject.MergeGame.Modules;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame 부트스트래퍼입니다.
    /// Host를 생성/초기화한 뒤 MergeGameView를 생성하고 Host를 주입합니다.
    /// </summary>
    public class MergeGameBootstrapper : BootstrapperBase
    {
        [SerializeField] private MergeHostConfig _config;
        [SerializeField] private MergeGameView _gameViewPrefab;

        private MergeGameView _gameViewInstance;

        protected override void OnInit()
        {
            base.OnInit();

            CreateGameMode();
        }

        private void CreateGameMode()
        {
            if (_gameViewPrefab == null)
            {
                Debug.LogError("[MergeGameBootstrapper] GameView Prefab이 할당되지 않았습니다.");
                return;
            }

            // Host 생성: StartSimulation 전에 모듈을 등록/초기화합니다.
            var hostConfig = _config ?? new MergeHostConfig();
            var host = new MergeGameHost(hostConfig, new DevTowerDatabase());

            host.AddModule(new MapModule(), BuildMapConfig());
            host.AddModule(new RuleModule(), BuildRuleConfig(hostConfig));
            host.AddModule(new WaveModule(), BuildWaveConfig(hostConfig));

            host.InitializeModules();
            host.StartSimulation();

            var instance = Instantiate(_gameViewPrefab);
            if (instance == null)
            {
                Debug.LogError("[MergeGameBootstrapper] GameView 인스턴스 생성에 실패했습니다.");

                host.StopSimulation();
                (host as IDisposable)?.Dispose();
                return;
            }

            _gameViewInstance = instance;

            // 기본 뷰 모듈을 자동으로 구성합니다.
            EnsureViewModules(_gameViewInstance);

            _gameViewInstance.Initialize(host);
        }

        private static void EnsureViewModules(MergeGameView view)
        {
            if (view == null)
            {
                return;
            }

            EnsureModule<MapViewModule>(view, "MapViewModule");
            EnsureModule<TowerViewModule>(view, "TowerViewModule");
            EnsureModule<MonsterViewModule>(view, "MonsterViewModule");
            EnsureModule<ProjectileViewModule>(view, "ProjectileViewModule");
            EnsureModule<HudViewModule>(view, "HudViewModule");
            EnsureModule<InputViewModule>(view, "InputViewModule");
        }

        private static T EnsureModule<T>(MergeGameView view, string name) where T : Component
        {
            var existing = view.GetComponentInChildren<T>(true);
            if (existing != null)
            {
                return existing;
            }

            var obj = new GameObject(name);
            obj.transform.SetParent(view.transform, false);
            return obj.AddComponent<T>();
        }

        private static MapModuleConfig BuildMapConfig()
        {
            // 프로토타입용: 단순 그리드 + 1개 경로
            return new MapModuleConfig
            {
                MapId = 1,
                SlotDefinitions = MapModuleConfig.CreateGridSlotDefinitions(rows: 4, columns: 4, slotWidth: 1.5f, slotHeight: 1.5f),
                PathDefinitions = new List<PathDefinition>
                {
                    new PathDefinition(
                        pathIndex: 0,
                        waypoints: new List<Point3D>
                        {
                            new Point3D(-6f, 3f, 0f),
                            new Point3D(-2f, 3f, 0f),
                            new Point3D( 2f, 3f, 0f),
                            new Point3D( 6f, 3f, 0f)
                        }
                    )
                }
            };
        }

        private static RuleModuleConfig BuildRuleConfig(MergeHostConfig hostConfig)
        {
            // Host 기본 설정을 RuleModuleConfig에 반영합니다.
            return new RuleModuleConfig
            {
                PlayerMaxHp = hostConfig.PlayerMaxHp,
                PlayerStartGold = hostConfig.PlayerStartGold,
                ScorePerGrade = hostConfig.ScorePerGrade,
                InitialUnitGrade = hostConfig.InitialUnitGrade,
                MaxUnitGrade = hostConfig.MaxUnitGrade,
                WaveCompletionBonusGold = hostConfig.WaveCompletionBonusGold,
            };
        }

        private static WaveModuleConfig BuildWaveConfig(MergeHostConfig hostConfig)
        {
            // 프로토타입용: 기본 웨이브 테이블 + 기본 증가값
            return new WaveModuleConfig
            {
                AutoStartWaves = true,
                WaveStartDelay = 2f,
                WaveIntervalDelay = 3f,
                DefaultSpawnInterval = hostConfig.WaveSpawnInterval,
                MaxWaveCount = 5,
                BaseMonsterCount = 3,
                MonstersPerWaveIncrease = 1,
            };
        }

        /// <summary>
        /// 프로토타입용 하드코딩 타워 DB입니다.
        /// 나중에 SO/JSON/서버 데이터로 교체할 수 있도록 인터페이스로 분리합니다.
        /// </summary>
        private sealed class DevTowerDatabase : ITowerDatabase
        {
            private readonly Dictionary<string, TowerDefinition> _definitions = new()
            {
                {
                    "unit_basic",
                    new TowerDefinition
                    {
                        TowerId = "unit_basic",
                        TowerType = "basic",
                        InitialGrade = 1,
                        BaseAttackDamage = 10f,
                        BaseAttackSpeed = 1f,
                        BaseAttackRange = 10f,
                        AttackType = TowerAttackType.HitScan,
                        ProjectileType = ProjectileType.Direct,
                        ProjectileSpeed = 8f,
                        ThrowRadius = 1.5f,
                    }
                },
            };

            public TowerDefinition GetDefinition(string towerId)
            {
                if (string.IsNullOrEmpty(towerId))
                {
                    return null;
                }

                return _definitions.TryGetValue(towerId, out var definition) ? definition : null;
            }

            public string GetRandomIdForGrade(int grade)
            {
                // 프로토타입에서는 기본 ID를 반환합니다.
                return "unit_basic";
            }
        }
    }
}


