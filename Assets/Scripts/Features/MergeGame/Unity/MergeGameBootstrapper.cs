using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Presentation
{
    /// <summary>
    /// MergeGame 모드를 생성하고 시작하기를 담당하는 부트스트래퍼입니다.
    /// </summary>
    public class MergeGameBootstrapper : BootstrapperBase
    {
        [SerializeField] private MergeHostConfig _config;
        [SerializeField] private MergeGameMode _gameModePrefab;

        private MergeGameMode _gameModeInstance;

        protected override void OnInit()
        {
            base.OnInit();

            CreateGameMode();
        }

        private void CreateGameMode()
        {
            if (_gameModePrefab == null)
            {
                Debug.LogError("GameMode Prefab이 설정되지 않았습니다.");
                return;
            }

            // 게임 모드 프리팹을 생성합니다.
            var instance = Instantiate(_gameModePrefab);
            if (instance == null)
            {
                Debug.LogError("GameMode 인스턴스 생성에 실패했습니다.");
                return;
            }

            _gameModeInstance = instance;
            _gameModeInstance.Initialize(new MergeGameHost(_config));
        }
    }
}
