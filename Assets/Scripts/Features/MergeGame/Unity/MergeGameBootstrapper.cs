using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame 뷰를 생성하고 시작하기를 담당하는 부트스트래퍼입니다.
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
                Debug.LogError("GameView Prefab이 설정되지 않았습니다.");
                return;
            }

            // 게임 뷰 프리팹을 생성합니다.
            var instance = Instantiate(_gameViewPrefab);
            if (instance == null)
            {
                Debug.LogError("GameView 인스턴스 생성에 실패했습니다.");
                return;
            }

            _gameViewInstance = instance;
            _gameViewInstance.Initialize(new MergeGameHost(_config));
        }
    }
}
