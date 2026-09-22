using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Dinh nghia mot cong trinh trong Kingdom View. Tao asset qua menu:
    /// Assets > Create > Kingdom Ruins > Building Data
    /// </summary>
    [CreateAssetMenu(fileName = "Building_", menuName = "Kingdom Ruins/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Hien thi")]
        public string displayName = "Cong trinh moi";
        [TextArea(2, 4)] public string description;
        public Sprite icon;
        public Sprite sprite;

        [Header("Kich thuoc tren luoi (o)")]
        public Vector2Int footprint = Vector2Int.one;

        [Header("Dia hinh")]
        [Tooltip("Chi xay duoc tren khu dat dung loai nay")]
        public LoaiKhuDat khuYeuCau = LoaiKhuDat.DongBang;

        [Header("Hoat hinh")]
        public KieuHoatDong kieuHoatDong = KieuHoatDong.KhongCo;

        [Header("Huan luyen linh")]
        [Tooltip("Cong trinh tu dong ra linh loai nay vao ban ngay (de trong neu khong)")]
        public MinionData linhSanXuat;
        [Min(1f)] public float thoiGianRaLinh = 25f;

        [Header("The thuong")]
        [Tooltip("0 = thuong, 1 = hiem (ra it hon 3 lan khi chon the)")]
        [Range(0, 1)] public int doHiem = 0;
        [Tooltip("Vi tri ong khoi, tinh theo ti le khung hinh cong trinh (0..1)")]
        public Vector2 viTriOngKhoi = new Vector2(0.68f, 0.86f);
        [Tooltip("Cong trinh co nhieu ong khoi thi khai bao het o day, se dung thay viTriOngKhoi")]
        public Vector2[] cacOngKhoi;
        [Tooltip("Vi tri chan ngon lua cua tung lo ren (ti le 0..1)")]
        public Vector2[] cacLoLua;

        [Header("Chi phi xay")]
        public ResourceCost[] buildCosts;

        [Header("San xuat")]
        [Tooltip("De None neu cong trinh khong san xuat gi")]
        public bool producesResource = true;
        public ResourceType output = ResourceType.Bone;
        [Min(0)] public int outputAmount = 1;
        [Tooltip("So giay moi lan san xuat")]
        [Min(0.1f)] public float productionInterval = 3f;

        [Header("Nhan cong")]
        [Tooltip("Can gan quai vao lam viec moi san xuat")]
        public bool requiresWorker = true;
        [Min(1)] public int maxWorkers = 1;

        [Header("Nang cap")]
        public ResourceCost[] upgradeCosts;
        [Tooltip("He so nhan san luong sau moi cap nang cap")]
        public float upgradeOutputMultiplier = 1.5f;
    }
}
