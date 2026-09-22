using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Mot o dat da danh san de xay cong trinh. Vi tri co dinh, khong cho dat tu do,
    /// nen bo cuc thi tran luon gon gang va de can bang do kho.
    /// </summary>
    public class ODatXayDung : MonoBehaviour
    {
        [Header("Vi tri tren luoi")]
        [Tooltip("O duoi-trai cua khu dat")]
        public Vector2Int goc;
        [Tooltip("Kich thuoc khu dat, cong trinh phai nam vua trong day")]
        public Vector2Int kichThuoc = new Vector2Int(6, 4);

        [Tooltip("Dia hinh khu dat, quyet dinh xay duoc cong trinh nao")]
        public LoaiKhuDat loaiKhu = LoaiKhuDat.DongBang;

        [Header("Khoa")]
        [Tooltip("Bat: o dat phai mo khoa moi xay duoc")]
        public bool caKhoa;
        [Tooltip("Phai qua ngay thu may moi mo duoc")]
        public int ngayYeuCau = 1;
        [Tooltip("Chi phi mo khoa")]
        public ResourceCost[] giaMoKhoa;
        [Tooltip("Dong chu giai thich hien tren bang mo khoa")]
        [TextArea(2, 3)] public string moTaKhoa = "Vung dat chua khai pha";

        [SerializeField] private bool daMoKhoa;

        [Header("Trang thai")]
        [SerializeField] private ProductionBuilding congTrinh;

        public bool DaXay => congTrinh != null;

        /// <summary>O dat da dung duoc chua: khong khoa, hoac da tra phi mo khoa.</summary>
        public bool DungDuoc => !caKhoa || daMoKhoa;
        public bool DaMoKhoa => daMoKhoa;

        /// <summary>Da qua ngay yeu cau chua.</summary>
        public bool DuNgay => GameManager.Instance == null || GameManager.Instance.DayNumber >= ngayYeuCau;

        /// <summary>Du tien mo khoa chua.</summary>
        public bool DuTienMoKhoa =>
            giaMoKhoa == null || giaMoKhoa.Length == 0
            || (ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(giaMoKhoa));

        /// <summary>Tra phi va mo khoa. Tra ve false neu chua du dieu kien.</summary>
        public bool MoKhoa()
        {
            if (daMoKhoa) return true;
            if (!DuNgay) return false;
            if (giaMoKhoa != null && giaMoKhoa.Length > 0)
            {
                if (ResourceManager.Instance == null) return false;
                if (!ResourceManager.Instance.TrySpend(giaMoKhoa)) return false;
            }
            daMoKhoa = true;
            return true;
        }
        public ProductionBuilding CongTrinh => congTrinh;

        /// <summary>Tam o dat, tinh theo toa do the gioi.</summary>
        public Vector3 Tam => new Vector3(goc.x + kichThuoc.x * 0.5f,
                                          goc.y + kichThuoc.y * 0.5f, 0f);

        /// <summary>Cong trinh vua o dat va dung loai dia hinh thi moi xay duoc.</summary>
        public bool ChuaDuoc(BuildingData ct)
            => ct != null
            && ct.footprint.x <= kichThuoc.x && ct.footprint.y <= kichThuoc.y
            && ct.khuYeuCau == loaiKhu;

        /// <summary>Cong trinh xay xong thi bao lai cho o dat giu tham chieu.</summary>
        public void Nhan(ProductionBuilding ct) => congTrinh = ct;

        public void Xoa()
        {
            if (congTrinh != null) Destroy(congTrinh.gameObject);
            congTrinh = null;
        }

        /// <summary>Kiem tra mot o luoi co nam trong khu dat nay khong.</summary>
        public bool Chua(Vector3Int o)
            => o.x >= goc.x && o.x < goc.x + kichThuoc.x
            && o.y >= goc.y && o.y < goc.y + kichThuoc.y;

        private void OnDrawGizmos()
        {
            Gizmos.color = DaXay ? new Color(0.3f, 0.9f, 0.4f, 0.35f) : new Color(1f, 0.85f, 0.3f, 0.35f);
            Gizmos.DrawCube(Tam, new Vector3(kichThuoc.x, kichThuoc.y, 0.1f));
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.9f);
            Gizmos.DrawWireCube(Tam, new Vector3(kichThuoc.x, kichThuoc.y, 0.1f));
        }
    }
}
