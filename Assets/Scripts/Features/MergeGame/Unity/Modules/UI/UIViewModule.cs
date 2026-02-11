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
        private int _currentMonsterCount = 0;
        private int _maxMonsterStack = 0;
        private int _difficultyStep = 0;
        private int _gold = 0;

        protected override void OnInit()
        {
            base.OnInit();

            EnsureMainHudPage();
        }

        public override void OnConnectedEvent()
        {
            if (_mainHudInstance == null) return;

            // 연결 성공 시 HUD UI Active 킨다.
            _mainHudInstance.gameObject.SetActive(true);
            _mainHudInstance?.SetActiveReadyButton(true);
        }

        public override void OnCommandResultMsg(MergeCommandResult result)
        {
            if (result == null) return;

            if (result is ReadyMergeGameResult readyCmdResult)
            {
                if (readyCmdResult.Success)
                {
                    // Ready 성공했으면 Ready 버튼 숨긴다.
                    _mainHudInstance?.SetActiveReadyButton(false);

                    _mainHudInstance?.SetActiveMonsterNumText(true);
                    _mainHudInstance?.SetActiveDifficultyStepText(true);
                    _mainHudInstance?.SetActiveGoldText(true);

                    RefreshMonsterNumText();
                    RefreshDifficultyText();
                    RefreshGoldNumText();
                }
            }
        }

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (!IsMyEvent(evt)) return;

            if (evt is GameStartedEvent)
            {
                // GameStartedEvent 이벤트가 오면 스폰 버튼 활성화
                _mainHudInstance?.SetActiveSpawnButton(true);
            }
            else if (evt is MonsterSpawnedEvent)
            {
                // 스냅샷 반영 전까지 HUD 즉시성 확보를 위해 이벤트에서도 증가시킵니다.
                _currentMonsterCount++;
                RefreshMonsterNumText();
            }
            else if (evt is PlayerGoldChangedEvent goldChangedEvent)
            {
                _gold = goldChangedEvent.CurrentGold;
                RefreshGoldNumText();
            }
            else if (evt is DifficultyStepChangedEvent difficultyStepChangedEvent)
            {
                _difficultyStep = difficultyStepChangedEvent.Step;
                RefreshDifficultyText();
            }
            else if (evt is GameOverEvent gameOverEvent)
            {
                var result = gameOverEvent.IsVictory ? "Win" : "Lose";
                _mainHudInstance?.SetResultText(result);
                _mainHudInstance?.SetActiveResultText(true);

                _mainHudInstance?.SetActiveSpawnButton(false);
            }
        }

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            if (snapshot == null || !IsMySnapshot(snapshot))
            {
                return;
            }

            _currentMonsterCount = snapshot.Monsters?.Count ?? 0;
            _maxMonsterStack = snapshot.MaxMonsterStack;
            _gold = snapshot.PlayerGold;
            _difficultyStep = snapshot.DifficultyStep;

            RefreshMonsterNumText();
            RefreshDifficultyText();
            RefreshGoldNumText();
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

            // 만들어두고 액티브 꺼둔다.
            // 서버 연결 성공 시 켠다.
            _mainHudInstance.gameObject.SetActive(false);

            _mainHudInstance.SetActiveSpawnButton(false);
            _mainHudInstance.SetActiveReadyButton(false);
            _mainHudInstance.SetActiveResultText(false);
            _mainHudInstance.SetActiveMonsterNumText(true);
            RefreshMonsterNumText();
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

        private void RefreshMonsterNumText()
        {
            if (_mainHudInstance == null)
            {
                return;
            }

            var maxText = _maxMonsterStack > 0 ? _maxMonsterStack.ToString() : "?";
            _mainHudInstance.SetMonsterNumText($"몬스터 : {_currentMonsterCount} / {maxText}");
        }

        private void RefreshGoldNumText()
        {
            if (_mainHudInstance == null)
            {
                return;
            }

            _mainHudInstance.SetGoldText($"Gold : {_gold}");
        }

        private void RefreshDifficultyText()
        {
            if (_mainHudInstance == null)
            {
                return;
            }

            _mainHudInstance.SetDifficultyText($"Difficulty Step : {_difficultyStep}");
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

