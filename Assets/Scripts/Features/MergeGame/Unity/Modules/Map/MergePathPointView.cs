using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 경로 포인트 오브젝트에 부착되는 뷰 컴포넌트입니다.
    /// Host에서 전달받은 (PathIndex, WaypointIndex)를 보관합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MergePathPointView : MonoBehaviour
    {
        [SerializeField] private int _pathIndex = -1;
        [SerializeField] private int _waypointIndex = -1;

        public int PathIndex => _pathIndex;
        public int WaypointIndex => _waypointIndex;

        public void SetIndices(int pathIndex, int waypointIndex)
        {
            _pathIndex = pathIndex;
            _waypointIndex = waypointIndex;
        }
    }
}
