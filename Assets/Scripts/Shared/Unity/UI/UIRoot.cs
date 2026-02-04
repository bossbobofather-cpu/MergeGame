using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// UGUI 猷⑦듃 ?꾨━?뱀쓽 湲곕낯 援ъ꽦 ?붿냼瑜?愿由ы빀?덈떎.
    /// </summary>
    public sealed class UIRoot : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _uiCamera;

        [Header("Canvases")]
        [SerializeField] private Canvas _pageCanvas;
        [SerializeField] private Canvas _popupCanvas;
        [SerializeField] private Canvas _systemCanvas;

        [Header("Roots")]
        [SerializeField] private RectTransform _pageRoot;
        [SerializeField] private RectTransform _popupRoot;
        [SerializeField] private RectTransform _systemRoot;

        [Header("Sorting Order")]
        [SerializeField] private int _pageOrder = 0;
        [SerializeField] private int _popupOrder = 100;
        [SerializeField] private int _systemOrder = 200;

        /// <summary>
        /// UI ?꾩슜 移대찓?쇱엯?덈떎.
        /// </summary>
        public Camera UICamera => _uiCamera;

        /// <summary>
        /// ?섏씠吏 罹붾쾭?ㅼ엯?덈떎.
        /// </summary>
        public Canvas PageCanvas => _pageCanvas;

        /// <summary>
        /// ?앹뾽 罹붾쾭?ㅼ엯?덈떎.
        /// </summary>
        public Canvas PopupCanvas => _popupCanvas;

        /// <summary>
        /// ?쒖뒪??罹붾쾭?ㅼ엯?덈떎.
        /// </summary>
        public Canvas SystemCanvas => _systemCanvas;

        /// <summary>
        /// ?섏씠吏 猷⑦듃?낅땲??
        /// </summary>
        public RectTransform PageRoot => _pageRoot;

        /// <summary>
        /// ?앹뾽 猷⑦듃?낅땲??
        /// </summary>
        public RectTransform PopupRoot => _popupRoot;

        /// <summary>
        /// ?쒖뒪??猷⑦듃?낅땲??
        /// </summary>
        public RectTransform SystemRoot => _systemRoot;


        private void Awake()
        {
            ApplyCanvasSettings();

            AttackToMainCameraStack();
        }

        private void OnDestroy()
        {
            DettachFromMainCameraStack();    
        }

        private void OnValidate()
        {
            ApplyCanvasSettings();
        }

        private void ApplyCanvasSettings()
        {
            ApplyCanvas(_pageCanvas, _pageOrder);
            ApplyCanvas(_popupCanvas, _popupOrder);
            ApplyCanvas(_systemCanvas, _systemOrder);
        }

        private void ApplyCanvas(Canvas canvas, int sortingOrder)
        {
            if (canvas == null)
            {
                return;
            }

            canvas.renderMode = _uiCamera != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = _uiCamera;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        private void AttackToMainCameraStack()
        {
            if(Camera.main == null) return;

            // var baseData = Camera.main.GetUniversalAdditionalCameraData();
            // if (!baseData.cameraStack.Contains(_uiCamera))
            //     baseData.cameraStack.Add(_uiCamera);
        }

        private void DettachFromMainCameraStack()
        {
            if(Camera.main == null) return;

            // var baseData = Camera.main.GetUniversalAdditionalCameraData();
            // if (baseData != null) baseData.cameraStack.Remove(_uiCamera);
        }
    }
}


