using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 몬스터 뷰 상태(Idle/Move)를 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewState : MonoBehaviour
    {
        [SerializeField] private MonsterVisualState _state = MonsterVisualState.Idle;
        [SerializeField] private float _moveStateTimeout = 0.2f;
        [SerializeField] private float _moveScaleMultiplier = 1.03f;

        private float _moveTimer;
        private Vector3 _baseScale = Vector3.one;

        /// <summary>
        /// 현재 상태입니다.
        /// </summary>
        public MonsterVisualState State => _state;

        /// <summary>
        /// 기본 스케일을 설정합니다.
        /// </summary>
        public void SetBaseScale(Vector3 baseScale)
        {
            _baseScale = baseScale;
            if (_state == MonsterVisualState.Idle)
            {
                transform.localScale = _baseScale;
            }
        }

        /// <summary>
        /// 이동 상태로 표시합니다.
        /// </summary>
        public void MarkMoving()
        {
            _state = MonsterVisualState.Move;
            _moveTimer = _moveStateTimeout;
            transform.localScale = _baseScale * _moveScaleMultiplier;
        }

        private void Update()
        {
            if (_state != MonsterVisualState.Move)
            {
                return;
            }

            _moveTimer -= Time.deltaTime;
            if (_moveTimer <= 0f)
            {
                _state = MonsterVisualState.Idle;
                transform.localScale = _baseScale;
            }
        }
    }

    /// <summary>
    /// 몬스터 뷰 상태 타입입니다.
    /// </summary>
    public enum MonsterVisualState
    {
        Idle,
        Move
    }
}
