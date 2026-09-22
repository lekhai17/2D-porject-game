using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Lam cong trinh "song": nhan cong dung lam dung nghe (so nguoi = so nhan cong da thue),
    /// khoi ong, lua lo, bui da, lap lanh; moi lan ra hang co nguoi khieng hang di va so +N noi len.
    /// Khong co nhan cong thi dung im, nhin la biet cong trinh dang bo khong.
    /// </summary>
    [RequireComponent(typeof(ProductionBuilding))]
    public class HoatDongCongTrinh : MonoBehaviour
    {
        public KhoHoatHinh kho;

        private ProductionBuilding pb;
        private SpriteRenderer srCT;
        private Transform giaDo;
        private readonly List<SpriteRenderer> tho = new List<SpriteRenderer>();
        private readonly List<HoatHinhKhung> hhTho = new List<HoatHinhKhung>();
        private readonly List<SpriteRenderer> daCuaTho = new List<SpriteRenderer>();   // tang da tho mo dap
        private readonly List<SpriteRenderer> lua = new List<SpriteRenderer>();
        private float henKhoi, henBui, henSang, nhip;
        private Vector3 coGoc = Vector3.one;
        private bool daCoCoGoc;

        private void Start()
        {
            pb = GetComponent<ProductionBuilding>();
            srCT = GetComponent<SpriteRenderer>();
            var g = new GameObject("HoatDong_" + name);
            if (transform.parent != null) g.transform.SetParent(transform.parent, false);
            giaDo = g.transform;
            pb.OnProduced += KhiSanXuat;
            pb.OnRaLinh += KhiRaLinh;
        }

        private void OnDestroy()
        {
            if (pb != null) { pb.OnProduced -= KhiSanXuat; pb.OnRaLinh -= KhiRaLinh; }
            if (giaDo != null) Destroy(giaDo.gameObject);
        }

        private KieuHoatDong Kieu => pb != null && pb.Data != null ? pb.Data.kieuHoatDong : KieuHoatDong.KhongCo;

        private float Dt
        {
            get { var gm = GameManager.Instance; return gm != null ? gm.ScaledDelta : Time.deltaTime; }
        }

        private bool DangLam()
        {
            var gm = GameManager.Instance;
            var d = pb.Data;
            if (gm == null || gm.Phase != GamePhase.Kingdom || gm.IsPaused) return false;
            if (Kieu == KieuHoatDong.QuanTro) return true;
            if (!d.producesResource && d.linhSanXuat == null) return false;
            if (d.requiresWorker && pb.TongNhanCong == 0) return false;
            return true;
        }

        private void Update()
        {
            if (kho == null || pb == null || pb.Data == null || srCT == null) return;

            // Dang thi cong thi an het
            if (!pb.enabled) { AnTatCa(); return; }
            if (!daCoCoGoc) { coGoc = transform.localScale; daCoCoGoc = true; }

            bool lam = DangLam();
            var b = srCT.bounds;
            var k = Kieu;
            float dt = Dt;

            CapNhatTho(lam, b, k);
            CapNhatLua(lam && k == KieuHoatDong.RenSat, b);

            // Khoi ong
            if (lam && (k == KieuHoatDong.RenSat || k == KieuHoatDong.QuanTro))
            {
                henKhoi -= dt;
                if (henKhoi <= 0f)
                {
                    henKhoi = 0.55f + Random.value * 0.3f;
                    var ds = pb.Data.cacOngKhoi != null && pb.Data.cacOngKhoi.Length > 0
                        ? pb.Data.cacOngKhoi : new[] { pb.Data.viTriOngKhoi };
                    foreach (var o in ds)
                    {
                        var p = new Vector3(b.min.x + b.size.x * o.x, b.min.y + b.size.y * o.y, 0f);
                        HatHieuUng.Tao(kho.khoi, p, 0.4f, new Vector3(0.06f, 0.5f, 0f), 1.2f,
                            new Color(0.78f, 0.78f, 0.8f, 0.75f), 0.9f, kho.vatLieuThuong, srCT.sortingOrder + 5, giaDo);
                    }
                }
            }

            // Bui da khi dao
            if (lam && k == KieuHoatDong.DaoMo)
            {
                henBui -= dt;
                if (henBui <= 0f)
                {
                    henBui = 0.85f;
                    foreach (var t in tho)
                    {
                        var p = t.transform.position + new Vector3(t.flipX ? -0.45f : 0.45f, 0.1f, 0f);
                        HatHieuUng.Tao(kho.khoi, p, 0.38f, new Vector3(0f, 0.25f, 0f), 0.6f,
                            new Color(0.82f, 0.8f, 0.76f, 0.9f), 0.5f, kho.vatLieuThuong, t.sortingOrder + 2, giaDo);
                    }
                }
            }

            // Lap lanh tim: thap phap su va mo ngoc
            if (lam && (k == KieuHoatDong.PhepThuat || pb.Data.output == ResourceType.Gem))
            {
                henSang -= dt;
                if (henSang <= 0f)
                {
                    henSang = 0.22f;
                    var p = new Vector3(Random.Range(b.min.x + b.size.x * 0.2f, b.max.x - b.size.x * 0.2f),
                                        Random.Range(b.min.y + b.size.y * 0.3f, b.max.y - b.size.y * 0.1f), 0f);
                    HatHieuUng.Tao(kho.khoi, p, 0.22f, new Vector3(0f, 0.7f, 0f), 0.8f,
                        new Color(0.85f, 0.6f, 1f, 0.9f), 0.2f, kho.vatLieuSang, srCT.sortingOrder + 6, giaDo);
                }
            }

            // Nay nhe khi vua ra hang
            if (nhip > 0f)
            {
                nhip -= Time.deltaTime;
                float p = 1f - Mathf.Clamp01(nhip / 0.3f);
                float s = Mathf.Sin(p * Mathf.PI) * 0.07f;
                transform.localScale = new Vector3(coGoc.x * (1f + s * 0.5f), coGoc.y * (1f + s), coGoc.z);
                if (nhip <= 0f) transform.localScale = coGoc;
            }
        }

        private void CapNhatTho(bool lam, Bounds b, KieuHoatDong k)
        {
            int can = 0;
            if (k == KieuHoatDong.QuanTro) can = 1;
            else if (k != KieuHoatDong.KhongCo) can = Mathf.Min(pb.TongNhanCong, 2);

            while (tho.Count < can) TaoTho();
            while (tho.Count > can)
            {
                int cuoi = tho.Count - 1;
                if (tho[cuoi] != null) Destroy(tho[cuoi].gameObject);
                if (daCuaTho[cuoi] != null) Destroy(daCuaTho[cuoi].gameObject);
                tho.RemoveAt(cuoi); hhTho.RemoveAt(cuoi); daCuaTho.RemoveAt(cuoi);
            }

            bool dapDa = k == KieuHoatDong.DaoMo;
            var khungLam = kho.LayLam(k);
            var khungNghi = kho.LayNghi(k);
            for (int i = 0; i < tho.Count; i++)
            {
                var sr = tho[i];
                sr.enabled = true;
                float y = b.min.y + 0.12f;
                float x;
                if (dapDa)
                {
                    // Tho mo dung truoc cua mo, quay mat ra ngoai dap tang da dat canh
                    x = i == 0 ? b.min.x + b.size.x * 0.30f : b.max.x - b.size.x * 0.30f;
                    sr.flipX = i == 0;
                }
                else
                {
                    x = i == 0 ? b.min.x + b.size.x * 0.18f : b.max.x - b.size.x * 0.18f;
                    sr.flipX = i == 1;                      // nguoi ben phai quay mat vao trong
                }
                sr.transform.position = new Vector3(x, y, 0f);
                sr.sortingOrder = srCT.sortingOrder + 3;
                hhTho[i].Dat(lam ? khungLam : khungNghi, lam ? kho.FpsLam(k) : 8f, true);

                var da = daCuaTho[i];
                if (da == null) continue;
                da.enabled = dapDa;
                if (!dapDa) continue;
                // Tang da nam dung cho luoi cuoc bo xuong
                da.transform.position = new Vector3(x + (sr.flipX ? -0.55f : 0.55f), y + 0.15f, 0f);
                da.sortingOrder = srCT.sortingOrder + 4;
            }
        }

        private void TaoTho()
        {
            var go = new GameObject("NhanCong");
            go.transform.SetParent(giaDo, false);
            go.transform.localScale = Vector3.one * kho.coTho;
            var sr = go.AddComponent<SpriteRenderer>();
            if (kho.vatLieuThuong != null) sr.sharedMaterial = kho.vatLieuThuong;
            var hh = go.AddComponent<HoatHinhKhung>();
            tho.Add(sr); hhTho.Add(hh);

            SpriteRenderer da = null;
            if (kho.daTang != null && kho.daTang.Length > 0)
            {
                var gd = new GameObject("TangDa");
                gd.transform.SetParent(giaDo, false);
                gd.transform.localScale = Vector3.one * 0.6f;
                da = gd.AddComponent<SpriteRenderer>();
                da.sprite = kho.daTang[Random.Range(0, kho.daTang.Length)];
                if (kho.vatLieuThuong != null) da.sharedMaterial = kho.vatLieuThuong;
                // Mo ngoc: da anh tim cho biet ben trong co tinh the
                if (pb != null && pb.Data != null && pb.Data.output == ResourceType.Gem)
                    da.color = new Color(0.86f, 0.72f, 1f);
                da.enabled = false;
            }
            daCuaTho.Add(da);
        }

        private void CapNhatLua(bool bat, Bounds b)
        {
            var ds = pb.Data.cacLoLua != null && pb.Data.cacLoLua.Length > 0
                ? pb.Data.cacLoLua : new[] { new Vector2(0.32f, 0.1f) };
            if (!bat) { foreach (var l in lua) if (l != null) l.enabled = false; return; }

            while (lua.Count < ds.Length)
            {
                var go = new GameObject("LuaLo");
                go.transform.SetParent(giaDo, false);
                go.transform.localScale = Vector3.one * 0.5f;
                var sr = go.AddComponent<SpriteRenderer>();
                if (kho.vatLieuSang != null) sr.sharedMaterial = kho.vatLieuSang;
                go.AddComponent<HoatHinhKhung>().Dat(kho.lua, 12f, true);
                lua.Add(sr);
            }
            for (int i = 0; i < lua.Count; i++)
            {
                var sr = lua[i];
                if (i >= ds.Length) { sr.enabled = false; continue; }
                sr.enabled = true;
                sr.transform.position = new Vector3(b.min.x + b.size.x * ds[i].x, b.min.y + b.size.y * ds[i].y, 0f);
                sr.sortingOrder = srCT.sortingOrder + 2;
            }
        }

        private void AnTatCa()
        {
            foreach (var t in tho) if (t != null) t.enabled = false;
            foreach (var d in daCuaTho) if (d != null) d.enabled = false;
            foreach (var l in lua) if (l != null) l.enabled = false;
        }

        private void KhiSanXuat(ProductionBuilding p)
        {
            if (kho == null || srCT == null) return;
            nhip = 0.3f;
            var b = srCT.bounds;

            // Nguoi khieng hang chay ra phia trai roi mat dan
            var dau = tho.Count > 0 ? tho[0].transform.position : new Vector3(b.center.x, b.min.y + 0.1f, 0f);
            var khung = kho.LayChay(p.Data.output);
            if (khung != null && khung.Length > 0)
            {
                var go = new GameObject("KhiengHang");
                go.transform.SetParent(giaDo, false);
                go.transform.position = dau;
                go.transform.localScale = Vector3.one * kho.coTho;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.flipX = true;
                sr.sortingOrder = srCT.sortingOrder + 4;
                if (kho.vatLieuThuong != null) sr.sharedMaterial = kho.vatLieuThuong;
                go.AddComponent<HoatHinhKhung>().Dat(khung, 12f, true);
                var dc = go.AddComponent<DiChuyenBienMat>();
                dc.tu = dau;
                dc.den = dau + new Vector3(-2.4f, -0.3f, 0f);
                dc.thoiGian = 1.7f;
            }

            ChuNoiLen.Tao("+" + p.SanLuongMoiLan + " " + TenNgan(p.Data.output), MauTaiNguyen(p.Data.output),
                          new Vector3(b.center.x, b.max.y - 0.1f, 0f), giaDo);
        }

        private void KhiRaLinh(ProductionBuilding p, MinionData linh)
        {
            if (srCT == null || linh == null) return;
            nhip = 0.3f;
            var b = srCT.bounds;
            var dau = new Vector3(b.center.x, b.min.y + 0.1f, 0f);
            var khung = linh.khungChay != null && linh.khungChay.Length > 0 ? linh.khungChay : linh.khungDung;
            if (khung != null && khung.Length > 0)
            {
                // Tan binh chay ra khoi cong trinh
                var go = new GameObject("TanBinh");
                go.transform.SetParent(giaDo, false);
                go.transform.position = dau;
                go.transform.localScale = Vector3.one * linh.coHinh;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = srCT.sortingOrder + 4;
                if (kho != null && kho.vatLieuThuong != null) sr.sharedMaterial = kho.vatLieuThuong;
                go.AddComponent<HoatHinhKhung>().Dat(khung, 12f, true);
                var dc = go.AddComponent<DiChuyenBienMat>();
                dc.tu = dau;
                dc.den = dau + new Vector3(2.4f, -0.4f, 0f);
                dc.thoiGian = 1.6f;
            }
            ChuNoiLen.Tao("+1 " + linh.displayName, new Color(0.6f, 0.82f, 1f), new Vector3(b.center.x, b.max.y - 0.1f, 0f), giaDo);
        }

        private static string TenNgan(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Gold: return "Vang";
                case ResourceType.Gem: return "Ngoc";
                case ResourceType.Wood: return "Go";
                case ResourceType.Stone: return "Da";
                case ResourceType.Food: return "Luong";
                case ResourceType.Army: return "Quan";
                default: return t.ToString();
            }
        }

        private static Color MauTaiNguyen(ResourceType t)
        {
            switch (t)
            {
                case ResourceType.Gold: return new Color(1f, 0.85f, 0.35f);
                case ResourceType.Gem: return new Color(0.85f, 0.6f, 1f);
                case ResourceType.Wood: return new Color(0.9f, 0.68f, 0.42f);
                case ResourceType.Stone: return new Color(0.82f, 0.82f, 0.86f);
                case ResourceType.Food: return new Color(0.62f, 0.92f, 0.45f);
                case ResourceType.Army: return new Color(1f, 0.55f, 0.45f);
                default: return Color.white;
            }
        }
    }
}
