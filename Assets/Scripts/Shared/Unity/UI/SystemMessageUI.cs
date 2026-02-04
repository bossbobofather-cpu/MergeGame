using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{
    /// <summary>
    /// ?붾㈃ 以묒븰 ?곷떒 ?깆뿉 ?쒖뒪??硫붿떆吏瑜??꾩슦??UI 留ㅻ땲??낅땲??
    /// SystemMessageBus ?대깽?몃? 援щ룆?섏뿬 硫붿떆吏瑜??쒖떆?⑸땲??
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SystemMessageUI : MonoBehaviour
    {
        [Header("UI")]
        /// <summary>
        /// 硫붿떆吏媛 ?앹꽦??遺紐?而⑦뀒?대꼫?낅땲??
        /// </summary>
        [SerializeField] private RectTransform _container;
        
        /// <summary>
        /// 硫붿떆吏???ъ슜???고듃?낅땲??
        /// </summary>
        [SerializeField] private Font _font;
        
        /// <summary>
        /// 硫붿떆吏 ?띿뒪???ш린?낅땲??
        /// </summary>
        [SerializeField] private int _fontSize = 24;
        
        /// <summary>
        /// 硫붿떆吏 ?띿뒪???됱긽?낅땲??
        /// </summary>
        [SerializeField] private Color _textColor = Color.white;
        
        /// <summary>
        /// 硫붿떆吏 諛곌꼍 ?됱긽?낅땲??
        /// </summary>
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        
        /// <summary>
        /// 硫붿떆吏 ?대? ?щ갚(padding)?낅땲??
        /// </summary>
        [SerializeField] private Vector2 _padding = new Vector2(10f, 4f);
        
        /// <summary>
        /// 硫붿떆吏 媛꾩쓽 媛꾧꺽?낅땲??
        /// </summary>
        [SerializeField] private float _rowSpacing = 2f;

        [Header("Behavior")]
        /// <summary>
        /// ?붾㈃???숈떆???쒖떆??理쒕? 硫붿떆吏 媛쒖닔?낅땲??
        /// </summary>
        [SerializeField] private int _maxVisible = 30;

        [Header("Auto Setup")]
        /// <summary>
        /// UI媛 諛곗튂??罹붾쾭?ㅼ엯?덈떎.
        /// </summary>
        [SerializeField] private Canvas _canvas;
        
        /// <summary>
        /// 罹붾쾭?ㅺ? ?놁쓣 寃쎌슦 ?먮룞 ?앹꽦 ?щ??낅땲??
        /// </summary>
        [SerializeField] private bool _autoCreateCanvas = true;

        private readonly List<MessageRow> _activeRows = new();
        private readonly Queue<MessageRow> _pool = new();

        private sealed class MessageRow
        {
            public GameObject Root;
            public Text Label;
            public Image Background;
            public CanvasGroup Group;
        }

        private void Awake()
        {
            EnsureCanvas();
            EnsureContainer();
            EnsureFont();
        }

        private void OnEnable()
        {
            SystemMessageBus.MessagePublished += HandleMessage;
        }

        private void OnDisable()
        {
            SystemMessageBus.MessagePublished -= HandleMessage;
        }

        // Update??媛쒖닔 湲곕컲?쇰줈留??숈옉?섎?濡??쒓굅

        /// <summary>
        /// ?쒖뒪??硫붿떆吏瑜?寃뚯떆?⑸땲??
        /// </summary>
        /// <param name="message">?쒖떆??硫붿떆吏 ?댁슜</param>
        public static void Post(string message)
        {
            SystemMessageBus.Publish(message);
        }

        private void HandleMessage(SystemMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            var row = GetOrCreateRow();
            row.Label.text = message.Text;

            // 諛곌꼍???곸슜
            if (row.Background != null)
            {
                row.Background.color = message.BackgroundColor;
            }

            if (row.Group != null)
            {
                row.Group.alpha = 1f;
            }

            // 理쒖떊 硫붿떆吏瑜??섎떒??異붽? (?꾨줈 ?볦엫)
            row.Root.transform.SetAsLastSibling();
            _activeRows.Add(row);
            TrimOverflow();
        }

        private MessageRow GetOrCreateRow()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Dequeue();
                pooled.Root.SetActive(true);
                return pooled;
            }

            return CreateRow();
        }

        private MessageRow CreateRow()
        {
            // 硫붿떆吏 ??Row) UI ?숈쟻 ?앹꽦
            var root = new GameObject("SystemMessageRow", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(_container, false);
            rect.localScale = Vector3.one;
            rect.sizeDelta = Vector2.zero;

            var background = root.AddComponent<Image>();
            background.color = _backgroundColor;

            var layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(
                Mathf.RoundToInt(_padding.x),
                Mathf.RoundToInt(_padding.x),
                Mathf.RoundToInt(_padding.y),
                Mathf.RoundToInt(_padding.y));
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var element = root.AddComponent<LayoutElement>();
            element.preferredHeight = _fontSize + _padding.y * 2f;

            var labelObj = new GameObject("Label", typeof(RectTransform));
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.localScale = Vector3.one;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            var text = labelObj.AddComponent<Text>();
            text.font = _font;
            text.fontSize = _fontSize;
            text.color = _textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var group = root.AddComponent<CanvasGroup>();

            return new MessageRow
            {
                Root = root,
                Label = text,
                Background = background,
                Group = group
            };
        }

        private void TrimOverflow()
        {
            // 理쒕? ?쒖떆 媛쒖닔瑜?珥덇낵?섎㈃ 媛???ㅻ옒??硫붿떆吏遺???쒓굅 (留??꾨???
            var max = Mathf.Max(1, _maxVisible);
            while (_activeRows.Count > max)
            {
                var row = _activeRows[0];
                _activeRows.RemoveAt(0);
                RecycleRow(row);
            }
        }

        private void RecycleRow(MessageRow row)
        {
            if (row == null || row.Root == null)
            {
                return;
            }

            row.Root.SetActive(false);
            _pool.Enqueue(row);
        }

        private void EnsureCanvas()
        {
            if (_container != null)
            {
                return;
            }

            if (_canvas == null)
            {
                if (_autoCreateCanvas)
                {
                    var obj = new GameObject("SystemMessageCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    _canvas = obj.GetComponent<Canvas>();
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    var scaler = obj.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
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

            // ?붾㈃ 理쒗븯??以묒븰??諛곗튂 (?꾨줈 ?볦씠???ㅽ깮??
            var obj = new GameObject("SystemMessageContainer", typeof(RectTransform));
            _container = obj.GetComponent<RectTransform>();
            _container.SetParent(_canvas.transform, false);
            _container.anchorMin = new Vector2(0.5f, 0f);
            _container.anchorMax = new Vector2(0.5f, 0f);
            _container.pivot = new Vector2(0.5f, 0f);
            _container.anchoredPosition = new Vector2(0f, 10f); // ?섎떒?먯꽌 10px ??
            _container.sizeDelta = new Vector2(1200f, 0f);

            var layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.spacing = _rowSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = obj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
    }
}


