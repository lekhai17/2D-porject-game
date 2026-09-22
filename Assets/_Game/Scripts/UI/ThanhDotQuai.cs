using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Thanh du bao dot quai duoi HUD: dem nay va cac dem toi,
    /// chan dung tung loai quai, so luong, cap, nhan TRUM. The dem nay co dem nguoc toi gio tan cong.
    /// </summary>
    public class ThanhDotQuai : MonoBehaviour
    {
        [SerializeField] private Sprite khung;
        [SerializeField] private int soDem = 3;
        [Tooltip("Quai co mau tu muc nay tro len duoc gan nhan TRUM")]
        [SerializeField] private int nguongTrum = 80;

        [SerializeField] private Color mauNen = new Color(0.16f, 0.11f, 0.06f, 0.95f);
        [SerializeField] private Color mauNenNay = new Color(0.30f, 0.16f, 0.08f, 0.97f);
        [SerializeField] private Color chuSang = new Color32(0xFF, 0xF4, 0xD8, 255);
        [SerializeField] private Color chuNhat = new Color32(0xE2, 0xCD, 0xA0, 255);
        [SerializeField] private Color mauDo = new Color32(0xE8, 0x5A, 0x48, 255);

        private class The
        {
            public Image nen;
            public Text tieuDe;
            public RectTransform hang;
            public Image thanhDay;
            public Text chuThoiGian;
        }

        private Font fontTen, fontSo;
        private readonly List<The> cacThe = new List<The>();
        private int ngayDaVe = -1;
        private GamePhase phaDaVe;
        private static readonly Dictionary<Sprite, Sprite> chanDung = new Dictionary<Sprite, Sprite>();

        private void Start()
        {
            fontTen = ChuUI.Dam;
            fontSo = ChuUI.Thuong;
            Dung();
            VeLai();
        }

        private void Dung()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -158f);
            rt.sizeDelta = new Vector2(924f, 132f);

            float x = 0f;
            for (int i = 0; i < soDem; i++)
            {
                float w = i == 0 ? 360f : 270f;
                cacThe.Add(TaoThe(x, w, i == 0));
                x += w + 12f;
            }
        }

        private The TaoThe(float x, float w, bool laDemNay)
        {
            var go = new GameObject("The", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(w, laDemNay ? 132f : 112f);

            var t = new The();
            t.nen = go.AddComponent<Image>();
            t.nen.sprite = khung;
            t.nen.type = Image.Type.Sliced;
            t.nen.pixelsPerUnitMultiplier = 3.125f;
            t.nen.color = laDemNay ? mauNenNay : mauNen;

            t.tieuDe = Chu(go.transform, "TieuDe", new Vector2(14f, -9f), new Vector2(w - 28f, 26f), fontTen, 20, chuSang, TextAnchor.UpperLeft);

            var h = new GameObject("Hang", typeof(RectTransform));
            h.transform.SetParent(go.transform, false);
            t.hang = h.GetComponent<RectTransform>();
            t.hang.anchorMin = t.hang.anchorMax = new Vector2(0f, 1f);
            t.hang.pivot = new Vector2(0f, 1f);
            t.hang.anchoredPosition = new Vector2(12f, -38f);
            t.hang.sizeDelta = new Vector2(w - 24f, 52f);

            if (laDemNay)
            {
                var nenThanh = new GameObject("NenThanh", typeof(RectTransform));
                nenThanh.transform.SetParent(go.transform, false);
                var rn = nenThanh.GetComponent<RectTransform>();
                rn.anchorMin = rn.anchorMax = new Vector2(0f, 0f);
                rn.pivot = new Vector2(0f, 0f);
                rn.anchoredPosition = new Vector2(14f, 10f);
                rn.sizeDelta = new Vector2(w - 28f, 12f);
                nenThanh.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.02f, 0.9f);

                var day = new GameObject("Day", typeof(RectTransform));
                day.transform.SetParent(nenThanh.transform, false);
                var rd = day.GetComponent<RectTransform>();
                rd.anchorMin = Vector2.zero; rd.anchorMax = Vector2.one;
                rd.offsetMin = new Vector2(2f, 2f); rd.offsetMax = new Vector2(-2f, -2f);
                t.thanhDay = day.AddComponent<Image>();
                t.thanhDay.color = mauDo;
                t.thanhDay.type = Image.Type.Filled;
                t.thanhDay.fillMethod = Image.FillMethod.Horizontal;

                t.chuThoiGian = Chu(go.transform, "ThoiGian", new Vector2(14f, -92f), new Vector2(w - 28f, 24f), fontSo, 20, chuNhat, TextAnchor.UpperLeft);
            }
            return t;
        }

        private Text Chu(Transform cha, string ten, Vector2 pos, Vector2 co, Font f, int size, Color mau, TextAnchor canh)
        {
            var go = new GameObject(ten, typeof(RectTransform));
            go.transform.SetParent(cha, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = co;
            var t = go.AddComponent<Text>();
            t.font = f; t.fontSize = size; t.color = mau; t.alignment = canh;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            ChuUI.ApDung(t, f == fontTen);
            return t;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.DayNumber != ngayDaVe || gm.Phase != phaDaVe) VeLai();

            if (cacThe.Count == 0 || cacThe[0].thanhDay == null) return;
            var t0 = cacThe[0];
            if (gm.Phase == GamePhase.Kingdom && gm.KingdomPhaseDuration > 0f)
            {
                float con = Mathf.Max(0f, gm.PhaseTimeRemaining);
                t0.thanhDay.fillAmount = 1f - con / gm.KingdomPhaseDuration;
                int s = Mathf.CeilToInt(con);
                t0.chuThoiGian.text = "Tan cong sau " + (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
            }
            else if (gm.Phase == GamePhase.Battle)
            {
                t0.thanhDay.fillAmount = 1f;
                int con = BattleManager.Instance != null ? BattleManager.Instance.InvadersAlive : 0;
                t0.chuThoiGian.text = "DANG TAN CONG  -  con " + con;
            }
        }

        private void VeLai()
        {
            var gm = GameManager.Instance;
            var bm = BattleManager.Instance;
            if (gm == null || bm == null) return;
            ngayDaVe = gm.DayNumber;
            phaDaVe = gm.Phase;

            for (int i = 0; i < cacThe.Count; i++)
            {
                var t = cacThe[i];
                int ngay = gm.DayNumber + i;
                var ds = bm.DuBaoDot(ngay);
                int cap = bm.CapQuaiTheoNgay(ngay);
                bool coTrum = false;
                foreach (var kv in ds) if (kv.Key.maxHp >= nguongTrum) coTrum = true;

                t.tieuDe.text = (i == 0 ? "DEM NAY  -  " : "") + "DEM " + ngay + "   Lv " + cap + (coTrum ? "   TRUM!" : "");
                t.tieuDe.color = coTrum ? mauDo : chuSang;

                foreach (Transform con in t.hang) Destroy(con.gameObject);
                float x = 0f;
                foreach (var kv in ds)
                {
                    var o = new GameObject("Quai", typeof(RectTransform));
                    o.transform.SetParent(t.hang, false);
                    var ro = o.GetComponent<RectTransform>();
                    ro.anchorMin = ro.anchorMax = new Vector2(0f, 1f);
                    ro.pivot = new Vector2(0f, 1f);
                    ro.anchoredPosition = new Vector2(x, 0f);
                    ro.sizeDelta = new Vector2(52f, 52f);
                    var img = o.AddComponent<Image>();
                    img.sprite = ChanDung(kv.Key.icon);
                    img.preserveAspect = true;
                    img.raycastTarget = false;
                    if (kv.Key.maxHp >= nguongTrum)
                    {
                        var vien = o.AddComponent<Outline>();
                        vien.effectColor = mauDo;
                        vien.effectDistance = new Vector2(2f, -2f);
                    }
                    Chu(t.hang, "SoLuong", new Vector2(x + 52f, -13f), new Vector2(36f, 28f), fontSo, 23,
                        kv.Key.maxHp >= nguongTrum ? mauDo : chuSang, TextAnchor.UpperLeft).text = "x" + kv.Value;
                    x += 94f;
                }
            }
        }

        /// <summary>Cat phan than nhan vat o giua khung 192px cho chan dung khong bi nho xiu.</summary>
        private static Sprite ChanDung(Sprite s)
        {
            if (s == null) return null;
            if (chanDung.TryGetValue(s, out var c) && c != null) return c;
            var r = s.rect;
            var cat = new Rect(r.x + r.width * 0.28f, r.y + r.height * 0.22f, r.width * 0.44f, r.height * 0.50f);
            c = Sprite.Create(s.texture, cat, new Vector2(0.5f, 0.5f), s.pixelsPerUnit);
            chanDung[s] = c;
            return c;
        }
    }
}
