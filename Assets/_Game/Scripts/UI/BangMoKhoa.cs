using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Bang hien khi bam vao o dat con khoa: cho biet can gi de mo va cho bam mo.
    /// </summary>
    public class BangMoKhoa : MonoBehaviour
    {
        [SerializeField] private Sprite khungBang;
        [SerializeField] private Font fontTen;
        [SerializeField] private Font fontSo;

        [SerializeField] private Color mauNen = new Color(0.16f, 0.11f, 0.06f, 0.97f);
        [SerializeField] private Color chuSang = new Color32(0xFF, 0xF4, 0xD8, 255);
        [SerializeField] private Color chuNhat = new Color32(0xE2, 0xCD, 0xA0, 255);
        [SerializeField] private Color mauDo = new Color32(0xC8, 0x6A, 0x5A, 255);
        [SerializeField] private Color mauXanh = new Color32(0x8C, 0xC8, 0x5A, 255);

        private ODatXayDung dangXem;
        private Image anhNen;
        private Text oTieuDe, oMoTa, oDieuKienNgay, oGia;
        private Button nutMo, nutDong;
        private Text chuNutMo;

        private void Awake() { Dung(); An(); }

        private void Start()
        {
            if (QuanLyODat.Instance == null) return;
            QuanLyODat.Instance.OnChonOKhoa += Hien;
            QuanLyODat.Instance.OnChonODat += _ => An();
            QuanLyODat.Instance.OnChonCongTrinh += _ => An();
            QuanLyODat.Instance.OnBoChon += An;
        }

        private void OnDestroy()
        {
            if (QuanLyODat.Instance == null) return;
            QuanLyODat.Instance.OnChonOKhoa -= Hien;
            QuanLyODat.Instance.OnBoChon -= An;
        }

        private void Dung()
        {
            fontTen = ChuUI.Dam;
            fontSo = ChuUI.Thuong;

            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(460f, 300f);

            anhNen = gameObject.AddComponent<Image>();
            anhNen.sprite = khungBang;
            anhNen.type = Image.Type.Sliced;
            anhNen.pixelsPerUnitMultiplier = 3.125f;
            anhNen.color = mauNen;

            oTieuDe       = Chu("TieuDe", 0f, -20f, 420f, 40f, fontTen, 26, chuSang);
            oMoTa         = Chu("MoTa", 0f, -66f, 420f, 52f, fontTen, 17, chuNhat);
            oDieuKienNgay = Chu("DieuKienNgay", 0f, -128f, 420f, 32f, fontSo, 20, chuSang);
            oGia          = Chu("Gia", 0f, -166f, 420f, 32f, fontSo, 20, chuSang);

            nutMo = Nut("Mo", 0f, -216f, 260f, 54f, "MO KHOA", () =>
            {
                if (dangXem == null) return;
                if (dangXem.MoKhoa()) An();
                else CapNhat();
            });
            chuNutMo = nutMo.GetComponentInChildren<Text>();
            nutDong = Nut("Dong", 196f, -20f, 40f, 40f, "X", An);
        }

        private Text Chu(string ten, float x, float y, float rong, float cao, Font f, int co, Color mau)
        {
            var go = new GameObject(ten, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(rong, cao);
            var t = go.AddComponent<Text>();
            t.font = f; t.fontSize = co; t.color = mau;
            t.fontStyle = FontStyle.Normal;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            HieuUng(go, co >= 18);
            return t;
        }

        private Button Nut(string ten, float x, float y, float rong, float cao,
                           string nhan, UnityEngine.Events.UnityAction khiBam)
        {
            var go = new GameObject("Nut_" + ten, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(rong, cao);

            var img = go.AddComponent<Image>();
            img.sprite = khungBang;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 3.125f;
            img.color = new Color(0.30f, 0.21f, 0.09f, 1f);

            var b = go.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(khiBam);

            var goChu = new GameObject("Chu", typeof(RectTransform));
            goChu.transform.SetParent(go.transform, false);
            var rc = goChu.GetComponent<RectTransform>();
            rc.anchorMin = Vector2.zero; rc.anchorMax = Vector2.one;
            rc.offsetMin = Vector2.zero; rc.offsetMax = Vector2.zero;
            var t = goChu.AddComponent<Text>();
            t.font = fontSo; t.fontSize = 24; t.color = chuSang;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = nhan; t.raycastTarget = false;
            HieuUng(goChu, true);
            return b;
        }

        private void HieuUng(GameObject go, bool day)
        {
            ChuUI.ApDung(go.GetComponent<Text>(), day);
        }

        public void Hien(ODatXayDung o)
        {
            dangXem = o;
            if (o == null) { An(); return; }
            if (anhNen != null) anhNen.enabled = true;
            foreach (Transform con in transform) con.gameObject.SetActive(true);
            CapNhat();
        }

        public void An()
        {
            dangXem = null;
            if (anhNen != null) anhNen.enabled = false;
            foreach (Transform con in transform) con.gameObject.SetActive(false);
        }

        private void CapNhat()
        {
            if (dangXem == null) return;

            oTieuDe.text = TenKhu(dangXem.loaiKhu);
            oMoTa.text = dangXem.moTaKhoa;

            int ngayHienTai = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
            bool duNgay = dangXem.DuNgay;
            oDieuKienNgay.text = "Can den ngay " + dangXem.ngayYeuCau
                                 + "   (hien tai: ngay " + ngayHienTai + ")";
            oDieuKienNgay.color = duNgay ? mauXanh : mauDo;

            bool duTien = dangXem.DuTienMoKhoa;
            oGia.text = "Chi phi: " + MoTaGia(dangXem.giaMoKhoa);
            oGia.color = duTien ? mauXanh : mauDo;

            bool mo = duNgay && duTien;
            nutMo.interactable = mo;
            if (chuNutMo != null)
                chuNutMo.text = mo ? "MO KHOA" : (!duNgay ? "CHUA DEN NGAY" : "THIEU TAI NGUYEN");
        }

        private string TenKhu(LoaiKhuDat k)
        {
            switch (k)
            {
                case LoaiKhuDat.Nui: return "KHU MO KHOANG";
                case LoaiKhuDat.Rung: return "KHU RUNG GIA";
                case LoaiKhuDat.VenBien: return "KHU VEN BIEN";
                default: return "KHU DAT MOI";
            }
        }

        private string MoTaGia(ResourceCost[] gia)
        {
            if (gia == null || gia.Length == 0) return "mien phi";
            var s = "";
            foreach (var g in gia)
            {
                if (s.Length > 0) s += "   ";
                s += TenTaiNguyen(g.type) + " " + g.amount;
            }
            return s;
        }

        private string TenTaiNguyen(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Gold: return "Vang";
                case ResourceType.Gem: return "Ngoc";
                case ResourceType.Wood: return "Go";
                case ResourceType.Stone: return "Da";
                case ResourceType.Food: return "Luong thuc";
                case ResourceType.Army: return "Quan so";
                default: return t.ToString();
            }
        }
    }
}
