using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using MyProject.MergeGame.Unity.Network;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame HUD 모듈입니다.
    /// 스냅샷을 받아 현재 상태(HP/Gold/Wave/Score)를 표시합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIViewModule : MergeViewModuleBase
    {
        [Header("Main HUD Page")]
        [SerializeField] private Page_MainHud _mainHudPrefab;

        private Page_MainHud _mainHudInstance;

        protected override void OnInit()
        {
            base.OnInit();

            EnsureMainHudPage();
        }

        public override void OnConnectedEvent()
        {
            if(_mainHudInstance == null) return;
            
            //연결 성공 시 HUD UI Active 킨다.
            _mainHudInstance.gameObject.SetActive(true);
            _mainHudInstance.SetActiveReadyButton(true);
        }

        public override void OnCommandResultMsg(MergeCommandResult result)
        {
            if(result == null) return;

            if(result is ReadyMergeGameResult readyCmdResult)
            {
                if(readyCmdResult.Success)
                {
                    //Ready 성공했으면 Ready 버튼 숨긴다.
                    _mainHudInstance?.SetActiveReadyButton(false);
                }
            }
        }

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if(evt == null) return;

            if (evt is GameStartedEvent gameStartedEvent)
            {
                if (!IsMyPlayer(gameStartedEvent.PlayerIndex))
                {
                    return;
                }

                // GameStartedEvent 이벤트가 오면 스폰 버튼 활성화
                _mainHudInstance?.SetActiveSpawnButton(true);
            }
        }

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            if (snapshot == null || !IsMySnapshot(snapshot))
            {
                return;
            }
        }

        private void EnsureMainHudPage()
        {
            if (_mainHudInstance != null)
            {
                return;
            }

            var ui = UIManager.Instance;
            if (ui == null)
            {
                return;
            }

            if (_mainHudInstance == null)
            {
                _mainHudInstance = _mainHudPrefab != null
                    ? ui.OpenPage(_mainHudPrefab)
                    : ui.OpenPage<Page_MainHud>();
            }

            if (_mainHudInstance == null)
            {
                return;
            }

            if (_mainHudInstance.SpawnTowerButton != null)
            {
                _mainHudInstance.SpawnTowerButton.onClick.AddListener(HandleSpawnTowerClicked);
            }

            if (_mainHudInstance.ReadyButton != null)
            {
                _mainHudInstance.ReadyButton.onClick.AddListener(HandleReadyClicked);
            }

            //만들어두고 액티브 꺼둔다.
            //서버 연결 성공 시 켠다.
            _mainHudInstance.gameObject.SetActive(false);

            //두 버튼 다 꺼두고 필요할 때 켜는 걸로
            _mainHudInstance.SetActiveSpawnButton(false);
            _mainHudInstance.SetActiveReadyButton(false);
        }

        private void HandleSpawnTowerClicked()
        {
            if (GameView == null)
            {
                return;
            }

            GameView.SendCommand(
                new SpawnTowerCommand(GameView.LocalUserId),
                MergeNetCommandType.SpawnTower);
        }

        private void HandleReadyClicked()
        {
            if (GameView == null)
            {
                return;
            }

            GameView.SendReady();
        }

        protected override void OnShutdown()
        {
            if (_mainHudInstance != null)
            {
                if (_mainHudInstance.SpawnTowerButton != null)
                {
                    _mainHudInstance.SpawnTowerButton.onClick.RemoveListener(HandleSpawnTowerClicked);
                }

                if (_mainHudInstance.ReadyButton != null)
                {
                    _mainHudInstance.ReadyButton.onClick.RemoveListener(HandleReadyClicked);
                }
            }

            _mainHudInstance = null;

            base.OnShutdown();
        }
    }
}


