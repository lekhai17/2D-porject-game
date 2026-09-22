using UnityEngine;

namespace KingdomRuins
{
    /// <summary>Di tu diem A toi diem B, mo dan o doan cuoi roi tu huy. Dung cho nguoi khieng hang.</summary>
    public class DiChuyenBienMat : MonoBehaviour
    {
        public Vector3 tu, den;
        public float thoiGian = 1.6f;
        public float doanMo = 0.35f;

        private SpriteRenderer sr;
        private float t;

        private void Start() { sr = GetComponent<SpriteRenderer>(); }

        private void Update()
        {
            t += Time.deltaTime;
            float p = t / thoiGian;
            if (p >= 1f) { Destroy(gameObject); return; }
            transform.position = Vector3.Lerp(tu, den, p);
            if (sr != null && p > 1f - doanMo)
            {
                var c = sr.color; c.a = (1f - p) / doanMo; sr.color = c;
            }
        }
    }
}
