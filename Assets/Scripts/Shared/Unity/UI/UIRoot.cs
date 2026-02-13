using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// UGUI 루트 오브젝트의 기본 구성 요소를 관리합니다.
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
        /// UI 전용 카메라입니다.
        /// </summary>
        public Camera UICamera => _uiCamera;

        /// <summary>
        /// 페이지 캔버스입니다.
        /// </summary>
        public Canvas PageCanvas => _pageCanvas;

        /// <summary>
        /// 팝업 캔버스입니다.
        /// </summary>
        public Canvas PopupCanvas => _popupCanvas;

        /// <summary>
        /// 시스템 캔버스입니다.
        /// </summary>
        public Canvas SystemCanvas => _systemCanvas;

        /// <summary>
        /// 페이지 루트입니다.
        /// </summary>
        public RectTransform PageRoot => _pageRoot;

        /// <summary>
        /// 팝업 루트입니다.
        /// </summary>
        public RectTransform PopupRoot => _popupRoot;

        /// <summary>
        /// 시스템 루트입니다.
        /// </summary>
        public RectTransform SystemRoot => _systemRoot;
        /// <summary>
        /// Awake 메서드입니다.
        /// </summary>


        private void Awake()
        {
            ApplyCanvasSettings();

            AttackToMainCameraStack();
        }
        /// <summary>
        /// OnDestroy 메서드입니다.
        /// </summary>

        private void OnDestroy()
        {
            DettachFromMainCameraStack();    
        }
        /// <summary>
        /// OnValidate 메서드입니다.
        /// </summary>

        private void OnValidate()
        {
            ApplyCanvasSettings();
        }
        /// <summary>
        /// ApplyCanvasSettings 메서드입니다.
        /// </summary>

        private void ApplyCanvasSettings()
        {
            ApplyCanvas(_pageCanvas, _pageOrder);
            ApplyCanvas(_popupCanvas, _popupOrder);
            ApplyCanvas(_systemCanvas, _systemOrder);
        }
        /// <summary>
        /// ApplyCanvas 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// AttackToMainCameraStack 메서드입니다.
        /// </summary>

        private void AttackToMainCameraStack()
        {
            if(Camera.main == null) return;

            // var baseData = Camera.main.GetUniversalAdditionalCameraData(); // 필요 시 카메라 스택 연결
            // if (!baseData.cameraStack.Contains(_uiCamera)) // 스택 중복 방지
            //     baseData.cameraStack.Add(_uiCamera); // UI 카메라 추가
        }
        /// <summary>
        /// DettachFromMainCameraStack 메서드입니다.
        /// </summary>

        private void DettachFromMainCameraStack()
        {
            if(Camera.main == null) return;

            // var baseData = Camera.main.GetUniversalAdditionalCameraData(); // 필요 시 카메라 스택 연결
            // if (baseData != null) baseData.cameraStack.Remove(_uiCamera); // UI 카메라 제거
        }
    }
}
