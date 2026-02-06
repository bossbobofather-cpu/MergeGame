using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// ?? ???? ???? ? ???????.
    /// Host?? ??? ?? ???? ?????.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergeSlotView : MonoBehaviour
    {
        [SerializeField] private int _slotIndex = -1;

        /// <summary>
        /// ?? ??????. (0-based)
        /// </summary>
        public int SlotIndex => _slotIndex;

        /// <summary>
        /// ?? ???? ?????.
        /// </summary>
        public void SetSlotIndex(int slotIndex)
        {
            _slotIndex = slotIndex;
        }
    }
}

