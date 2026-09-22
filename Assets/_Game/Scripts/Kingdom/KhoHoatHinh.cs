using UnityEngine;

namespace KingdomRuins
{
    [CreateAssetMenu(menuName = "KingdomRuins/Kho Hoat Hinh")]
    public class KhoHoatHinh : ScriptableObject
    {
        [Header("Nhan cong dang lam")]
        public Sprite[] riuLam, cuocLam, buaLam, daoLam;
        [Header("Nhan cong nghi")]
        public Sprite[] riuNghi, cuocNghi, buaNghi, daoNghi, vangNghi, thitNghi, nghiTron;
        [Header("Khieng hang")]
        public Sprite[] chayGo, chayVang, chayThit, chayBua, chayCuoc, chayTron;
        [Header("Hieu ung")]
        public Sprite[] khoi, lua;
        public Sprite[] daTang;   // tang da cho tho mo dap
        [Header("Vat lieu")]
        public Material vatLieuThuong;   // bi anh sang ngay dem anh huong
        public Material vatLieuSang;     // lua, lap lanh: luon sang
        public float coTho = 0.85f;

        public Sprite[] LayLam(KieuHoatDong k)
        {
            switch (k)
            {
                case KieuHoatDong.ChatGo: return riuLam;
                case KieuHoatDong.DaoMo: return cuocLam;
                case KieuHoatDong.RenSat: return buaLam;
                case KieuHoatDong.GatHai: return daoLam;
                case KieuHoatDong.BuonBan: return vangNghi;
                case KieuHoatDong.QuanTro: return thitNghi;
                default: return nghiTron;
            }
        }

        public Sprite[] LayNghi(KieuHoatDong k)
        {
            switch (k)
            {
                case KieuHoatDong.ChatGo: return riuNghi;
                case KieuHoatDong.DaoMo: return cuocNghi;
                case KieuHoatDong.RenSat: return buaNghi;
                case KieuHoatDong.GatHai: return daoNghi;
                case KieuHoatDong.BuonBan: return vangNghi;
                case KieuHoatDong.QuanTro: return thitNghi;
                default: return nghiTron;
            }
        }

        public float FpsLam(KieuHoatDong k)
        {
            switch (k)
            {
                case KieuHoatDong.RenSat: return 6f;     // bua chi 3 frame, cham lai cho ro nhip go
                case KieuHoatDong.ChatGo:
                case KieuHoatDong.DaoMo: return 10f;
                default: return 8f;
            }
        }

        public Sprite[] LayChay(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Wood: return chayGo;
                case ResourceType.Gold:
                case ResourceType.Gem: return chayVang;
                case ResourceType.Food: return chayThit;
                case ResourceType.Stone: return chayCuoc;
                case ResourceType.Army: return chayBua;
                default: return chayTron;
            }
        }
    }
}
