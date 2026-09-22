using UnityEngine;

namespace KingdomRuins
{
    /// <summary>So "+N" noi len tren cong trinh khi ra hang.</summary>
    public class ChuNoiLen : MonoBehaviour
    {
        private static Font font;
        private TextMesh chu, bong;
        private Color mau;
        private float t;
        private const float ThoiGian = 1.3f;

        public static void Tao(string noiDung, Color mau, Vector3 viTri, Transform cha)
        {
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Franklin Gothic Heavy", "Impact", "Arial Black", "Arial" }, 64);

            var go = new GameObject("ChuNoiLen");
            if (cha != null) go.transform.SetParent(cha, false);
            go.transform.position = viTri;

            var c = go.AddComponent<ChuNoiLen>();
            c.mau = mau;
            c.bong = TaoChu(go.transform, noiDung, new Color(0.05f, 0.03f, 0.01f, 0.9f), new Vector3(0.05f, -0.05f, 0f), ThuTuVe.TrenCung);
            c.chu = TaoChu(go.transform, noiDung, mau, Vector3.zero, ThuTuVe.TrenCung + 1);
        }

        private static TextMesh TaoChu(Transform cha, string s, Color mau, Vector3 lech, int thuTu)
        {
            var go = new GameObject("Chu");
            go.transform.SetParent(cha, false);
            go.transform.localPosition = lech;
            var tm = go.AddComponent<TextMesh>();
            tm.font = font;
            tm.text = s;
            tm.fontSize = 64;
            tm.characterSize = 0.055f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = mau;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = font.material;
            mr.sortingOrder = thuTu;
            return tm;
        }

        private void Update()
        {
            t += Time.deltaTime;
            float p = t / ThoiGian;
            if (p >= 1f) { Destroy(gameObject); return; }
            transform.position += Vector3.up * (0.75f * Time.deltaTime);
            float a = p < 0.6f ? 1f : 1f - (p - 0.6f) / 0.4f;
            if (chu != null) { var c = mau; c.a = a; chu.color = c; }
            if (bong != null) { var c = bong.color; c.a = 0.9f * a; bong.color = c; }
        }
    }
}
