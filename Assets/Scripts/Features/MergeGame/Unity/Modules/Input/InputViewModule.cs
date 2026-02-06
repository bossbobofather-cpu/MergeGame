using MyProject.Common.UI;
using MyProject.MergeGame.Commands;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 키보드 기반의 최소 입력 모듈입니다.
    /// 드래그/터치는 나중에 추가하고, 우선 커맨드 파이프라인을 검증하는 용도입니다.
    /// </summary>
    public sealed class InputViewModule : MergeViewModuleBase
    {
        [Header("Spawn")]
        [SerializeField] private bool _useTowerCommands = true;
        [SerializeField] private string _spawnTowerId = "unit_basic";

        [Header("Message")]
        [SerializeField] private Color _infoColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color _selectColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        [SerializeField] private Color _warnColor = new Color(1f, 0.55f, 0.1f, 0.6f);

        private int _selectedSlotIndex = -1;

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

            // Space: 자동 스폰
            if (WasPressedSpawn())
            {
                SpawnAuto();
            }

            // W: 웨이브 시작
            if (WasPressedStartWave())
            {
                GameView.SendCommand(new StartWaveCommand(GameView.LocalUserId));
            }

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


        /// <summary>
        /// UI 버튼 바인딩용: 자동 스폰을 요청합니다.
        /// </summary>
        public void OnClickSpawn()
        {
            if (GameView == null)
            {
                return;
            }

            SpawnAuto();
        }

        /// <summary>
        /// UI 버튼 바인딩용: 웨이브 시작을 요청합니다.
        /// </summary>
        public void OnClickStartWave()
        {
            if (GameView == null)
            {
                return;
            }

            GameView.SendCommand(new StartWaveCommand(GameView.LocalUserId));
        }

        /// <summary>
        /// UI 버튼 바인딩용: 슬롯 선택을 취소합니다.
        /// </summary>
        public void OnClickCancelSelection()
        {
            _selectedSlotIndex = -1;
            Post("[선택 취소]", _infoColor);
        }
        private void SpawnAuto()
        {
            if (_useTowerCommands)
            {
                if (string.IsNullOrEmpty(_spawnTowerId))
                {
                    Post("[스폰 실패] SpawnTowerId가 비어있습니다.", _warnColor);
                    return;
                }

                GameView.SendCommand(new SpawnTowerCommand(GameView.LocalUserId, _spawnTowerId));
            }
            else
            {
                GameView.SendCommand(new SpawnUnitCommand(GameView.LocalUserId));
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

        private static void Post(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log(message);
        }

        private bool WasPressedSpawn()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        private bool WasPressedStartWave()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.wKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.W);
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


