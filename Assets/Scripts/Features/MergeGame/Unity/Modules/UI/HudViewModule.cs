using UnityEngine;
using UnityEngine.UI;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// MergeGame HUD 모듈입니다.
    /// 스냅샷을 받아 현재 상태(HP/Gold/Wave/Score 등)를 화면에 표시합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudViewModule : MergeViewModuleBase
    {
        [Header("UI (Optional)")]
        [SerializeField] private Text _hpText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _miscText;

        [Header("Auto Setup")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _container;
        [SerializeField] private bool _autoCreateCanvas = true;

        [Header("Style")]
        [SerializeField] private Font _font;
        [SerializeField] private int _fontSize = 18;
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _panelColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Vector2 _panelPadding = new Vector2(10f, 10f);

        private bool _createdCanvas;
        private bool _createdContainer;

        public override void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            EnsureHud();

            if (_hpText != null)
            {
                _hpText.text = $"HP: {snapshot.PlayerHp}/{snapshot.PlayerMaxHp} ({snapshot.PlayerHpRatio:P0})";
            }

            if (_goldText != null)
            {
                _goldText.text = $"Gold: {snapshot.PlayerGold}";
            }

            if (_waveText != null)
            {
                _waveText.text = $"Wave: {snapshot.CurrentWaveNumber} ({snapshot.WavePhase})";
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {snapshot.Score}  MaxGrade: {snapshot.MaxGrade}";
            }

            if (_miscText != null)
            {
                _miscText.text =
                    $"Phase: {snapshot.SessionPhase}\n" +
                    $"Slots: {snapshot.UsedSlots}/{snapshot.TotalSlots}\n" +
                    $"Time: {snapshot.ElapsedTime:F1}s\n" +
                    $"Tick: {snapshot.Tick}";
            }
        }

        private void EnsureHud()
        {
            EnsureCanvas();
            EnsureContainer();
            EnsureFont();

            if (_hpText == null)
            {
                _hpText = CreateRow("HP");
            }

            if (_goldText == null)
            {
                _goldText = CreateRow("Gold");
            }

            if (_waveText == null)
            {
                _waveText = CreateRow("Wave");
            }

            if (_scoreText == null)
            {
                _scoreText = CreateRow("Score");
            }

            if (_miscText == null)
            {
                _miscText = CreateRow("Misc", preferredHeight: _fontSize * 4);
            }
        }

        private void EnsureCanvas()
        {
            if (_canvas != null)
            {
                return;
            }

            if (!_autoCreateCanvas)
            {
                return;
            }

            var obj = new GameObject("MergeHudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = obj.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _createdCanvas = true;

            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureContainer()
        {
            if (_container != null)
            {
                return;
            }

            if (_canvas == null)
            {
                return;
            }

            var panelObj = new GameObject("MergeHudPanel", typeof(RectTransform), typeof(Image));
            _container = panelObj.GetComponent<RectTransform>();
            _container.SetParent(_canvas.transform, false);
            _createdContainer = true;

            _container.anchorMin = new Vector2(0f, 1f);
            _container.anchorMax = new Vector2(0f, 1f);
            _container.pivot = new Vector2(0f, 1f);
            _container.anchoredPosition = new Vector2(10f, -10f);
            _container.sizeDelta = new Vector2(420f, 0f);

            var image = panelObj.GetComponent<Image>();
            image.color = _panelColor;

            var layout = panelObj.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.padding = new RectOffset(
                Mathf.RoundToInt(_panelPadding.x),
                Mathf.RoundToInt(_panelPadding.x),
                Mathf.RoundToInt(_panelPadding.y),
                Mathf.RoundToInt(_panelPadding.y));
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = panelObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void EnsureFont()
        {
            if (_font != null)
            {
                return;
            }

            var text = GetComponentInChildren<Text>(true);
            if (text != null && text.font != null)
            {
                _font = text.font;
                return;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private Text CreateRow(string name, int preferredHeight = -1)
        {
            if (_container == null)
            {
                return null;
            }

            var obj = new GameObject($"Hud_{name}", typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(_container, false);

            var label = obj.AddComponent<Text>();
            label.font = _font;
            label.fontSize = _fontSize;
            label.color = _textColor;
            label.alignment = TextAnchor.UpperLeft;
            label.raycastTarget = false;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            var element = obj.AddComponent<LayoutElement>();
            element.preferredHeight = preferredHeight > 0 ? preferredHeight : (_fontSize + 6);

            return label;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            // 런타임에 만든 HUD 오브젝트만 정리합니다.
            if (_createdContainer && _container != null)
            {
                Destroy(_container.gameObject);
                _container = null;
            }

            if (_createdCanvas && _canvas != null)
            {
                Destroy(_canvas.gameObject);
                _canvas = null;
            }

            _createdCanvas = false;
            _createdContainer = false;

            _hpText = null;
            _goldText = null;
            _waveText = null;
            _scoreText = null;
            _miscText = null;
        }
    }
}
