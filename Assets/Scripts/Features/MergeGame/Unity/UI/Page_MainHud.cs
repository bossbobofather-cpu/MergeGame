using System.Collections.Generic;
using MyProject.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame의 메인 HUD 페이지입니다.
    /// 게임 시작/타워 스폰 버튼과 상태 텍스트를 제공합니다.
    /// </summary>
    public sealed class Page_MainHud : UIPageBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _spawnTowerButton;
        [SerializeField] private Button _readyButton;

        [Header("Texts")]
        [SerializeField] private Text _resultText;
        [SerializeField] private Text _monsterNumText;
        [SerializeField] private Text _difficultyStepText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _loadingText;

        [Header("Observer Mini Map")]
        [SerializeField] private List<RawImage> _observerMiniMapImages = new();

        private readonly Dictionary<int, int> _observerMiniMapPlayerIndexBySlot = new();

        /// <summary>
        /// 타워 스폰 버튼입니다.
        /// </summary>
        public Button SpawnTowerButton => _spawnTowerButton;

        /// <summary>
        /// 준비 버튼입니다.
        /// </summary>
        public Button ReadyButton => _readyButton;

        public int ObserverMiniMapSlotCount => _observerMiniMapImages?.Count ?? 0;
        /// <summary>
        /// SetActiveSpawnButton 메서드입니다.
        /// </summary>

        public void SetActiveSpawnButton(bool active)
        {
            SpawnTowerButton?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveReadyButton 메서드입니다.
        /// </summary>

        public void SetActiveReadyButton(bool active)
        {
            ReadyButton?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveResultText 메서드입니다.
        /// </summary>

        public void SetActiveResultText(bool active)
        {
            _resultText?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveMonsterNumText 메서드입니다.
        /// </summary>

        public void SetActiveMonsterNumText(bool active)
        {
            _monsterNumText?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveDifficultyStepText 메서드입니다.
        /// </summary>

        public void SetActiveDifficultyStepText(bool active)
        {
            _difficultyStepText?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveGoldText 메서드입니다.
        /// </summary>

        public void SetActiveGoldText(bool active)
        {
            _goldText?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetActiveLoadingText 메서드입니다.
        /// </summary>

        public void SetActiveLoadingText(bool active)
        {
            _loadingText?.gameObject.SetActive(active);
        }
        /// <summary>
        /// SetObserverMiniMapTexture 메서드입니다.
        /// </summary>

        public void SetObserverMiniMapTexture(int slotIndex, Texture texture)
        {
            SetObserverMiniMapTexture(slotIndex, -1, texture);
        }
        /// <summary>
        /// SetObserverMiniMapTexture 메서드입니다.
        /// </summary>

        public void SetObserverMiniMapTexture(int slotIndex, int playerIndex, Texture texture)
        {
            if (_observerMiniMapImages == null || slotIndex < 0 || slotIndex >= _observerMiniMapImages.Count)
            {
                return;
            }

            var image = _observerMiniMapImages[slotIndex];
            if (image == null)
            {
                return;
            }

            image.texture = texture;
            image.gameObject.SetActive(texture != null);

            if (texture == null || playerIndex < 0)
            {
                _observerMiniMapPlayerIndexBySlot.Remove(slotIndex);
            }
            else
            {
                _observerMiniMapPlayerIndexBySlot[slotIndex] = playerIndex;
            }
        }
        /// <summary>
        /// TryGetObserverMiniMapCenterScreenPosition 메서드입니다.
        /// </summary>

        public bool TryGetObserverMiniMapCenterScreenPosition(int playerIndex, out Vector2 screenPosition)
        {
            screenPosition = default;
            if (_observerMiniMapImages == null || playerIndex < 0)
            {
                return false;
            }

            for (var i = 0; i < _observerMiniMapImages.Count; i++)
            {
                if (!_observerMiniMapPlayerIndexBySlot.TryGetValue(i, out var mappedPlayerIndex) || mappedPlayerIndex != playerIndex)
                {
                    continue;
                }

                var image = _observerMiniMapImages[i];
                if (image == null)
                {
                    continue;
                }

                var rectTransform = image.rectTransform;
                if (rectTransform == null)
                {
                    continue;
                }

                var worldCenter = rectTransform.TransformPoint(rectTransform.rect.center);

                var canvas = rectTransform.GetComponentInParent<Canvas>();
                var cameraForConversion = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                screenPosition = RectTransformUtility.WorldToScreenPoint(cameraForConversion, worldCenter);
                return true;
            }

            return false;
        }
        /// <summary>
        /// ClearObserverMiniMaps 메서드입니다.
        /// </summary>

        public void ClearObserverMiniMaps()
        {
            _observerMiniMapPlayerIndexBySlot.Clear();

            if (_observerMiniMapImages == null)
            {
                return;
            }

            for (var i = 0; i < _observerMiniMapImages.Count; i++)
            {
                var image = _observerMiniMapImages[i];
                if (image == null)
                {
                    continue;
                }

                image.texture = null;
                image.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// SetResultText 메서드입니다.
        /// </summary>

        public void SetResultText(string text)
        {
            if (_resultText != null) _resultText.text = text;
        }
        /// <summary>
        /// SetMonsterNumText 메서드입니다.
        /// </summary>

        public void SetMonsterNumText(string text)
        {
            if (_monsterNumText != null) _monsterNumText.text = text;
        }
        /// <summary>
        /// SetDifficultyText 메서드입니다.
        /// </summary>

        public void SetDifficultyText(string text)
        {
            if (_difficultyStepText != null) _difficultyStepText.text = text;
        }
        /// <summary>
        /// SetGoldText 메서드입니다.
        /// </summary>

        public void SetGoldText(string text)
        {
            if (_goldText != null) _goldText.text = text;
        }
        /// <summary>
        /// SetLoadingText 메서드입니다.
        /// </summary>

        public void SetLoadingText(string text)
        {
            if (_loadingText != null) _loadingText.text = text;
        }
    }
}


