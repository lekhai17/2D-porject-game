using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomRuins
{
    /// <summary>
    /// Dieu phoi toan bo o dat: bam vao o trong de chon roi xay,
    /// bam vao cong trinh da xay de mo bang thong tin.
    /// </summary>
    public class QuanLyODat : MonoBehaviour
    {
        public static QuanLyODat Instance { get; private set; }

        [SerializeField] private Grid luoi;
        [SerializeField] private Camera camKingdom;
        [SerializeField] private Transform khoCongTrinh;
        [SerializeField] private List<ODatXayDung> danhSachO = new List<ODatXayDung>();

        [SerializeField] private HieuUngXayDung hieuUngXay;
        [SerializeField] private KhoHoatHinh khoHoatHinh;

        [Header("Danh dau o trong")]
        [SerializeField] private Sprite anhODat;
        [SerializeField] private Color mauODat = new Color(1f, 0.88f, 0.45f, 0.30f);
        [SerializeField] private Color mauODatSang = new Color(1f, 0.95f, 0.6f, 0.55f);
        [Tooltip("Mau danh dau o dang khoa")]
        [SerializeField] private Color mauODatKhoa = new Color(0.45f, 0.45f, 0.5f, 0.38f);

        public ODatXayDung ODangChon { get; private set; }

        public event Action<ODatXayDung> OnChonODat;          // bam o trong
        public event Action<ProductionBuilding> OnChonCongTrinh;  // bam cong trinh da xay
        public event Action OnBoChon;
        public event Action<ODatXayDung> OnChonOKhoa;   // bam o dang khoa

        private readonly Dictionary<ODatXayDung, SpriteRenderer> danhDau
            = new Dictionary<ODatXayDung, SpriteRenderer>();

        private void Awake()
        {
            Instance = this;
            if (luoi == null) luoi = GetComponent<Grid>();
            if (camKingdom == null) camKingdom = Camera.main;
        }

        private void Start()
        {
            if (danhSachO.Count == 0)
                danhSachO.AddRange(GetComponentsInChildren<ODatXayDung>(true));
            TaoDanhDau();
        }

        private void TaoDanhDau()
        {
            foreach (var o in danhSachO)
            {
                if (o == null) continue;
                var go = new GameObject("DanhDau");
                go.transform.SetParent(o.transform, false);
                go.transform.position = o.Tam;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = anhODat;
                sr.color = mauODat;
                sr.sortingOrder = -21;
                if (anhODat != null)
                {
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(o.kichThuoc.x, o.kichThuoc.y);
                }
                danhDau[o] = sr;
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.Phase != GamePhase.Kingdom) return;

            var chuot = Mouse.current;
            if (chuot == null) return;

            // An danh dau cua o da xay
            foreach (var cap in danhDau)
            {
                if (cap.Key == null || cap.Value == null) continue;
                bool hien = !cap.Key.DaXay;
                cap.Value.enabled = hien;
                if (!hien) continue;
                if (!cap.Key.DungDuoc) cap.Value.color = mauODatKhoa;
                else cap.Value.color = cap.Key == ODangChon ? mauODatSang : mauODat;
            }

            if (!chuot.leftButton.wasPressedThisFrame) return;

            // Bam vao UI thi bo qua
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            var o2 = OChuot(chuot.position.ReadValue());
            var oDat = TimO(o2);

            if (oDat == null) { BoChon(); return; }

            if (oDat.DaXay)
            {
                ODangChon = null;
                OnChonCongTrinh?.Invoke(oDat.CongTrinh);
            }
            else if (!oDat.DungDuoc)
            {
                // O con khoa: khong cho chon de xay, chi mo bang dieu kien
                ODangChon = null;
                OnChonOKhoa?.Invoke(oDat);
            }
            else
            {
                ODangChon = oDat;
                OnChonODat?.Invoke(oDat);
            }
        }

        public void BoChon()
        {
            ODangChon = null;
            OnBoChon?.Invoke();
        }

        private Vector3Int OChuot(Vector2 manHinh)
        {
            var cam = camKingdom != null ? camKingdom : Camera.main;
            if (cam == null || luoi == null) return Vector3Int.zero;
            var tg = cam.ScreenToWorldPoint(new Vector3(manHinh.x, manHinh.y, 0f));
            tg.z = 0f;
            return luoi.WorldToCell(tg);
        }

        private ODatXayDung TimO(Vector3Int o)
        {
            foreach (var d in danhSachO)
                if (d != null && d.Chua(o)) return d;
            return null;
        }

        /// <summary>Xay cong trinh vao o dang chon. Tra ve null neu khong xay duoc.</summary>
        public ProductionBuilding Xay(BuildingData ct)
        {
            var o = ODangChon;
            if (o == null || ct == null) return null;
            if (o.DaXay) return null;
            if (!o.DungDuoc) return null;
            if (!o.ChuaDuoc(ct)) { Debug.LogWarning("[QuanLyODat] Cong trinh qua lon so voi o dat"); return null; }

            // Moi cong trinh can mot the
            if (KhoThe.Instance != null && KhoThe.Instance.SoThe(ct) <= 0) return null;

            var kho = ResourceManager.Instance;
            if (kho != null && ct.buildCosts != null && ct.buildCosts.Length > 0
                && !kho.TrySpend(ct.buildCosts)) return null;

            var go = new GameObject("CongTrinh_" + ct.displayName);
            go.transform.SetParent(khoCongTrinh != null ? khoCongTrinh : transform);

            // Dat cong trinh vao giua o dat, chan cham day o
            // Can giua trong o dat theo kich thuoc that cua sprite, dung voi moi kieu diem neo
            var spr = ct.sprite;
            float caoSp = spr != null ? spr.rect.height / spr.pixelsPerUnit : ct.footprint.y;
            float neoDuoi = spr != null ? spr.pivot.y / spr.pixelsPerUnit : caoSp * 0.5f;
            float x = o.goc.x + o.kichThuoc.x * 0.5f;
            float day = o.goc.y + Mathf.Max(0.15f, (o.kichThuoc.y - caoSp) * 0.5f);
            float y = day + neoDuoi;
            go.transform.position = new Vector3(x, y, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ct.sprite;
            sr.sortingOrder = ThuTuVe.TheoY(day);

            var pb = go.AddComponent<ProductionBuilding>();
            pb.Initialize(ct, new Vector3Int(o.goc.x, o.goc.y, 0));
            var hd = go.AddComponent<HoatDongCongTrinh>();
            hd.kho = khoHoatHinh;

            o.Nhan(pb);
            if (KhoThe.Instance != null) KhoThe.Instance.Dung(ct);

            // Chay hieu ung thi cong: tho go bua, khoi bui, xong moi hien cong trinh
            if (hieuUngXay != null) hieuUngXay.Chay(pb, o.Tam, o.kichThuoc);

            BoChon();
            return pb;
        }

        public IReadOnlyList<ODatXayDung> DanhSachO => danhSachO;
    }
}
