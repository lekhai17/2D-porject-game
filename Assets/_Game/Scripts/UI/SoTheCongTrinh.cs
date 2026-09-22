using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    // Nut va bang So The: tam bang co khung o giua man hinh, ben trong la luoi moi cong trinh trong game.
    // The da kham pha hien anh mau va thong tin, the chua co hien bong den kem dau hoi.
    public class SoTheCongTrinh : MonoBehaviour
    {
        [SerializeField] private Sprite khung;
        [SerializeField] private Font font;
        [SerializeField] private Sprite hinhTrangTri;   // thap canh hai ben tieu de
        [SerializeField] private int soCot = 6;

        private static readonly Color NenBang = new Color32(20, 27, 21, 250);
        private static readonly Color NenTieuDe = new Color32(12, 17, 13, 255);
        private static readonly Color NenThe = new Color32(30, 38, 29, 255);
        private static readonly Color NenHiem = new Color32(46, 30, 58, 255);
        private static readonly Color NenAn = new Color32(16, 17, 18, 255);
        private static readonly Color Dong = new Color32(222, 171, 67, 255);
        private static readonly Color DongToi = new Color32(122, 88, 34, 255);
        private static readonly Color Tim = new Color32(196, 140, 255, 255);
        private static readonly Color Sang = new Color32(248, 239, 211, 255);
        private static readonly Color Nhat = new Color32(222, 214, 186, 255);
        private static readonly Color Xam = new Color32(125, 122, 112, 255);

        private const float RongBang = 1260f, CaoBang = 790f, CaoTieuDe = 110f;

        private GameObject lopPhu;
        private RectTransform bang, luoi, thanhTienDo;
        private Text tieuDe, chuTienDo;
        private bool truocDoDangDung;

        private void Start()
        {
            if (font == null) font = Resources.Load<Font>("UI/Fonts/Inter-SemiBold");
            if (font == null) font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Semibold", "Arial" }, 24);
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            TaoNutMo();
            TaoBang();
            lopPhu.SetActive(false);
        }

        private void Update()
        {
            var bp = UnityEngine.InputSystem.Keyboard.current;
            if (lopPhu != null && lopPhu.activeSelf && bp != null && bp.escapeKey.wasPressedThisFrame) DongBang();
        }

        // ================= Nut mo =================
        private void TaoNutMo()
        {
            var nut = TaoNut(transform, "NutSoThe", new Vector2(288f, 58f), new Color32(52, 38, 20, 250), Dong, "SỔ THẺ CÔNG TRÌNH", 22, Mo);
            var rt = nut.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-24f, 302f);
        }

        // ================= Tam bang =================
        private void TaoBang()
        {
            lopPhu = O(transform, "LopPhu");
            var rp = lopPhu.GetComponent<RectTransform>();
            rp.anchorMin = Vector2.zero; rp.anchorMax = Vector2.one;
            rp.offsetMin = Vector2.zero; rp.offsetMax = Vector2.zero;
            lopPhu.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

            // Than bang: nen, vien dong ngoai, vien trong mo
            var goBang = O(lopPhu.transform, "Bang");
            bang = goBang.GetComponent<RectTransform>();
            Dat(bang, Vector2.zero, new Vector2(RongBang, CaoBang));
            var nen = Anh(goBang, NenBang);
            var vienNgoai = goBang.AddComponent<Outline>();
            vienNgoai.effectColor = Dong; vienNgoai.effectDistance = new Vector2(3f, -3f);
            var bong = goBang.AddComponent<Shadow>();
            bong.effectColor = new Color(0f, 0f, 0f, 0.7f); bong.effectDistance = new Vector2(8f, -8f);

            var vienTrong = O(bang, "VienTrong");
            var rv = vienTrong.GetComponent<RectTransform>();
            rv.anchorMin = Vector2.zero; rv.anchorMax = Vector2.one;
            rv.offsetMin = new Vector2(12f, 12f); rv.offsetMax = new Vector2(-12f, -12f);
            var anhVien = vienTrong.AddComponent<Image>();
            anhVien.color = new Color(0f, 0f, 0f, 0f);
            anhVien.raycastTarget = false;
            var ov = vienTrong.AddComponent<Outline>();
            ov.effectColor = DongToi; ov.effectDistance = new Vector2(2f, -2f);

            // Hoa van dong o bon goc
            foreach (var g in new[] { new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f) })
            {
                var hv = O(bang, "HoaVan");
                var rh = hv.GetComponent<RectTransform>();
                Dat(rh, new Vector2(g.x * (RongBang * 0.5f - 12f), g.y * (CaoBang * 0.5f - 12f)), new Vector2(20f, 20f));
                rh.localRotation = Quaternion.Euler(0f, 0f, 45f);
                Anh(hv, Dong).raycastTarget = false;
            }

            // Dai tieu de
            var dai = O(bang, "DaiTieuDe");
            var rd = dai.GetComponent<RectTransform>();
            Dat(rd, new Vector2(0f, CaoBang * 0.5f - 12f - CaoTieuDe * 0.5f), new Vector2(RongBang - 24f, CaoTieuDe));
            Anh(dai, NenTieuDe).raycastTarget = false;
            var gach = O(dai.transform, "GachDuoi");
            Dat(gach.GetComponent<RectTransform>(), new Vector2(0f, -CaoTieuDe * 0.5f), new Vector2(RongBang - 24f, 3f));
            Anh(gach, Dong).raycastTarget = false;

            // Thap canh hai ben tieu de
            if (hinhTrangTri != null)
                foreach (float x in new[] { -1f, 1f })
                {
                    var th = O(dai.transform, "TrangTri");
                    Dat(th.GetComponent<RectTransform>(), new Vector2(x * (RongBang * 0.5f - 90f), 4f), new Vector2(84f, 100f));
                    var ai = th.AddComponent<Image>();
                    ai.sprite = hinhTrangTri; ai.preserveAspect = true; ai.raycastTarget = false;
                }

            tieuDe = Chu(dai.transform, "TieuDe", new Vector2(0f, 22f), new Vector2(800f, 48f), 40, Dong, "SỔ THẺ CÔNG TRÌNH");

            // Thanh tien do kham pha
            var nenThanh = O(dai.transform, "NenTienDo");
            Dat(nenThanh.GetComponent<RectTransform>(), new Vector2(-60f, -26f), new Vector2(420f, 14f));
            Anh(nenThanh, new Color32(6, 8, 6, 255)).raycastTarget = false;
            var oVien = nenThanh.AddComponent<Outline>(); oVien.effectColor = DongToi; oVien.effectDistance = new Vector2(1f, -1f);
            var day = O(nenThanh.transform, "Day");
            thanhTienDo = day.GetComponent<RectTransform>();
            thanhTienDo.anchorMin = new Vector2(0f, 0f); thanhTienDo.anchorMax = new Vector2(0f, 1f);
            thanhTienDo.pivot = new Vector2(0f, 0.5f);
            thanhTienDo.offsetMin = new Vector2(2f, 2f); thanhTienDo.offsetMax = new Vector2(2f, -2f);
            Anh(day, Dong).raycastTarget = false;
            chuTienDo = Chu(dai.transform, "ChuTienDo", new Vector2(250f, -26f), new Vector2(240f, 26f), 19, Nhat, "");
            chuTienDo.alignment = TextAnchor.MiddleLeft;

            // Luoi the
            var g2 = O(bang, "Luoi");
            luoi = g2.GetComponent<RectTransform>();
            Dat(luoi, new Vector2(0f, -8f), Vector2.zero);

            // Chan bang: goi y va nut dong
            Chu(bang, "GoiY", new Vector2(0f, -CaoBang * 0.5f + 88f), new Vector2(900f, 26f), 18, Nhat,
                "Giữ thành qua mỗi đêm để được chọn thêm thẻ mới");
            var nutDong = TaoNut(bang, "NutDong", new Vector2(220f, 52f), new Color32(214, 145, 48, 255),
                                 new Color32(34, 22, 10, 255), "ĐÓNG", 24, DongBang);
            Dat(nutDong.GetComponent<RectTransform>(), new Vector2(0f, -CaoBang * 0.5f + 44f), new Vector2(220f, 52f));

            var nutX = TaoNut(bang, "NutX", new Vector2(46f, 46f), new Color32(52, 38, 20, 255), Dong, "X", 24, DongBang);
            Dat(nutX.GetComponent<RectTransform>(), new Vector2(RongBang * 0.5f - 44f, CaoBang * 0.5f - 44f), new Vector2(46f, 46f));
        }

        // ================= Mo / dong =================
        public void Mo()
        {
            var kho = KhoThe.Instance;
            if (kho == null) return;
            foreach (Transform con in luoi) Destroy(con.gameObject);

            var ds = kho.TatCa;
            float w = 182f, h = 250f, cach = 14f;
            int cot = Mathf.Min(soCot, Mathf.Max(1, ds.Count));
            int hang = Mathf.CeilToInt(ds.Count / (float)cot);
            luoi.sizeDelta = new Vector2(cot * w + (cot - 1) * cach, hang * h + (hang - 1) * cach);

            for (int i = 0; i < ds.Count; i++)
            {
                int c = i % cot, r = i / cot;
                int soO = Mathf.Min(cot, ds.Count - r * cot);
                float rongHang = soO * w + (soO - 1) * cach;
                float x = -rongHang * 0.5f + w * 0.5f + c * (w + cach);
                float y = luoi.sizeDelta.y * 0.5f - h * 0.5f - r * (h + cach);
                TaoThe(ds[i], new Vector2(x, y), w, h, kho);
            }

            float tiLe = ds.Count > 0 ? kho.SoDaKhamPha / (float)ds.Count : 0f;
            thanhTienDo.sizeDelta = new Vector2(Mathf.Max(0f, (420f - 4f) * tiLe), thanhTienDo.sizeDelta.y);
            chuTienDo.text = "Đã khám phá " + kho.SoDaKhamPha + " / " + ds.Count;

            lopPhu.SetActive(true);
            var gm = GameManager.Instance;
            if (gm != null) { truocDoDangDung = gm.IsPaused; gm.SetPaused(true); }
        }

        private void DongBang()
        {
            lopPhu.SetActive(false);
            var gm = GameManager.Instance;
            if (gm != null) gm.SetPaused(truocDoDangDung);
        }

        // ================= Mot the =================
        private void TaoThe(BuildingData b, Vector2 pos, float w, float h, KhoThe kho)
        {
            if (b == null) return;
            bool biet = kho.DaKhamPha(b);
            bool hiem = b.doHiem >= 1;

            var go = O(luoi, "The");
            Dat(go.GetComponent<RectTransform>(), pos, new Vector2(w, h));
            var nen = go.AddComponent<Image>();
            nen.sprite = khung; nen.type = Image.Type.Sliced; nen.pixelsPerUnitMultiplier = 3.125f;
            nen.color = biet ? (hiem ? NenHiem : NenThe) : NenAn;
            var vien = go.AddComponent<Outline>();
            vien.effectColor = biet ? (hiem ? Tim : Dong) : Xam;
            vien.effectDistance = new Vector2(2f, -2f);

            // Khung anh sang hon de hinh noi len
            var khungAnh = O(go.transform, "KhungAnh");
            Dat(khungAnh.GetComponent<RectTransform>(), new Vector2(0f, 44f), new Vector2(w - 20f, 128f));
            Anh(khungAnh, biet ? new Color(0f, 0f, 0f, 0.28f) : new Color(0f, 0f, 0f, 0.4f)).raycastTarget = false;

            var anh = O(go.transform, "Anh");
            Dat(anh.GetComponent<RectTransform>(), new Vector2(0f, 44f), new Vector2(w - 34f, 116f));
            var img = anh.AddComponent<Image>();
            img.sprite = b.icon != null ? b.icon : b.sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = biet ? Color.white : new Color(0.02f, 0.02f, 0.03f, 0.92f);

            if (!biet)
            {
                Chu(go.transform, "Hoi", new Vector2(0f, 44f), new Vector2(w, 100f), 76, Dong, "?");
                Chu(go.transform, "Ten", new Vector2(0f, -50f), new Vector2(w - 12f, 30f), 24, Xam, "???");
                Chu(go.transform, "GoiY", new Vector2(0f, -86f), new Vector2(w - 16f, 24f), 16, Xam, "Chưa khám phá");
                return;
            }

            if (hiem)
            {
                var nhan = O(go.transform, "NhanHiem");
                Dat(nhan.GetComponent<RectTransform>(), new Vector2(0f, h * 0.5f - 14f), new Vector2(76f, 22f));
                Anh(nhan, Tim).raycastTarget = false;
                Chu(nhan.transform, "Chu", Vector2.zero, new Vector2(76f, 22f), 14, new Color32(30, 16, 40, 255), "HIẾM");
            }

            Chu(go.transform, "Ten", new Vector2(0f, -38f), new Vector2(w - 12f, 30f), 23, Sang, b.displayName);

            string dong;
            if (b.linhSanXuat != null) dong = "Lính: " + b.linhSanXuat.displayName;
            else if (b.producesResource) dong = TenTaiNguyen(b.output) + " x" + b.outputAmount + " / " + b.productionInterval + "s";
            else dong = "Hỗ trợ";
            Chu(go.transform, "SanXuat", new Vector2(0f, -70f), new Vector2(w - 12f, 24f), 17, Dong, dong);

            int soCo = kho.SoThe(b);
            Chu(go.transform, "SoLuong", new Vector2(0f, -100f), new Vector2(w - 12f, 24f), 17, soCo > 0 ? Sang : Xam, "Đang có: x" + soCo);
        }

        // ================= Tien ich =================
        private static GameObject O(Transform cha, string ten)
        {
            var go = new GameObject(ten, typeof(RectTransform));
            go.transform.SetParent(cha, false);
            return go;
        }

        private static void Dat(RectTransform rt, Vector2 pos, Vector2 co)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = co;
        }

        private Image Anh(GameObject go, Color mau)
        {
            var i = go.AddComponent<Image>();
            i.sprite = khung; i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = 3.125f;
            i.color = mau;
            return i;
        }

        private GameObject TaoNut(Transform cha, string ten, Vector2 co, Color nen, Color mauChu, string noiDung, int size,
                                  UnityEngine.Events.UnityAction khiBam)
        {
            var go = O(cha, ten);
            Dat(go.GetComponent<RectTransform>(), Vector2.zero, co);
            var img = Anh(go, nen);
            var ov = go.AddComponent<Outline>(); ov.effectColor = DongToi; ov.effectDistance = new Vector2(2f, -2f);
            var b = go.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(khiBam);
            Chu(go.transform, "Chu", Vector2.zero, co, size, mauChu, noiDung);
            return go;
        }

        private Text Chu(Transform cha, string ten, Vector2 pos, Vector2 co, int size, Color mau, string noiDung)
        {
            var go = O(cha, ten);
            Dat(go.GetComponent<RectTransform>(), pos, co);
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

        private static string TenTaiNguyen(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Gold: return "Vàng";
                case ResourceType.Gem: return "Ngọc";
                case ResourceType.Wood: return "Gỗ";
                case ResourceType.Stone: return "Đá";
                case ResourceType.Food: return "Lương thực";
                default: return t.ToString();
            }
        }
    }
}
