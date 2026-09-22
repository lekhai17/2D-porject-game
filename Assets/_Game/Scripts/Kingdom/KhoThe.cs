using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Kho the cong trinh. Moi lan xay ton mot the. Giu duoc thanh qua dem thi duoc chon 1 trong 3 the thuong.
    /// </summary>
    public class KhoThe : MonoBehaviour
    {
        public static KhoThe Instance { get; private set; }

        [Serializable]
        public class TheKhoiDau
        {
            public BuildingData congTrinh;
            [Min(0)] public int soLuong = 1;
        }

        [Header("The khoi dau")]
        [SerializeField] private List<TheKhoiDau> theKhoiDau = new List<TheKhoiDau>();

        [Header("The thuong sau moi dem")]
        [SerializeField] private List<BuildingData> khoThuong = new List<BuildingData>();
        [SerializeField] private int soLuaChon = 3;
        [Tooltip("The thuong nang gap bao nhieu lan the hiem")]
        [SerializeField] private int tiLeThuongSoVoiHiem = 3;

        private readonly Dictionary<BuildingData, int> kho = new Dictionary<BuildingData, int>();
        private readonly HashSet<BuildingData> daKhamPha = new HashSet<BuildingData>();

        // Da tung nhan the nay chua, ke ca khi da dung het de xay
        public bool DaKhamPha(BuildingData b) => b != null && daKhamPha.Contains(b);
        public int SoDaKhamPha => daKhamPha.Count;

        // Moi cong trinh trong game, theo thu tu kho thuong
        public IReadOnlyList<BuildingData> TatCa => khoThuong;
        public event Action OnThayDoi;

        private void Awake()
        {
            Instance = this;
            foreach (var t in theKhoiDau)
                if (t != null && t.congTrinh != null && t.soLuong > 0) Them(t.congTrinh, t.soLuong, false);
        }

        public int SoThe(BuildingData b) => b != null && kho.TryGetValue(b, out var n) ? n : 0;

        public void Them(BuildingData b, int n = 1, bool baoTin = true)
        {
            if (b == null || n <= 0) return;
            kho[b] = SoThe(b) + n;
            daKhamPha.Add(b);
            if (baoTin) OnThayDoi?.Invoke();
        }

        /// <summary>Tru mot the khi xay. Tra ve false neu het the.</summary>
        public bool Dung(BuildingData b)
        {
            int n = SoThe(b);
            if (n <= 0) return false;
            kho[b] = n - 1;
            OnThayDoi?.Invoke();
            return true;
        }

        /// <summary>Rut ngau nhien cac the khac nhau de nguoi choi chon, bo qua the chua co cho xay.</summary>
        public List<BuildingData> RutLuaChon()
        {
            var ungVien = new List<BuildingData>();
            foreach (var b in khoThuong)
                if (b != null && CoChoXay(b) && !ungVien.Contains(b)) ungVien.Add(b);

            var kq = new List<BuildingData>();
            for (int lan = 0; lan < soLuaChon && ungVien.Count > 0; lan++)
            {
                int tong = 0;
                foreach (var b in ungVien) tong += TrongSo(b);
                int r = UnityEngine.Random.Range(0, tong);
                foreach (var b in ungVien)
                {
                    r -= TrongSo(b);
                    if (r < 0) { kq.Add(b); ungVien.Remove(b); break; }
                }
            }
            return kq;
        }

        private int TrongSo(BuildingData b) => b.doHiem >= 1 ? 1 : Mathf.Max(1, tiLeThuongSoVoiHiem);

        /// <summary>Cong trinh dong bang luon co cho. Mo va trai go can da mo khoa it nhat mot o dung loai.</summary>
        private bool CoChoXay(BuildingData b)
        {
            if (b.khuYeuCau == LoaiKhuDat.DongBang) return true;
            var ql = QuanLyODat.Instance;
            if (ql == null) return false;
            foreach (var o in ql.DanhSachO)
                if (o != null && o.loaiKhu == b.khuYeuCau && o.DungDuoc) return true;
            return false;
        }
    }
}
