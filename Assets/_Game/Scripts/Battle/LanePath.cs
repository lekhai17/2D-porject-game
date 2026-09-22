using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Duong di cua quan xam nhap trong Battle View. Dat cac Transform con
    /// lam waypoint theo thu tu tu cong vao den loi ham nguc.
    /// </summary>
    public class LanePath : MonoBehaviour
    {
        [Tooltip("Cac diem moc theo thu tu. De trong se tu lay cac Transform con.")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        private void Awake()
        {
            if (waypoints.Count == 0)
                foreach (Transform child in transform)
                    waypoints.Add(child);
        }

        public int Count => waypoints.Count;

        public Vector3 Start => waypoints.Count > 0 ? waypoints[0].position : transform.position;

        public Vector3 End => waypoints.Count > 0 ? waypoints[waypoints.Count - 1].position : transform.position;

        public Vector3 GetPoint(int index)
        {
            if (waypoints.Count == 0) return transform.position;
            index = Mathf.Clamp(index, 0, waypoints.Count - 1);
            return waypoints[index].position;
        }

        public bool IsLastIndex(int index) => index >= waypoints.Count - 1;

        private void OnDrawGizmos()
        {
            var pts = waypoints.Count > 0 ? waypoints : null;
            if (pts == null) return;

            Gizmos.color = Color.magenta;
            for (int i = 0; i < pts.Count; i++)
            {
                if (pts[i] == null) continue;
                Gizmos.DrawSphere(pts[i].position, 0.15f);
                if (i + 1 < pts.Count && pts[i + 1] != null)
                    Gizmos.DrawLine(pts[i].position, pts[i + 1].position);
            }
        }
    }
}
