using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_Ladder : MonoBehaviour
    {

        public Transform[] Points;
        [SerializeField] private Collider TopCollider = null;
        [SerializeField] private Collider BottomCollider = null;

        public bool HasPending { get; set; }
        public Transform InsertionPoint { get; set; }

        private int CurrentPoint = 0;
        public const string TopColName = "TopTrigger";
        public const string BottomColName = "BottomTrigger";
        private float LastTime = 0;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            TopCollider.name = TopColName;
            BottomCollider.name = BottomColName;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetToTop()
        {
            LastTime = Time.time;
            CurrentPoint = 2;
            HasPending = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToBottom()
        {
            CurrentPoint = 0;
            HasPending = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToMiddle()
        {
            CurrentPoint = 1;
            HasPending = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void JumpOut()
        {
            LastTime = Time.time;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 GetCurrent
        {
            get
            {
                return Points[CurrentPoint].position;
            }
        }

        public bool CanUse
        {
            get
            {
                return ((Time.time - LastTime) > 1.5f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Points == null || Points.Length <= 0) return;

            var matrix = Gizmos.matrix;
            Gizmos.color = Color.yellow;        
            if (Points.Length > 0 && Points[0] != null && BottomCollider != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(BottomCollider.bounds.center, BottomCollider.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, BottomCollider.transform.InverseTransformDirection(BottomCollider.bounds.size));
                Gizmos.matrix = matrix;
                Gizmos.DrawLine(BottomCollider.transform.position, Points[0].position);
            }
            for (int i = 0; i < Points.Length; i++)
            {
                Gizmos.DrawWireSphere(Points[i].position, 0.33f);
                if (i < Points.Length - 1)
                {
                    Gizmos.DrawLine(Points[i].position, Points[i + 1].position);
                }
            }
            if (TopCollider != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(TopCollider.bounds.center, TopCollider.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, BottomCollider.transform.InverseTransformDirection(TopCollider.bounds.size));
                Gizmos.matrix = matrix;
            }
        }
    }
}