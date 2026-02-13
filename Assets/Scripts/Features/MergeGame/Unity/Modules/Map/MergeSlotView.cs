using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 슬롯 오브젝트에 부착되는 뷰 컴포넌트입니다.
    /// Host에서 전달받은 슬롯 인덱스를 보관합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergeSlotView : MonoBehaviour
    {
        [SerializeField] private int _slotIndex = -1;

        /// <summary>
        /// 슬롯 인덱스입니다. (0-based)
        /// </summary>
        public int SlotIndex => _slotIndex;

        /// <summary>
        /// 슬롯 인덱스를 지정합니다.
        /// </summary>
        public void SetSlotIndex(int slotIndex)
        {
            // 핵심 로직을 처리합니다.
            _slotIndex = slotIndex;
        }
    }
}
