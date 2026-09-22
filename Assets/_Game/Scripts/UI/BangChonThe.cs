using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Giu thanh thanh cong qua dem thi hien 3 the cong trinh de chon 1. Game tam dung cho toi khi chon.
    /// </summary>
    public class BangChonThe : MonoBehaviour
    {
        [SerializeField] private Sprite khung;
        [SerializeField] private Font font;

        private static readonly Color Nen = new Color32(26, 30, 24, 250);
        private static readonly Color NenHiem = new Color32(46, 30, 58, 250);
        private static readonly Color Dong = new Color32(222, 171, 67, 255);
        private static readonly Color Tim = new Color32(196, 140, 255, 255);
        private static readonly Color Sang = new Color32(248, 239, 211, 255);
        private static readonly Color Nhat = new Color32(222, 214, 186, 255);

        private GameObject lopPhu;
        private RectTransform hangThe;
        private bool dangHien;

        private void Start()
        {
            if (font == null) font = Resources.Load<Font>("UI/Fonts/Inter-SemiBold");
            if (font == null) font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Semibold", "Segoe UI", "Arial" }, 24);
            Dung();
            lopPhu.SetActive(false);
            if (BattleManager.Instance != null) BattleManager.Instance.OnBattleEnded += KhiHetTran;
        }

        private void OnDestroy()
        {
            if (BattleManager.Instance != null) BattleManager.Instance.OnBattleEnded -= KhiHetTran;
        }

        private void KhiHetTran(bool songSot)
        {
            if (songSot) StartCoroutine(HienSau());
        }

        // Doi mot khung hinh de GameManager chuyen xong sang pha xay dung roi moi tam dung
        private IEnumerator HienSau()
        {
            yield return null;
            Hien();
        }

        private void Dung()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            lopPhu = new GameObject("LopPhu", typeof(RectTransform));
            lopPhu.transform.SetParent(transform, false);
            var rp = lopPhu.GetComponent<RectTransform>();
            rp.anchorMin = Vector2.zero; rp.anchorMax = Vector2.one;
            rp.offsetMin = Vector2.zero; rp.offsetMax = Vector2.zero;
            lopPhu.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);   // chan bam xuyen xuong ban do

            Chu(lopPhu.transform, "TieuDe", new Vector2(0f, 250f), new Vector2(1200f, 60f), 40, Dong, "GIỮ THÀNH THÀNH CÔNG");
            Chu(lopPhu.transform, "PhuDe", new Vector2(0f, 200f), new Vector2(1200f, 40f), 24, Nhat, "Chọn 1 thẻ công trình để thêm vào kho");

            var h = new GameObject("HangThe", typeof(RectTransform));
            h.transform.SetParent(lopPhu.transform, false);
            hangThe = h.GetComponent<RectTransform>();
            hangThe.anchorMin = hangThe.anchorMax = new Vector2(0.5f, 0.5f);
            hangThe.sizeDelta = new Vector2(1000f, 400f);
            hangThe.anchoredPosition = new Vector2(0f, -40f);
        }

        public void Hien()
        {
            var kho = KhoThe.Instance;
            if (kho == null || dangHien) return;
            var ds = kho.RutLuaChon();
            if (ds.Count == 0) return;

            foreach (Transform con in hangThe) Destroy(con.gameObject);
            float rong = 280f, cach = 36f;
            float tong = ds.Count * rong + (ds.Count - 1) * cach;
            for (int i = 0; i < ds.Count; i++)
                TaoThe(ds[i], -tong * 0.5f + rong * 0.5f + i * (rong + cach), rong);

            lopPhu.SetActive(true);
            lopPhu.transform.SetAsLastSibling();
            dangHien = true;
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(true);
        }

        private void Chon(BuildingData b)
        {
            if (KhoThe.Instance != null) KhoThe.Instance.Them(b, 1);
            lopPhu.SetActive(false);
            dangHien = false;
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        }

        private void TaoThe(BuildingData b, float x, float rong)
        {
            bool hiem = b.doHiem >= 1;
            var go = new GameObject("The_" + b.displayName, typeof(RectTransform));
            go.transform.SetParent(hangThe, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(rong, 400f);

            var nen = go.AddComponent<Image>();
            nen.sprite = khung;
            nen.type = Image.Type.Sliced;
            nen.pixelsPerUnitMultiplier = 3.125f;
            nen.color = hiem ? NenHiem : Nen;
            var vien = go.AddComponent<Outline>();
            vien.effectColor = hiem ? Tim : Dong;
            vien.effectDistance = new Vector2(3f, -3f);

            var nut = go.AddComponent<Button>();
            nut.targetGraphic = nen;
            var cb = nut.colors;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            nut.colors = cb;
            nut.onClick.AddListener(() => Chon(b));

            if (hiem) Chu(go.transform, "Hiem", new Vector2(0f, 172f), new Vector2(rong, 26f), 18, Tim, "✦ HIẾM ✦");

            var anh = new GameObject("Anh", typeof(RectTransform));
            anh.transform.SetParent(go.transform, false);
            var ra = anh.GetComponent<RectTransform>();
            ra.anchorMin = ra.anchorMax = new Vector2(0.5f, 0.5f);
            ra.anchoredPosition = new Vector2(0f, 70f);
            ra.sizeDelta = new Vector2(rong - 60f, 150f);
            var img = anh.AddComponent<Image>();
            img.sprite = b.icon != null ? b.icon : b.sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;

            Chu(go.transform, "Ten", new Vector2(0f, -28f), new Vector2(rong - 20f, 34f), 26, Sang, b.displayName);
            var mt = Chu(go.transform, "MoTa", new Vector2(0f, -76f), new Vector2(rong - 36f, 56f), 16, Nhat, b.description);
            mt.horizontalOverflow = HorizontalWrapMode.Wrap;

            string dong;
            if (b.linhSanXuat != null) dong = "Huấn luyện: " + b.linhSanXuat.displayName + " / " + b.thoiGianRaLinh + "s";
            else if (b.producesResource) dong = "Sản xuất: " + TenTaiNguyen(b.output) + " x" + b.outputAmount + " / " + b.productionInterval + "s";
            else dong = "Hỗ trợ, không sản xuất";
            Chu(go.transform, "SanXuat", new Vector2(0f, -122f), new Vector2(rong - 20f, 26f), 17, Dong, dong);

            int dangCo = KhoThe.Instance != null ? KhoThe.Instance.SoThe(b) : 0;
            Chu(go.transform, "Khu", new Vector2(0f, -156f), new Vector2(rong - 20f, 24f), 15, Nhat,
                "Khu: " + TenKhu(b.khuYeuCau) + "   ·   Đang có: x" + dangCo);
        }

        private Text Chu(Transform cha, string ten, Vector2 pos, Vector2 co, int size, Color mau, string noiDung)
        {
            var go = new GameObject(ten, typeof(RectTransform));
            go.transform.SetParent(cha, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = co;
            var t = go.AddComponent<Text>();
            t.font = font; t.fontSize = size; t.color = mau; t.text = noiDung;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var bong = go.AddComponent<Shadow>();
            bong.effectColor = new Color(0f, 0f, 0f, 0.8f);
            bong.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        private static string TenKhu(LoaiKhuDat k)
        {
            switch (k)
            {
                case LoaiKhuDat.Nui: return "Núi";
                case LoaiKhuDat.Rung: return "Rừng";
                case LoaiKhuDat.VenBien: return "Ven biển";
                default: return "Đồng bằng";
            }
        }

        private static string TenTaiNguyen(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Gold: return "Vàng";
                case ResourceType.Gem: return "Ngọc";
                case ResourceType.Wood: return "Gỗ";
                case ResourceType.Stone: return "Đá";
                case ResourceType.Food: return "Lương thực";
                case ResourceType.Army: return "Quân số";
                default: return t.ToString();
            }
        }
    }
}
