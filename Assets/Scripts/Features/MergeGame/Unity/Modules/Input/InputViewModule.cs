using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 입력을 받아 Move/Merge 커맨드를 발행하는 모듈입니다.
    /// Spawn은 HUD 버튼에서 처리하고, 여기서는 드래그/키보드 입력만 처리합니다.
    /// </summary>
    public sealed class InputViewModule : MergeViewModuleBase
    {
        [Header("Keyboard Input")]
        [SerializeField] private bool _enableKeyboardInput = false;

        [Header("Drag & Drop Input")]
        [SerializeField] private bool _enableDragInput = true;
        [SerializeField] private Camera _raycastCamera;
        [SerializeField] private LayerMask _slotLayerMask = ~0;

        [Header("Command Mode")]
        [SerializeField] private bool _useTowerCommands = true;

        [Header("Message")]
        [SerializeField] private Color _infoColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color _selectColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        [SerializeField] private Color _warnColor = new Color(1f, 0.55f, 0.1f, 0.6f);

        private int _selectedSlotIndex = -1;
        private int _dragFromSlotIndex = -1;
        private bool _isDragging;

        /// <summary>
        /// MergeGameView.Update에서 호출되는 입력 처리 루프입니다.
        /// </summary>
        public void TickInput()
        {
            if (GameView == null)
            {
                return;
            }

            var snapshot = GameView.LatestSnapshot;
            if (snapshot == null)
            {
                return;
            }

            if (_enableKeyboardInput)
            {
                HandleKeyboardInput(snapshot);
            }

            if (_enableDragInput)
            {
                HandleDragInput(snapshot);
            }
        }

        private void HandleKeyboardInput(MergeHostSnapshot snapshot)
        {
            // Esc: 선택 취소
            if (WasPressedCancel())
            {
                _selectedSlotIndex = -1;
                Post("[선택 취소]", _infoColor);
            }

            // 1~9: 슬롯 선택(2번 입력 시 Move/Merge 커맨드 발행)
            var digit = GetPressedDigit1To9();
            if (digit > 0)
            {
                HandleSlotDigit(digit, snapshot);
            }
        }

        private void HandleDragInput(MergeHostSnapshot snapshot)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (!_isDragging && WasPointerDown())
            {
                if (TryGetSlotUnderPointer(out var slotIndex))
                {
                    if (slotIndex < 0 || slotIndex >= snapshot.Slots.Count)
                    {
                        return;
                    }

                    var slot = snapshot.Slots[slotIndex];
                    if (slot.IsEmpty)
                    {
                        Post($"[드래그 실패] 슬롯 {slotIndex}이 비어있습니다.", _warnColor);
                        return;
                    }

                    _dragFromSlotIndex = slotIndex;
                    _isDragging = true;
                    Post($"[드래그 시작] 슬롯 {slotIndex}", _selectColor);
                }

                return;
            }

            if (_isDragging && WasPointerUp())
            {
                var from = _dragFromSlotIndex;
                _dragFromSlotIndex = -1;
                _isDragging = false;

                if (!TryGetSlotUnderPointer(out var to))
                {
                    Post("[드래그 종료] 슬롯이 선택되지 않았습니다.", _infoColor);
                    return;
                }

                if (from < 0 || from >= snapshot.Slots.Count || to < 0 || to >= snapshot.Slots.Count)
                {
                    Post("[드래그 실패] 슬롯 범위를 벗어났습니다.", _warnColor);
                    return;
                }

                if (from == to)
                {
                    Post("[드래그 종료] 동일 슬롯입니다.", _infoColor);
                    return;
                }

                var fromSlot = snapshot.Slots[from];
                var toSlot = snapshot.Slots[to];

                if (fromSlot.IsEmpty)
                {
                    Post($"[실패] From 슬롯({from})이 비어있습니다.", _warnColor);
                    return;
                }

                if (toSlot.IsEmpty)
                {
                    if (_useTowerCommands)
                    {
                        GameView.SendCommand(new MoveTowerCommand(GameView.LocalUserId, from, to));
                    }
                    else
                    {
                        Post("[이동] Legacy(Unit) 모드에서는 Move를 지원하지 않습니다.", _warnColor);
                    }

                    return;
                }

                if (_useTowerCommands)
                {
                    GameView.SendCommand(new MergeTowerCommand(GameView.LocalUserId, from, to));
                }
                else
                {
                    GameView.SendCommand(new MergeUnitCommand(GameView.LocalUserId, from, to));
                }
            }
        }

        private void HandleSlotDigit(int digit, MergeHostSnapshot snapshot)
        {
            // 키보드 입력은 1-based이므로 슬롯 인덱스는 0-based로 변환합니다.
            var slotIndex = digit - 1;
            if (slotIndex < 0 || slotIndex >= snapshot.Slots.Count)
            {
                Post($"[슬롯 범위 초과] 입력: {digit}, 슬롯수: {snapshot.Slots.Count}", _warnColor);
                return;
            }

            if (_selectedSlotIndex < 0)
            {
                _selectedSlotIndex = slotIndex;
                Post($"[슬롯 선택] {digit} (index: {slotIndex})", _selectColor);
                return;
            }

            var from = _selectedSlotIndex;
            var to = slotIndex;
            _selectedSlotIndex = -1;

            if (from == to)
            {
                Post("[동일 슬롯] 이동/머지를 생략합니다.", _infoColor);
                return;
            }

            var fromSlot = snapshot.Slots[from];
            var toSlot = snapshot.Slots[to];

            if (fromSlot.IsEmpty)
            {
                Post($"[실패] From 슬롯({from})이 비어있습니다.", _warnColor);
                return;
            }

            // 빈 슬롯이면 이동, 아니면 머지 시도
            if (toSlot.IsEmpty)
            {
                if (_useTowerCommands)
                {
                    GameView.SendCommand(new MoveTowerCommand(GameView.LocalUserId, from, to));
                }
                else
                {
                    Post("[이동] Legacy(Unit) 모드에서는 Move를 지원하지 않습니다.", _warnColor);
                }

                return;
            }

            if (_useTowerCommands)
            {
                GameView.SendCommand(new MergeTowerCommand(GameView.LocalUserId, from, to));
            }
            else
            {
                GameView.SendCommand(new MergeUnitCommand(GameView.LocalUserId, from, to));
            }
        }

        private bool TryGetSlotUnderPointer(out int slotIndex)
        {
            slotIndex = -1;

            var camera = _raycastCamera != null ? _raycastCamera : Camera.main;
            if (camera == null)
            {
                return false;
            }

            var screenPos = GetPointerPosition();
            var ray = camera.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out var hit, 1000f, _slotLayerMask))
            {
                return false;
            }

            var slotView = hit.collider != null
                ? hit.collider.GetComponentInParent<MergeSlotView>()
                : null;

            if (slotView == null)
            {
                return false;
            }

            slotIndex = slotView.SlotIndex;
            return true;
        }

        private static void Post(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log(message);
        }

        private static Vector2 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return Vector2.zero;
            }

            return mouse.position.ReadValue();
#else
            return Input.mousePosition;
#endif
        }

        private bool WasPressedCancel()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private static bool WasPointerDown()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool WasPointerUp()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        private static int GetPressedDigit1To9()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0;
            }

            for (var i = 0; i < 9; i++)
            {
                var key = Key.Digit1 + i;
                if (keyboard[key].wasPressedThisFrame)
                {
                    return i + 1;
                }
            }

            return 0;
#else
            for (var i = 0; i < 9; i++)
            {
                var key = (KeyCode)((int)KeyCode.Alpha1 + i);
                if (Input.GetKeyDown(key))
                {
                    return i + 1;
                }
            }

            return 0;
#endif
        }
    }
}
