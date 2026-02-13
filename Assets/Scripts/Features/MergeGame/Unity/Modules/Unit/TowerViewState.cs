using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 타워 뷰 상태(Idle/Attack)를 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TowerViewState : MonoBehaviour
    {
        [SerializeField] private TowerVisualState _state = TowerVisualState.Idle;
        [SerializeField] private float _attackDuration = 0.15f;
        [SerializeField] private float _attackScaleMultiplier = 1.1f;

        private float _attackTimer;
        private Vector3 _baseScale = Vector3.one;

        /// <summary>
        /// 현재 상태입니다.
        /// </summary>
        public TowerVisualState State => _state;

        /// <summary>
        /// 공격 상태로 전환합니다.
        /// </summary>
        public void TriggerAttack(float? duration = null)
        {
            // 핵심 로직을 처리합니다.
            _state = TowerVisualState.Attack;
            _attackTimer = duration ?? _attackDuration;
            transform.localScale = _baseScale * _attackScaleMultiplier;
        }
        /// <summary>
        /// Update 함수를 처리합니다.
        /// </summary>

        private void Update()
        {
            // 핵심 로직을 처리합니다.
            if (_state != TowerVisualState.Attack)
            {
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _state = TowerVisualState.Idle;
                transform.localScale = _baseScale;
            }
        }
    }

    /// <summary>
    /// 타워 뷰 상태 타입입니다.
    /// </summary>
    public enum TowerVisualState
    {
        Idle,
        Attack
    }
}
