using UnityEngine;

namespace KingdomRuins
{
    /// <summary>Mot hat hieu ung ngan: khoi, bui, lap lanh. Troi, to dan, mo dan roi tu huy.</summary>
    public class HatHieuUng : MonoBehaviour
    {
        public Vector3 vanToc;
        public float tuoiTho = 1f;
        public float toDan;
        public Color mau = Color.white;

        private SpriteRenderer sr;
        private Vector3 coDau;
        private float t;

        private void Start() { sr = GetComponent<SpriteRenderer>(); coDau = transform.localScale; }

        private void Update()
        {
            t += Time.deltaTime;
            float p = t / tuoiTho;
            if (p >= 1f) { Destroy(gameObject); return; }
            transform.position += vanToc * Time.deltaTime;
            transform.localScale = coDau * (1f + toDan * p);
            if (sr != null) { var c = mau; c.a = mau.a * (1f - p * p); sr.color = c; }
        }

        public static void Tao(Sprite[] khung, Vector3 viTri, float co, Vector3 vanToc, float tuoiTho,
                               Color mau, float toDan, Material vl, int thuTu, Transform cha)
        {
            if (khung == null || khung.Length == 0) return;
            var go = new GameObject("Hat");
            if (cha != null) go.transform.SetParent(cha, false);
            go.transform.position = viTri;
            go.transform.localScale = Vector3.one * co;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = khung[0];
            sr.color = mau;
            sr.sortingOrder = thuTu;
            if (vl != null) sr.sharedMaterial = vl;
            var hh = go.AddComponent<HoatHinhKhung>();
            hh.theoNhipGame = false;
            hh.Dat(khung, khung.Length / Mathf.Max(0.1f, tuoiTho), false);
            var h = go.AddComponent<HatHieuUng>();
            h.vanToc = vanToc; h.tuoiTho = tuoiTho; h.mau = mau; h.toDan = toDan;
        }
    }
}
