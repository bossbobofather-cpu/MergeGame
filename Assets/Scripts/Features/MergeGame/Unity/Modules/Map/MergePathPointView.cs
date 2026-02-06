using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// ?? ????? ???? ???? ? ???????.
    /// Host?? ??? (PathIndex, WaypointIndex)? ?????.
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

