using UnityEngine;

namespace KingdomRuins
{
    /// <summary>Lat sprite theo khung hinh. Dung nhip game nen bam tam dung la dung.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HoatHinhKhung : MonoBehaviour
    {
        public Sprite[] khung;
        public float fps = 10f;
        public bool lap = true;
        public bool theoNhipGame = true;

        private SpriteRenderer sr;
        private float t;
        private int i;

        private void Awake() { sr = GetComponent<SpriteRenderer>(); }

        /// <summary>Doi bo khung. Goi lai voi cung bo khung thi khong reset.</summary>
        public void Dat(Sprite[] k, float f, bool l)
        {
            fps = f; lap = l;
            if (k == khung) return;
            khung = k; t = 0f; i = 0; enabled = true;
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr != null && k != null && k.Length > 0) sr.sprite = k[0];
        }

        private void Update()
        {
            if (khung == null || khung.Length == 0 || sr == null) return;
            var gm = GameManager.Instance;
            float dt = theoNhipGame && gm != null ? gm.ScaledDelta : Time.deltaTime;
            t += dt;
            float buoc = 1f / Mathf.Max(0.01f, fps);
            while (t >= buoc)
            {
                t -= buoc;
                i++;
                if (i >= khung.Length)
                {
                    if (lap) i = 0;
                    else { i = khung.Length - 1; enabled = false; break; }
                }
            }
            sr.sprite = khung[i];
        }
    }
}
