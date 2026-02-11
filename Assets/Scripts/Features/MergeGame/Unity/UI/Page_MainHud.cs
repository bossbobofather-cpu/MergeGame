using MyProject.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame의 메인 HUD 페이지입니다.
    /// 게임 시작/타워 스폰 버튼을 제공하고, HUD 모듈에서 생성됩니다.
    /// </summary>
    public sealed class Page_MainHud : UIPageBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _spawnTowerButton;
        [SerializeField] private Button _readyButton;

        [SerializeField] private Text _resultText;
        [SerializeField] private Text _monsterNumText;
        [SerializeField] private Text _difficultyStepText;
        [SerializeField] private Text _goldText;

        /// <summary>
        /// 타워 스폰 버튼입니다.
        /// </summary>
        public Button SpawnTowerButton => _spawnTowerButton;

        /// <summary>
        /// 웨이브 시작 버튼입니다.
        /// </summary>
        public Button ReadyButton => _readyButton;

        public void SetActiveSpawnButton(bool active)
        {
            SpawnTowerButton?.gameObject.SetActive(active);
        }

        public void SetActiveReadyButton(bool active)
        {
            ReadyButton?.gameObject.SetActive(active);
        }

        public void SetActiveResultText(bool active)
        {
            _resultText?.gameObject.SetActive(active);
        }

        public void SetActiveMonsterNumText(bool active)
        {
            _monsterNumText?.gameObject.SetActive(active);
        }

        public void SetActiveDifficultyStepText(bool active)
        {
            _difficultyStepText?.gameObject.SetActive(active);
        }

        public void SetActiveGoldText(bool active)
        {
            _goldText?.gameObject.SetActive(active);
        }

        public void SetResultText(string text)
        {
            if(_resultText != null) _resultText.text = text;
        }

        public void SetMonsterNumText(string text)
        {
            if (_monsterNumText != null) _monsterNumText.text = text;
        }

        public void SetDifficultyText(string text)
        {
            if(_difficultyStepText != null) _difficultyStepText.text = text;
        }

        public void SetGoldText(string text)
        {
            if (_goldText != null) _goldText.text = text;
        }
    }
}
