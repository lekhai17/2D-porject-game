using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>Bang thong tin voi nen kin, khung dong va du lieu cap nhat truc tiep.</summary>
    public class BangThongTinCongTrinh : MonoBehaviour
    {
        [SerializeField] private Font fontTen;
        [SerializeField] private Font fontSo;
        [System.Serializable]
        public struct IconTaiNguyen { public ResourceType loai; public Sprite icon; }
        [SerializeField] private IconTaiNguyen[] iconTaiNguyen;

        private static readonly Color Nen = new Color32(25, 34, 26, 255);
        private static readonly Color NenO = new Color32(34, 43, 32, 255);
        private static readonly Color Dong = new Color32(164, 119, 52, 255);
        private static readonly Color Vien = new Color32(79, 78, 52, 255);
        private static readonly Color Sang = new Color32(255, 241, 208, 255);
        private static readonly Color Nhat = new Color32(222, 214, 186, 255);
        private static readonly Color Xanh = new Color32(155, 204, 105, 255);
        private static readonly Color Do = new Color32(230, 137, 113, 255);
        private const float Rong = 460f;
        private float caoBang = 755f;
        private ProductionBuilding dangXem;
        private QuanLyODat quanLy;
        private CanvasGroup nhom;
        private RectTransform than, noiDung, vungGia, tienDo;
        private Image anhCongTrinh, anhSanPham;
        private Text oTen, oMoTa, oCap, oMucSanXuat, oSanPham, oChuKy, oQuanSo;
        private Text oTrangThai, oThoiGian, oNhanCong, oGoiY, oNangCap, oNutNangCap;
        private Button nutThem, nutBot, nutNangCap;
        private readonly List<Text> cacGia = new List<Text>();
        private readonly List<ResourceCost> chiPhi = new List<ResourceCost>();
        private readonly Dictionary<Sprite, Sprite> chanDung = new Dictionary<Sprite, Sprite>();
        private float lanCapNhat;

        private void Awake() { if (!GanTuCanh()) Dung(); An(); }

        /// <summary>
        /// Bang da dung san trong scene (chinh tay duoc trong Editor) thi chi noi tham chieu, khong dung lai.
        /// Noi lai hanh dong nut bam va ap dung font UI dong nhat moi lan Play.
        /// </summary>
        private bool GanTuCanh()
        {
            var nd = transform.Find("NoiDung") as RectTransform;
            if (nd == null) return false;

            T Tim<T>(string duong) where T : Component { var x = nd.Find(duong); return x != null ? x.GetComponent<T>() : null; }
            Button TimNut(string ten) { foreach (var b in nd.GetComponentsInChildren<Button>(true)) if (b.name == ten) return b; return null; }

            noiDung = nd;
            than = GetComponent<RectTransform>();
            anhCongTrinh = Tim<Image>("CongTrinh");
            oTen = Tim<Text>("Ten");
            oCap = Tim<Text>("HuyHieuCap/Cap");
            oMoTa = Tim<Text>("MoTa");
            oMucSanXuat = Tim<Text>("MucSanXuat");
            anhSanPham = Tim<Image>("SanPhamIcon");
            oSanPham = Tim<Text>("SanPham");
            oChuKy = Tim<Text>("ChuKy");
            oQuanSo = Tim<Text>("QuanSo/So");
            tienDo = nd.Find("TienDo/Day") as RectTransform;
            oTrangThai = Tim<Text>("TrangThai");
            oThoiGian = Tim<Text>("ThoiGian");
            oNhanCong = Tim<Text>("NhanCong");
            oGoiY = Tim<Text>("GoiY");
            oNangCap = Tim<Text>("CapTiep");
            vungGia = nd.Find("ChiPhi") as RectTransform;
            nutBot = TimNut("NutBot");
            nutThem = TimNut("NutThem");
            nutNangCap = TimNut("NutNangCap");
            var nutDong = TimNut("NutDong");

            if (oTen == null || tienDo == null || vungGia == null || nutBot == null || nutThem == null || nutNangCap == null)
            {
                Debug.LogWarning("[BangThongTinCongTrinh] Bang trong scene thieu thanh phan (co the da doi ten), dung lai bang code.");
                foreach (Transform con in transform) Destroy(con.gameObject);
                return false;
            }

            nhom = GetComponent<CanvasGroup>();
            if (nhom == null) nhom = gameObject.AddComponent<CanvasGroup>();
            caoBang = than.sizeDelta.y;
            oNutNangCap = nutNangCap.GetComponentInChildren<Text>();

            nutBot.onClick.RemoveAllListeners();
            nutBot.onClick.AddListener(() => { if (dangXem != null) dangXem.SaThaiNhanCong(); CapNhat(); });
            nutThem.onClick.RemoveAllListeners();
            nutThem.onClick.AddListener(() => { if (dangXem != null) dangXem.ThueNhanCong(); CapNhat(); });
            nutNangCap.onClick.RemoveAllListeners();
            nutNangCap.onClick.AddListener(() => { if (dangXem != null) dangXem.TryUpgrade(); CapNhat(); });
            if (nutDong != null) { nutDong.onClick.RemoveAllListeners(); nutDong.onClick.AddListener(An); }

            // Font dong goi cung du an, dung giong nhau trong Editor va Play.
            fontTen = ChuUI.Dam;
            fontSo = ChuUI.Thuong;
            foreach (var t in GetComponentsInChildren<Text>(true)) ChuUI.ApDung(t, t.font == ChuUI.Dam || t.fontStyle == FontStyle.Bold);
            return true;
        }
        private void Start()
        {
            quanLy = QuanLyODat.Instance;
            if (quanLy == null) return;
            quanLy.OnChonCongTrinh += Hien;
            quanLy.OnChonODat += ChonDat;
            quanLy.OnChonOKhoa += ChonDat;
            quanLy.OnBoChon += An;
        }
        private void OnDestroy()
        {
            foreach (var sprite in chanDung.Values) if (sprite != null) Destroy(sprite);
            chanDung.Clear();
            if (quanLy == null) return;
            quanLy.OnChonCongTrinh -= Hien;
            quanLy.OnChonODat -= ChonDat;
            quanLy.OnChonOKhoa -= ChonDat;
            quanLy.OnBoChon -= An;
        }
        private void ChonDat(ODatXayDung o) => An();

#if UNITY_EDITOR
        /// <summary>
        /// Chuot phai vao component trong Inspector de chay. Dung toan bo bang thanh doi tuong that trong scene
        /// de chinh tay bang chuot. Khi Play, GanTuCanh() se noi vao ban nay thay vi dung lai.
        /// </summary>
        [ContextMenu("Dung bang vao scene de chinh tay")]
        private void DungVaoScene()
        {
            if (Application.isPlaying) return;
            for (int i = transform.childCount - 1; i >= 0; i--) DestroyImmediate(transform.GetChild(i).gameObject);
            foreach (var cg in GetComponents<CanvasGroup>()) DestroyImmediate(cg);
            foreach (var sh in GetComponents<Shadow>()) DestroyImmediate(sh);
            foreach (var im in GetComponents<Image>()) DestroyImmediate(im);

            fontTen = null; fontSo = null;
            Dung();

            // Font asset luu duoc vao scene, khong can thay bang font tam.
            foreach (var t in GetComponentsInChildren<Text>(true)) ChuUI.ApDung(t, t.font == ChuUI.Dam);
            fontTen = null; fontSo = null;

            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        private void Dung()
        {
            // Font thuong co dau, khong chong vien day len chu nho.
            fontTen = ChuUI.Dam;
            fontSo = ChuUI.Thuong;
            than = GetComponent<RectTransform>();
            if (than == null) than = gameObject.AddComponent<RectTransform>();
            than.anchorMin = than.anchorMax = new Vector2(0f, 0.5f);
            than.pivot = new Vector2(0f, 0.5f);
            than.anchoredPosition = new Vector2(28f, -12f);
            than.sizeDelta = new Vector2(Rong, caoBang);
            nhom = gameObject.AddComponent<CanvasGroup>();
            // Nen doc lap voi sprite vien: ban do khong xuyen qua chu.
            var nen = GetComponent<Image>();
            if (nen == null) nen = gameObject.AddComponent<Image>();
            nen.sprite = null; nen.color = Nen; nen.raycastTarget = true;
            var bong = gameObject.AddComponent<Shadow>();
            bong.effectColor = new Color(0f, 0f, 0f, 0.45f);
            bong.effectDistance = new Vector2(6f, -7f);
            Khung(than, Dong, 3f);
            for (int i = 0; i < 4; i++)
            {
                var goc = Hop(than, "GocDong" + i, 0, 0, 14, 14, Nen, Dong);
                goc.anchorMin = goc.anchorMax = goc.pivot = new Vector2(i % 2, i / 2);
                goc.anchoredPosition = new Vector2(i % 2 == 0 ? 3 : -3, i / 2 == 0 ? 3 : -3);
                Hop(goc, "Dinh", 5, 5, 4, 4, Sang, Dong);
            }
            noiDung = Rect(than, "NoiDung", 0, 0, Rong, caoBang);
            anhCongTrinh = Anh(noiDung, "CongTrinh", 24, 26, 116, 130, null);
            oTen = Chu(noiDung, "Ten", 156, 25, 230, 42, 28, Sang, true);
            var cap = Hop(noiDung, "HuyHieuCap", 156, 72, 92, 29, NenO, Dong);
            oCap = Chu(cap, "Cap", 0, 0, 92, 29, 17, Sang, true, TextAnchor.MiddleCenter);
            oMoTa = Chu(noiDung, "MoTa", 156, 108, 274, 64, 18, Nhat);
            CoChuTuDong(oMoTa, 15, 18);
            Nut(noiDung, "Dong", 398, 22, 38, 36, "×", An);
            PhanCach(184);
            oMucSanXuat = Chu(noiDung, "MucSanXuat", 26, 199, 290, 29, 21, Sang, true);
            anhSanPham = Anh(noiDung, "SanPhamIcon", 28, 241, 56, 60, null);
            oSanPham = Chu(noiDung, "SanPham", 98, 239, 212, 31, 23, Sang, true);
            CoChuTuDong(oSanPham, 16, 23);
            oChuKy = Chu(noiDung, "ChuKy", 98, 273, 220, 26, 17, Nhat);
            CoChuTuDong(oChuKy, 16, 18);
            var quan = Hop(noiDung, "QuanSo", 322, 244, 112, 52, NenO, Vien);
            oQuanSo = Chu(quan, "So", 4, 2, 104, 48, 17, Nhat, false, TextAnchor.MiddleCenter);
            var ray = Hop(noiDung, "TienDo", 26, 318, 408, 24, new Color32(10, 18, 13, 255), Vien);
            tienDo = Rect(ray, "Day", 4, 4, 0, 16);
            var day = tienDo.gameObject.AddComponent<Image>();
            day.color = Xanh; day.raycastTarget = false;
            oTrangThai = Chu(noiDung, "TrangThai", 26, 350, 258, 25, 18, Xanh, true);
            oThoiGian = Chu(noiDung, "ThoiGian", 292, 350, 142, 25, 17, Nhat, false, TextAnchor.MiddleRight);
            PhanCach(391);
            Chu(noiDung, "MucNhanCong", 26, 404, 310, 29, 21, Sang, true).text = "NHÂN CÔNG";
            nutBot = Nut(noiDung, "Bot", 70, 448, 50, 46, "−", () => { if (dangXem != null) dangXem.SaThaiNhanCong(); CapNhat(); });
            Anh(noiDung, "NhanCongIcon", 155, 451, 38, 40, TimIcon(ResourceType.Population));
            oNhanCong = Chu(noiDung, "NhanCong", 202, 448, 96, 46, 27, Sang, true, TextAnchor.MiddleCenter);
            nutThem = Nut(noiDung, "Them", 340, 448, 50, 46, "+", () => { if (dangXem != null) dangXem.ThueNhanCong(); CapNhat(); });
            oGoiY = Chu(noiDung, "GoiY", 26, 501, 408, 28, 15, Nhat, false, TextAnchor.MiddleCenter);
            CoChuTuDong(oGoiY, 14, 16);
            PhanCach(540);
            Chu(noiDung, "MucNangCap", 26, 553, 408, 29, 21, Sang, true).text = "NÂNG CẤP CÔNG TRÌNH";
            oNangCap = Chu(noiDung, "CapTiep", 26, 587, 408, 28, 21, Sang, true, TextAnchor.MiddleCenter);
            vungGia = Rect(noiDung, "ChiPhi", 26, 625, 408, 42);
            nutNangCap = Nut(noiDung, "NangCap", 26, 681, 408, 50, "NÂNG CẤP", () => { if (dangXem != null) dangXem.TryUpgrade(); CapNhat(); }, true);
            oNutNangCap = nutNangCap.GetComponentInChildren<Text>();
        }

        // Toa do tu goc tren trai giup giu cac hang va le nhat quan.
        private static RectTransform Rect(Transform cha, string ten, float x, float y, float w, float h)
        {
            var go = new GameObject(ten, typeof(RectTransform));
            go.layer = cha.gameObject.layer; go.transform.SetParent(cha, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(w, h);
            return rt;
        }
        private static void Khung(RectTransform rt, Color mau, float day)
        {
            for (int i = 0; i < 4; i++)
            {
                var canh = Rect(rt, "Vien" + i, 0, 0, 0, 0);
                canh.anchorMin = new Vector2(i == 1 ? 1 : 0, i == 2 ? 1 : 0);
                canh.anchorMax = new Vector2(i == 0 ? 0 : 1, i == 3 ? 0 : 1);
                canh.pivot = canh.anchorMin;
                canh.offsetMin = Vector2.zero; canh.offsetMax = Vector2.zero;
                canh.sizeDelta = i < 2 ? new Vector2(day, 0) : new Vector2(0, day);
                var img = canh.gameObject.AddComponent<Image>();
                img.color = mau; img.raycastTarget = false;
            }
        }
        private static RectTransform Hop(Transform cha, string ten, float x, float y, float w, float h, Color mau, Color vien)
        {
            var rt = Rect(cha, ten, x, y, w, h);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = mau; img.raycastTarget = false; Khung(rt, vien, 2f);
            return rt;
        }
        private Text Chu(Transform cha, string ten, float x, float y, float w, float h, int co, Color mau, bool dam = false, TextAnchor canh = TextAnchor.MiddleLeft)
        {
            var rt = Rect(cha, ten, x, y, w, h);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = dam ? fontTen : fontSo; t.fontSize = co;
            t.fontStyle = FontStyle.Normal;
            t.color = mau; t.alignment = canh;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false; t.supportRichText = false;
            ChuUI.ApDung(t, dam);
            return t;
        }
        private static void CoChuTuDong(Text t, int min, int max)
        { t.resizeTextForBestFit = true; t.resizeTextMinSize = min; t.resizeTextMaxSize = max; }
        private static Image Anh(Transform cha, string ten, float x, float y, float w, float h, Sprite sprite)
        {
            var img = Rect(cha, ten, x, y, w, h).gameObject.AddComponent<Image>();
            img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false; img.enabled = sprite != null;
            return img;
        }
        private void PhanCach(float y)
        {
            Hop(noiDung, "PhanCach", 26, y, 408, 1, Vien, Vien);
            var hat = Hop(noiDung, "NutDong", 226, y - 3, 7, 7, Dong, Dong);
            hat.localRotation = Quaternion.Euler(0, 0, 45);
        }
        private Button Nut(Transform cha, string ten, float x, float y, float w, float h, string nhan, UnityEngine.Events.UnityAction bam, bool chinh = false)
        {
            var rt = Hop(cha, "Nut" + ten, x, y, w, h, Color.white, Dong);
            var img = rt.GetComponent<Image>(); img.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            var mau = b.colors;
            mau.normalColor = chinh ? new Color32(230, 169, 66, 255) : NenO;
            mau.highlightedColor = chinh ? new Color32(255, 201, 108, 255) : new Color32(66, 70, 43, 255);
            mau.pressedColor = chinh ? Dong : Vien; mau.selectedColor = mau.normalColor;
            mau.disabledColor = new Color32(53, 57, 43, 255); mau.fadeDuration = 0.08f;
            b.colors = mau; b.onClick.AddListener(bam);
            Chu(rt, "Chu", 5, 2, w - 10, h - 4, chinh ? 23 : 27, chinh ? Nen : Sang, true, TextAnchor.MiddleCenter).text = nhan;
            return b;
        }

        public void Hien(ProductionBuilding ct)
        {
            if (ct == null || ct.Data == null) { An(); return; }
            dangXem = ct; nhom.alpha = 1; nhom.interactable = nhom.blocksRaycasts = true;
            transform.SetAsLastSibling();
            var d = ct.Data;
            oTen.text = TenHienThi(d.displayName).ToUpperInvariant(); CoChuTuDong(oTen, 18, 28);
            oMoTa.text = TenHienThi(d.description);
            anhCongTrinh.sprite = d.sprite != null ? d.sprite : d.icon;
            anhCongTrinh.enabled = anhCongTrinh.sprite != null;
            anhSanPham.sprite = d.linhSanXuat != null ? ChanDungLinh(d.linhSanXuat) : TimIcon(d.output);
            anhSanPham.enabled = anhSanPham.sprite != null && (d.linhSanXuat != null || d.producesResource);
            DungGia(d.upgradeCosts); CapNhat(); CanKichThuoc();
        }
        public void An()
        {
            dangXem = null;
            if (nhom == null) return;
            nhom.alpha = 0; nhom.interactable = nhom.blocksRaycasts = false;
        }
        private void Update()
        {
            if (dangXem == null) { if (nhom.alpha > 0) An(); return; }
            if (dangXem.Data == null) { An(); return; }
            CanKichThuoc();
            // Tai nguyen tu cong trinh khac cung cap nhat nut nang cap, ke ca khi pause.
            if (Time.unscaledTime < lanCapNhat) return;
            lanCapNhat = Time.unscaledTime + 0.1f; CapNhat();
        }
        private void CanKichThuoc()
        {
            var cha = than.parent as RectTransform;
            if (cha == null || cha.rect.height <= 0) return;
            float tiLe = Mathf.Min(1f, Mathf.Min((cha.rect.height - 160f) / caoBang, (cha.rect.width - 56f) / Rong));
            than.localScale = Vector3.one * Mathf.Max(0.1f, tiLe);
        }
        private void DungGia(ResourceCost[] gia)
        {
            foreach (Transform con in vungGia) { con.gameObject.SetActive(false); Destroy(con.gameObject); }
            cacGia.Clear(); chiPhi.Clear();
            if (gia != null) chiPhi.AddRange(gia);
            int hang = Mathf.Max(1, Mathf.CeilToInt(chiPhi.Count / 2f));
            for (int i = 0; i < chiPhi.Count; i++)
            {
                float w = chiPhi.Count == 1 ? 408 : 198;
                var o = Hop(vungGia, "Gia" + i, (i % 2) * 210, (i / 2) * 50, w, 42, NenO, Vien);
                var icon = TimIcon(chiPhi[i].type); Anh(o, "Icon", 12, 6, 30, 30, icon);
                var t = Chu(o, "So", icon != null ? 52 : 12, 3, w - (icon != null ? 62 : 24), 36, 18, Sang, true);
                CoChuTuDong(t, 13, 18); cacGia.Add(t);
            }
            if (chiPhi.Count == 0)
                Chu(vungGia, "KhongCo", 0, 0, 408, 42, 17, Nhat, false, TextAnchor.MiddleCenter).text = "Công trình này chưa có nâng cấp";
            float caoGia = hang * 50 - 8;
            vungGia.sizeDelta = new Vector2(408, caoGia);
            nutNangCap.GetComponent<RectTransform>().anchoredPosition = new Vector2(26, -(625 + caoGia + 14));
            caoBang = 625 + caoGia + 14 + 50 + 24;
            than.sizeDelta = noiDung.sizeDelta = new Vector2(Rong, caoBang);
        }
        private void CapNhat()
        {
            if (dangXem == null || dangXem.Data == null) return;
            var d = dangXem.Data; var kho = ResourceManager.Instance; var ro = MinionRoster.Instance; var gm = GameManager.Instance;
            bool linh = d.linhSanXuat != null; bool coSanXuat = linh || d.producesResource;
            float tocDo = linh && d.requiresWorker ? Mathf.Max(1, dangXem.TongNhanCong) : 1f;
            float chuKy = (linh ? d.thoiGianRaLinh : d.productionInterval) / tocDo;
            float phanTram = coSanXuat ? (linh ? dangXem.TienDoRaLinh : dangXem.Progress) : 0f;
            tienDo.sizeDelta = new Vector2(400 * phanTram, 16);
            oCap.text = "Cấp " + dangXem.Level;
            oMucSanXuat.text = linh ? "HUẤN LUYỆN" : "SẢN XUẤT";
            oSanPham.text = linh ? TenHienThi(d.linhSanXuat.displayName) : coSanXuat ? TenTaiNguyen(d.output) : "Công trình hỗ trợ";
            oChuKy.text = coSanXuat ? (linh ? "1 lính" : dangXem.SanLuongMoiLan + " / chu kỳ") + " · " + chuKy.ToString("0.#") + " giây" : "Không sản xuất tài nguyên";
            oQuanSo.text = linh ? "Quân số\n" + (ro != null ? ro.Count + " / " + ro.SucChua : "—") : "Trong kho\n" + (kho != null && coSanXuat ? kho.Get(d.output).ToString() : "—");
            oThoiGian.text = coSanXuat ? (phanTram * chuKy).ToString("0") + " / " + chuKy.ToString("0.#") + " giây" : "—";
            string trangThai = linh ? "Đang huấn luyện" : "Đang sản xuất"; bool hoatDong = true;
            if (!coSanXuat) { trangThai = "Sẵn sàng"; hoatDong = false; }
            else if (gm == null) { trangThai = "Chờ bắt đầu"; hoatDong = false; }
            else if (gm.Phase == GamePhase.GameOver) { trangThai = "Trận đấu đã kết thúc"; hoatDong = false; }
            else if (gm.IsPaused) { trangThai = "Đang tạm dừng"; hoatDong = false; }
            else if (gm.Phase != GamePhase.Kingdom) { trangThai = "Chờ pha xây dựng"; hoatDong = false; }
            else if (d.requiresWorker && dangXem.TongNhanCong == 0) { trangThai = "Cần nhân công"; hoatDong = false; }
            else if (linh && (ro == null || ro.Count >= ro.SucChua)) { trangThai = ro == null ? "Chưa có đội quân" : "Đã đầy quân số"; hoatDong = false; }
            oTrangThai.text = trangThai; oTrangThai.color = hoatDong ? Xanh : Nhat;
            oNhanCong.text = dangXem.TongNhanCong + " / " + dangXem.TranNhanCong;
            int dan = kho != null ? kho.Get(ResourceType.Population) : 0;
            oGoiY.text = !d.requiresWorker ? "Công trình hoạt động không cần nhân công" : "Dân nhàn rỗi: " + dan + (linh ? " · Thêm người để luyện nhanh hơn" : " · Thêm người để tăng sản lượng");
            nutThem.interactable = d.requiresWorker && dangXem.CanAcceptWorker && dan > 0;
            nutBot.interactable = dangXem.NhanCongThue > 0;
            oNangCap.text = chiPhi.Count > 0 ? "Cấp " + dangXem.Level + "   →   Cấp " + (dangXem.Level + 1) : "Cấp " + dangXem.Level;
            for (int i = 0; i < chiPhi.Count; i++)
            {
                var c = chiPhi[i]; cacGia[i].text = c.amount + " " + TenTaiNguyen(c.type);
                cacGia[i].color = kho != null && kho.CanAfford(c.type, c.amount) ? Sang : Do;
            }
            bool du = dangXem.CanUpgrade(); nutNangCap.interactable = du;
            oNutNangCap.text = du ? "↑  NÂNG CẤP" : chiPhi.Count > 0 ? "CHƯA ĐỦ TÀI NGUYÊN" : "CHƯA CÓ NÂNG CẤP";
            oNutNangCap.color = du ? Nen : Nhat;
        }
        private Sprite TimIcon(ResourceType loai)
        {
            if (iconTaiNguyen != null) foreach (var c in iconTaiNguyen) if (c.loai == loai) return c.icon;
            return null;
        }
        private Sprite ChanDungLinh(MinionData linh)
        {
            var s = linh.icon;
            // Khung hoat hinh Tiny Swords chua nhieu khoang trong quanh nhan vat.
            // Chi cat khung do; icon chan dung rieng duoc giu nguyen.
            if (s == null || linh.khungDung == null || linh.khungDung.Length == 0 ||
                s != linh.khungDung[0] || s.rect.width < 128) return s;
            if (chanDung.TryGetValue(s, out var daCat)) return daCat;
            var r = s.rect;
            var vung = new UnityEngine.Rect(r.x + r.width * 0.28f, r.y + r.height * 0.22f, r.width * 0.44f, r.height * 0.50f);
            daCat = Sprite.Create(s.texture, vung, new Vector2(0.5f, 0.5f), s.pixelsPerUnit);
            chanDung[s] = daCat;
            return daCat;
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
                case ResourceType.Population: return "Dân số";
                case ResourceType.Souls: return "Linh hồn";
                case ResourceType.Bone: return "Xương";
                case ResourceType.DarkCrystal: return "Tinh thể";
                case ResourceType.Meat: return "Thịt";
                case ResourceType.Renown: return "Danh vọng";
                default: return t.ToString();
            }
        }
        // Tuong thich ten khong dau trong cac asset hien tai.
        private static string TenHienThi(string ten)
        {
            switch (ten)
            {
                case "Ren giap va thuong, huan luyen Thuong Binh": return "Rèn giáp và thương, huấn luyện Thương Binh.";
                case "Buon ban, thu Vang": return "Buôn bán, thu Vàng.";
                case "Xe go thanh van, cho ra Go xay dung": return "Xẻ gỗ thành ván, cho ra Gỗ xây dựng.";
                case "Trong trot, san xuat Luong thuc nuoi dan": return "Trồng trọt, sản xuất Lương thực nuôi dân.";
                case "Noi anh hung nghi ngoi va len cap": return "Nơi anh hùng nghỉ ngơi và lên cấp.";
                case "Nghien cuu phep thuat, ket tinh Ngoc": return "Nghiên cứu phép thuật, kết tinh Ngọc.";
                case "Don ha cay trong rung gia, san luong cao hon Xuong Cua": return "Đốn hạ cây trong rừng già, sản lượng cao hơn Xưởng Cưa.";
                case "Duc da tu vach nui, cho ra Da xay dung": return "Đục đá từ vách núi, cho ra Đá xây dựng.";
                case "Dao tinh the quy trong long nui": return "Đào tinh thể quý trong lòng núi.";
                case "Huan luyen Kiem Si giu cong thanh": return "Huấn luyện Kiếm Sĩ giữ cổng thành.";
                case "Huan luyen Cung Thu ban tu xa": return "Huấn luyện Cung Thủ bắn từ xa.";
                case "Lo Ren": return "Lò Rèn";
                case "Cho Phien": return "Chợ Phiên";
                case "Xuong Cua": return "Xưởng Cưa";
                case "Nong Trai": return "Nông Trại";
                case "Quan Tro": return "Quán Trọ";
                case "Thap Phap Su": return "Tháp Pháp Sư";
                case "Trai Go": return "Trại Gỗ";
                case "Mo Da": return "Mỏ Đá";
                case "Mo Ngoc": return "Mỏ Ngọc";
                case "Trai Linh": return "Trại Lính";
                case "Truong Ban": return "Trường Bắn";
                case "Hac Dan": return "Hắc Dân";
                case "Hac Kiem Si": return "Hắc Kiếm Sĩ";
                case "Hac Cung Thu": return "Hắc Cung Thủ";
                case "Hac Thuong Tuong": return "Hắc Thương Tướng";
                case "Kiem Si": return "Kiếm Sĩ";
                case "Cung Thu": return "Cung Thủ";
                case "Thuong Binh": return "Thương Binh";
                default: return ten ?? "Công trình";
            }
        }
    }
}
