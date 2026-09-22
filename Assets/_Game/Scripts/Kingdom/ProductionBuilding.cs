using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Cong trinh da dat tren luoi. Tu san xuat tai nguyen theo chu ky
    /// khi co nhan cong duoc gan. Chi chay trong pha Kingdom.
    /// </summary>
    public class ProductionBuilding : MonoBehaviour
    {
        [SerializeField] private BuildingData data;

        [Header("Trang thai")]
        [SerializeField] private int level = 1;

        private readonly List<MinionInstance> workers = new List<MinionInstance>();
        [SerializeField] private int nhanCongThue;   // dan thuong duoc thue vao lam
        private float timer;

        public BuildingData Data => data;
        public int Level => level;
        public Vector3Int GridOrigin { get; set; }
        public IReadOnlyList<MinionInstance> Workers => workers;

        /// <summary>Tong nhan cong dang lam: quai duoc gan cong dan thuong duoc thue.</summary>
        public int TongNhanCong => workers.Count + nhanCongThue;
        public int NhanCongThue => nhanCongThue;
        public int TranNhanCong => data != null ? data.maxWorkers : 0;

        /// <summary>San luong moi lan ra hang, cung cong thuc voi Produce().</summary>
        public int SanLuongMoiLan
        {
            get
            {
                if (data == null) return 0;
                float m = Mathf.Pow(Mathf.Max(1f, data.upgradeOutputMultiplier), level - 1);
                int wb = data.requiresWorker ? Mathf.Max(1, TongNhanCong) : 1;
                return Mathf.RoundToInt(data.outputAmount * m) * wb;
            }
        }

        /// <summary>Tien do chu ky san xuat hien tai (0..1), dung cho thanh progress tren UI.</summary>
        public float Progress => data != null && data.productionInterval > 0f
            ? Mathf.Clamp01(timer / data.productionInterval)
            : 0f;

        public event Action<ProductionBuilding> OnProduced;
        public event Action<ProductionBuilding> OnStateChanged;

        public void Initialize(BuildingData buildingData, Vector3Int origin)
        {
            data = buildingData;
            GridOrigin = origin;
            timer = 0f;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && data != null && data.sprite != null)
                sr.sprite = data.sprite;
        }

        private void Update()
        {
            if (data != null && data.linhSanXuat != null) CapNhatRaLinh();
            if (data == null || !data.producesResource) return;

            var gm = GameManager.Instance;
            // Chi san xuat trong pha xay dung - giu dung nhip "ngay xay, dem danh"
            if (gm == null || gm.Phase != GamePhase.Kingdom || gm.IsPaused) return;

            if (data.requiresWorker && TongNhanCong == 0) return;

            timer += gm.ScaledDelta;
            if (timer < data.productionInterval) return;

            timer -= data.productionInterval;
            Produce();
        }

        // ---------- Huan luyen linh ----------
        private float henLinh;
        public event Action<ProductionBuilding, MinionData> OnRaLinh;
        public float TienDoRaLinh => data != null && data.linhSanXuat != null && data.thoiGianRaLinh > 0f
                                     ? Mathf.Clamp01(henLinh / data.thoiGianRaLinh) : 0f;

        private void CapNhatRaLinh()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Phase != GamePhase.Kingdom || gm.IsPaused) return;
            if (data.requiresWorker && TongNhanCong == 0) return;
            var ro = MinionRoster.Instance;
            if (ro == null || ro.Count >= ro.SucChua) return;

            // Them nhan cong thi huan luyen nhanh hon
            float nhanh = data.requiresWorker ? Mathf.Max(1, TongNhanCong) : 1f;
            henLinh += gm.ScaledDelta * nhanh;
            if (henLinh < data.thoiGianRaLinh) return;

            henLinh = 0f;
            ro.Recruit(data.linhSanXuat, level);
            OnRaLinh?.Invoke(this, data.linhSanXuat);
        }

        private void Produce()
        {
            if (ResourceManager.Instance == null) return;

            float multiplier = Mathf.Pow(Mathf.Max(1f, data.upgradeOutputMultiplier), level - 1);
            int workerBonus = data.requiresWorker ? Mathf.Max(1, TongNhanCong) : 1;
            int amount = Mathf.RoundToInt(data.outputAmount * multiplier) * workerBonus;

            ResourceManager.Instance.Add(data.output, amount);
            OnProduced?.Invoke(this);
        }

        // ---------- Nhan cong ----------

        public bool CanAcceptWorker => data != null && TongNhanCong < data.maxWorkers;

        /// <summary>Thue them mot dan thuong. Tra ve false neu het cho hoac het dan.</summary>
        public bool ThueNhanCong()
        {
            if (!CanAcceptWorker) return false;
            if (ResourceManager.Instance != null &&
                !ResourceManager.Instance.TrySpend(ResourceType.Population, 1)) return false;
            nhanCongThue++;
            OnStateChanged?.Invoke(this);
            return true;
        }

        /// <summary>Tra mot dan thuong ve kho dan so.</summary>
        public bool SaThaiNhanCong()
        {
            if (nhanCongThue <= 0) return false;
            nhanCongThue--;
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.Add(ResourceType.Population, 1);
            OnStateChanged?.Invoke(this);
            return true;
        }

        public bool AssignWorker(MinionInstance minion)
        {
            if (minion == null || !CanAcceptWorker) return false;
            if (minion.assignedTo != null) minion.assignedTo.UnassignWorker(minion);

            workers.Add(minion);
            minion.assignedTo = this;
            OnStateChanged?.Invoke(this);
            return true;
        }

        public void UnassignWorker(MinionInstance minion)
        {
            if (minion == null) return;
            if (workers.Remove(minion))
            {
                minion.assignedTo = null;
                OnStateChanged?.Invoke(this);
            }
        }

        public void UnassignAll()
        {
            foreach (var w in workers) w.assignedTo = null;
            workers.Clear();
            OnStateChanged?.Invoke(this);
        }

        // ---------- Nang cap ----------

        public bool CanUpgrade()
        {
            if (data == null || data.upgradeCosts == null || data.upgradeCosts.Length == 0) return false;
            return ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(data.upgradeCosts);
        }

        public bool TryUpgrade()
        {
            if (!CanUpgrade()) return false;
            if (!ResourceManager.Instance.TrySpend(data.upgradeCosts)) return false;

            level++;
            OnStateChanged?.Invoke(this);
            return true;
        }

        private void OnDestroy()
        {
            foreach (var w in workers)
                if (w != null) w.assignedTo = null;
        }
    }
}
