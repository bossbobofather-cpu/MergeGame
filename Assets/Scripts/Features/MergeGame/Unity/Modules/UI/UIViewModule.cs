using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using MyProject.MergeGame.Unity.Events;
using MyProject.MergeGame.Unity.Network;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame HUD 모듈입니다.
    /// 스냅샷을 받아 현재 상태를 표시합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIViewModule : MergeViewModuleBase
    {
        [Header("Main HUD Page")]
        [SerializeField] private Page_MainHud _mainHudPrefab;

        [Header("Loading Text")]
        [SerializeField] private string _waitingOpponentReadyText = "상대방 준비 대기중...";

        private Page_MainHud _mainHudInstance;

        private int _currentMonsterCount = 0;
        private int _maxMonsterStack = 0;
        private int _difficultyStep = 0;
        private int _gold = 0;
        /// <summary>
        /// OnInit 함수를 처리합니다.
        /// </summary>

        protected override void OnInit()
        {
            // 핵심 로직을 처리합니다.
            base.OnInit();
            EnsureMainHudPage();

            if (GameView != null)
            {
                GameView.Subscribe<MiniMapRenderTargetsUpdatedEvent>(OnMiniMapTargetsUpdated);
            }
        }
        /// <summary>
        /// OnConnectedEvent 함수를 처리합니다.
        /// </summary>

        public override void OnConnectedEvent()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null) return;

            _mainHudInstance.gameObject.SetActive(true);
            _mainHudInstance.SetActiveReadyButton(true);
            _mainHudInstance.SetActiveSpawnButton(false);
            _mainHudInstance.SetActiveLoadingText(false);
            _mainHudInstance.ClearObserverMiniMaps();
        }
        /// <summary>
        /// OnDisconnectedEvent 함수를 처리합니다.
        /// </summary>

        public override void OnDisconnectedEvent()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null) return;

            _mainHudInstance.SetActiveReadyButton(false);
            _mainHudInstance.SetActiveSpawnButton(false);
            _mainHudInstance.SetActiveLoadingText(false);
            _mainHudInstance.ClearObserverMiniMaps();
        }
        /// <summary>
        /// OnCommandResultMsg 함수를 처리합니다.
        /// </summary>

        public override void OnCommandResultMsg(MergeCommandResult result)
        {
            // 핵심 로직을 처리합니다.
            if (result == null) return;

            if (result is ReadyMergeGameResult readyCmdResult)
            {
                if (readyCmdResult.Success)
                {
                    _mainHudInstance?.SetActiveReadyButton(false);
                    _mainHudInstance?.SetLoadingText(_waitingOpponentReadyText);
                    _mainHudInstance?.SetActiveLoadingText(true);

                    _mainHudInstance?.SetActiveMonsterNumText(true);
                    _mainHudInstance?.SetActiveDifficultyStepText(true);
                    _mainHudInstance?.SetActiveGoldText(true);

                    RefreshMonsterNumText();
                    RefreshDifficultyText();
                    RefreshGoldNumText();
                }
                else
                {
                    _mainHudInstance?.SetActiveReadyButton(true);
                    _mainHudInstance?.SetActiveLoadingText(false);
                }
            }
        }
        /// <summary>
        /// OnEventMsg 함수를 처리합니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (!IsMyEvent(evt)) return;

            if (evt is GameStartedEvent)
            {
                _mainHudInstance?.SetActiveLoadingText(false);
                _mainHudInstance?.SetActiveSpawnButton(true);
            }
            else if (evt is MonsterSpawnedEvent)
            {
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

                _mainHudInstance?.SetActiveLoadingText(false);
                _mainHudInstance?.SetActiveSpawnButton(false);
            }
        }
        /// <summary>
        /// OnSnapshotMsg 함수를 처리합니다.
        /// </summary>

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            // 핵심 로직을 처리합니다.
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
        /// <summary>
        /// OnMiniMapTargetsUpdated 함수를 처리합니다.
        /// </summary>

        private void OnMiniMapTargetsUpdated(MiniMapRenderTargetsUpdatedEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null || evt == null)
            {
                return;
            }

            var slotCount = _mainHudInstance.ObserverMiniMapSlotCount;
            if (slotCount <= 0)
            {
                return;
            }

            _mainHudInstance.ClearObserverMiniMaps();

            var targets = evt.Targets;
            if (targets == null)
            {
                return;
            }

            var writeIndex = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                if (writeIndex >= slotCount)
                {
                    break;
                }

                _mainHudInstance.SetObserverMiniMapTexture(writeIndex, targets[i].PlayerIndex, targets[i].Texture);
                writeIndex++;
            }
        }
        /// <summary>
        /// EnsureMainHudPage 함수를 처리합니다.
        /// </summary>

        private void EnsureMainHudPage()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance != null)
            {
                return;
            }

            var ui = UIManager.Instance;
            if (ui == null)
            {
                return;
            }

            _mainHudInstance = _mainHudPrefab != null
                ? ui.OpenPage(_mainHudPrefab)
                : ui.OpenPage<Page_MainHud>();

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

            _mainHudInstance.gameObject.SetActive(false);
            _mainHudInstance.SetActiveSpawnButton(false);
            _mainHudInstance.SetActiveReadyButton(false);
            _mainHudInstance.SetActiveResultText(false);
            _mainHudInstance.SetActiveLoadingText(false);
            _mainHudInstance.SetLoadingText(_waitingOpponentReadyText);
            _mainHudInstance.ClearObserverMiniMaps();

            _mainHudInstance.SetActiveMonsterNumText(true);
            RefreshMonsterNumText();
        }
        /// <summary>
        /// HandleSpawnTowerClicked 함수를 처리합니다.
        /// </summary>

        private void HandleSpawnTowerClicked()
        {
            // 핵심 로직을 처리합니다.
            if (GameView == null)
            {
                return;
            }

            GameView.SendCommand(
                new SpawnTowerCommand(GameView.LocalUserId),
                MergeNetCommandType.SpawnTower);
        }
        /// <summary>
        /// HandleReadyClicked 함수를 처리합니다.
        /// </summary>

        private void HandleReadyClicked()
        {
            // 핵심 로직을 처리합니다.
            if (GameView == null)
            {
                return;
            }

            GameView.SendReady();
        }
        /// <summary>
        /// RefreshMonsterNumText 함수를 처리합니다.
        /// </summary>

        private void RefreshMonsterNumText()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null)
            {
                return;
            }

            var maxText = _maxMonsterStack > 0 ? _maxMonsterStack.ToString() : "?";
            _mainHudInstance.SetMonsterNumText($"몬스터 : {_currentMonsterCount} / {maxText}");
        }
        /// <summary>
        /// RefreshGoldNumText 함수를 처리합니다.
        /// </summary>

        private void RefreshGoldNumText()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null)
            {
                return;
            }

            _mainHudInstance.SetGoldText($"Gold : {_gold}");
        }
        /// <summary>
        /// RefreshDifficultyText 함수를 처리합니다.
        /// </summary>

        private void RefreshDifficultyText()
        {
            // 핵심 로직을 처리합니다.
            if (_mainHudInstance == null)
            {
                return;
            }

            _mainHudInstance.SetDifficultyText($"Difficulty Step : {_difficultyStep}");
        }
        /// <summary>
        /// OnShutdown 함수를 처리합니다.
        /// </summary>

        protected override void OnShutdown()
        {
            // 핵심 로직을 처리합니다.
            if (GameView != null)
            {
                GameView.Unsubscribe<MiniMapRenderTargetsUpdatedEvent>(OnMiniMapTargetsUpdated);
            }

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

                _mainHudInstance.ClearObserverMiniMaps();
            }

            _mainHudInstance = null;
            base.OnShutdown();
        }
    }
}

