using MyProject.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame ?? HUD ??????.
    /// ?? ?? ?? UI ??? ViewModule? ????? ???? ?????.
    /// </summary>
    public sealed class Page_MainHud : UIPageBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _spawnTowerButton;
        [SerializeField] private Button _startWaveButton;

        /// <summary>
        /// ?? ?? ?????.
        /// </summary>
        public Button SpawnTowerButton => _spawnTowerButton;

        /// <summary>
        /// ??? ?? ?????. (??)
        /// </summary>
        public Button StartWaveButton => _startWaveButton;
    }
}

