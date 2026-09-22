using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Hang nut chon cong trinh o day man hinh.
    /// Bam mot nut la vao che do dat: di chuot thay bong cong trinh, bam trai de xay.
    /// Nut nao khong du tai nguyen thi mo di va khong bam duoc.
    /// </summary>
    public class BangChonCongTrinh : MonoBehaviour
    {
        [Header("Danh sach cong trinh")]
        [SerializeField] private List<BuildingData> danhSach = new List<BuildingData>();

        [Header("Hinh nen")]
        [SerializeField] private Sprite khungNut;

        [Header("Kich thuoc")]
        [SerializeField] private int rongNut = 144;
        [SerializeField] private int caoNut = 152;
        [SerializeField] private int demGiua = 10;

        [Header("Mau")]
        [SerializeField] private Color mauThuong = new Color(0.16f, 0.11f, 0.06f, 0.97f);
        [SerializeField] private Color mauDangChon = new Color(0.38f, 0.28f, 0.10f, 1f);
        [SerializeField] private Color mauKhongDu = new Color(0.12f, 0.09f, 0.07f, 0.85f);
        [SerializeField] private Color chuSang = new Color32(0xFF, 0xF4, 0xD8, 255);
        [SerializeField] private Color chuMo = new Color32(0xA0, 0x90, 0x76, 255);

        private class Nut
        {
            public BuildingData dulieu;
            public Image nen;
            public Image icon;
            public Text tenCT;
            public Text giaCT;
            public Button bam;
            public bool duTien;
            public Text soThe;
        }

        private readonly List<Nut> cacNut = new List<Nut>();
        private Font fontTen;
        private Font fontSo;

        private void Start()
        {
            Dung();
        }

        private void Dung()
        {
            foreach (Transform con in transform) Destroy(con.gameObject);
            cacNut.Clear();

            var hang = GetComponent<HorizontalLayoutGroup>();
            if (hang == null) hang = gameObject.AddComponent<HorizontalLayoutGroup>();
            hang.spacing = demGiua;
            hang.childAlignment = TextAnchor.LowerCenter;
            hang.childForceExpandWidth = false;
            hang.childForceExpandHeight = false;

            fontTen = ChuUI.Dam;
            fontSo = ChuUI.Thuong;

            foreach (var ct in danhSach)
            {
                if (ct == null) continue;
                cacNut.Add(TaoNut(ct, fontTen));
            }
        }

        private Nut TaoNut(BuildingData ct, Font font)
        {
            var go = new GameObject("Nut_" + ct.displayName, typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = rongNut;
            le.preferredHeight = caoNut;

            var nen = go.AddComponent<Image>();
            nen.sprite = khungNut;
            nen.type = Image.Type.Sliced;
            nen.pixelsPerUnitMultiplier = 3.125f;
            nen.color = mauThuong;

            var bam = go.AddComponent<Button>();
            bam.targetGraphic = nen;

            // Anh cong trinh
            var goIcon = new GameObject("Anh", typeof(RectTransform));
            goIcon.transform.SetParent(go.transform, false);
            var rtIcon = goIcon.GetComponent<RectTransform>();
            rtIcon.anchorMin = new Vector2(0.5f, 1f);
            rtIcon.anchorMax = new Vector2(0.5f, 1f);
            rtIcon.pivot = new Vector2(0.5f, 1f);
            rtIcon.anchoredPosition = new Vector2(0f, -10f);
            rtIcon.sizeDelta = new Vector2(rongNut - 30, caoNut - 74);
            var icon = goIcon.AddComponent<Image>();
            icon.sprite = ct.icon != null ? ct.icon : ct.sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            // Ten cong trinh
            var goTen = new GameObject("Ten", typeof(RectTransform));
            goTen.transform.SetParent(go.transform, false);
            var rtTen = goTen.GetComponent<RectTransform>();
            rtTen.anchorMin = new Vector2(0f, 0f);
            rtTen.anchorMax = new Vector2(1f, 0f);
            rtTen.pivot = new Vector2(0.5f, 0f);
            rtTen.offsetMin = new Vector2(2f, 30f);
            rtTen.offsetMax = new Vector2(-2f, 58f);
            var tenCT = goTen.AddComponent<Text>();
            tenCT.font = fontTen;
            tenCT.fontSize = 20;
            tenCT.alignment = TextAnchor.MiddleCenter;
            tenCT.color = chuSang;
            tenCT.text = ct.displayName;
            tenCT.raycastTarget = false;
            tenCT.horizontalOverflow = HorizontalWrapMode.Overflow;
            ThemVien(goTen);

            // Gia
            var goGia = new GameObject("Gia", typeof(RectTransform));
            goGia.transform.SetParent(go.transform, false);
            var rtGia = goGia.GetComponent<RectTransform>();
            rtGia.anchorMin = new Vector2(0f, 0f);
            rtGia.anchorMax = new Vector2(1f, 0f);
            rtGia.pivot = new Vector2(0.5f, 0f);
            rtGia.offsetMin = new Vector2(2f, 4f);
            rtGia.offsetMax = new Vector2(-2f, 30f);
            var giaCT = goGia.AddComponent<Text>();
            giaCT.font = fontSo;
            giaCT.fontSize = 18;
            giaCT.alignment = TextAnchor.MiddleCenter;
            giaCT.color = chuSang;
            giaCT.text = MoTaGia(ct);
            giaCT.raycastTarget = false;
            giaCT.horizontalOverflow = HorizontalWrapMode.Overflow;
            ThemVien(goGia);

            // So the dang co, goc tren ben phai
            var goThe = new GameObject("SoThe", typeof(RectTransform));
            goThe.transform.SetParent(go.transform, false);
            var rtThe = goThe.GetComponent<RectTransform>();
            rtThe.anchorMin = rtThe.anchorMax = new Vector2(1f, 1f);
            rtThe.pivot = new Vector2(1f, 1f);
            rtThe.anchoredPosition = new Vector2(-8f, -6f);
            rtThe.sizeDelta = new Vector2(50f, 26f);
            var soThe = goThe.AddComponent<Text>();
            soThe.font = fontSo != null ? fontSo : font;
            soThe.fontSize = 20;
            soThe.alignment = TextAnchor.UpperRight;
            soThe.color = new Color32(0xFF, 0xE0, 0x8A, 255);
            soThe.raycastTarget = false;
            soThe.horizontalOverflow = HorizontalWrapMode.Overflow;
            ThemVien(goThe);

            var nut = new Nut { dulieu = ct, nen = nen, icon = icon, tenCT = tenCT, giaCT = giaCT, bam = bam, soThe = soThe };
            bam.onClick.AddListener(() => Chon(nut));
            return nut;
        }

        private void ThemVien(GameObject go)
        {
            ChuUI.ApDung(go.GetComponent<Text>(), go.name == "Ten");
        }

        private string MoTaGia(BuildingData ct)
        {
            if (ct.buildCosts == null || ct.buildCosts.Length == 0) return "mien phi";
            var s = "";
            foreach (var c in ct.buildCosts)
            {
                if (s.Length > 0) s += "  ";
                s += TenNgan(c.type) + " " + c.amount;
            }
            return s;
        }

        private string TenNgan(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Souls: return "Hon";
                case ResourceType.Bone: return "Xuong";
                case ResourceType.DarkCrystal: return "Tinh the";
                case ResourceType.Meat: return "Thit";
                case ResourceType.Gold: return "Vang";
                case ResourceType.Gem: return "Ngoc";
                case ResourceType.Wood: return "Go";
                case ResourceType.Stone: return "Da";
                case ResourceType.Food: return "Luong";
                default: return t.ToString();
            }
        }

        private void Chon(Nut nut)
        {
            var ql = QuanLyODat.Instance;
            if (ql == null) { Debug.LogWarning("[BangChonCongTrinh] Chua co QuanLyODat"); return; }
            if (ql.ODangChon == null) { Debug.Log("Hay bam vao mot o dat trong truoc khi chon cong trinh"); return; }

            ql.Xay(nut.dulieu);
        }

        private void Update()
        {
            var kho = ResourceManager.Instance;
            var ql = QuanLyODat.Instance;
            var oChon = ql != null ? ql.ODangChon : null;

            // Chua chon o dat thi ca bang mo di, nhac nguoi choi bam o truoc
            bool coO = oChon != null;

            foreach (var n in cacNut)
            {
                bool duTien = kho == null || n.dulieu.buildCosts == null
                              || n.dulieu.buildCosts.Length == 0
                              || kho.CanAfford(n.dulieu.buildCosts);
                // Tach rieng hai ly do: sai dia hinh khac voi thieu tai nguyen
                bool saiDiaHinh = coO && oChon.loaiKhu != n.dulieu.khuYeuCau;
                bool vuaO = !coO || oChon.ChuaDuoc(n.dulieu);
                duTien = duTien && coO && vuaO;

                // Chi hien cong trinh dang co the, kem so luong
                int soTheCo = KhoThe.Instance != null ? KhoThe.Instance.SoThe(n.dulieu) : 1;
                bool hien = soTheCo > 0;
                if (n.nen.gameObject.activeSelf != hien) n.nen.gameObject.SetActive(hien);
                if (!hien) continue;
                if (n.soThe != null) n.soThe.text = "x" + soTheCo;
                bool dangChon = false;

                // Sai dia hinh thi lam mo han di, khong chi doi mau gia
                n.icon.color = saiDiaHinh
                    ? new Color(0.4f, 0.38f, 0.36f, 0.5f)
                    : (duTien ? Color.white : new Color(0.55f, 0.5f, 0.45f, 0.75f));

                n.duTien = duTien;
                n.bam.interactable = duTien;
                n.nen.color = dangChon ? mauDangChon : (duTien ? mauThuong : mauKhongDu);
                n.icon.color = duTien ? Color.white : new Color(0.55f, 0.5f, 0.45f, 0.75f);
                n.tenCT.color = duTien ? chuSang : chuMo;
                n.giaCT.color = duTien ? chuSang : new Color32(0xC8, 0x6A, 0x5A, 255);
            }
        }
    }
}
