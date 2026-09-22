using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>Mot con quai cu the thuoc quyen nguoi choi, co cap rieng.</summary>
    [Serializable]
    public class MinionInstance
    {
        public MinionData data;
        public int level = 1;
        public int storedMeat;

        /// <summary>True neu dang duoc gan lam viec tai mot cong trinh.</summary>
        [NonSerialized] public ProductionBuilding assignedTo;

        public int MaxHp => data != null ? data.GetHpAtLevel(level) : 0;
        public int Damage => data != null ? data.GetDamageAtLevel(level) : 0;
        public bool IsWorking => assignedTo != null;
    }

    /// <summary>
    /// Danh sach quai nguoi choi so huu. Day la du lieu ban le giua hai view:
    /// nuoi / len cap o Kingdom View, quyet dinh lua chon nao kha dung o Battle View.
    /// </summary>
    public class MinionRoster : MonoBehaviour
    {
        public static MinionRoster Instance { get; private set; }

        [Header("Quai khoi dau")]
        [SerializeField] private List<MinionData> startingMinions = new List<MinionData>();

        [Header("Suc chua")]
        [Tooltip("So linh toi da cung luc")]
        [SerializeField] private int sucChua = 12;
        public int SucChua => sucChua;

        private readonly List<MinionInstance> roster = new List<MinionInstance>();

        public IReadOnlyList<MinionInstance> All => roster;

        public event Action OnRosterChanged;
        public event Action<MinionInstance> OnMinionLeveledUp;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var m in startingMinions)
                if (m != null) Recruit(m);
        }

        public MinionInstance Recruit(MinionData data, int level = 1)
        {
            if (data == null) return null;

            var inst = new MinionInstance { data = data, level = Mathf.Max(1, level) };
            roster.Add(inst);
            OnRosterChanged?.Invoke(); DongBoQuanSo();
            return inst;
        }

        /// <summary>Trieu hoi quai moi bang tai nguyen. Tra ve null neu khong du.</summary>
        public MinionInstance TrySummon(MinionData data)
        {
            if (data == null || ResourceManager.Instance == null) return null;
            if (!ResourceManager.Instance.TrySpend(data.summonResource, data.summonCost)) return null;
            return Recruit(data);
        }

        public void Remove(MinionInstance inst)
        {
            if (inst == null) return;
            if (inst.assignedTo != null) inst.assignedTo.UnassignWorker(inst);
            if (roster.Remove(inst)) OnRosterChanged?.Invoke(); DongBoQuanSo();
        }

        /// <summary>
        /// Dien Tho: cho quai an thit de len cap. Tuong duong co che
        /// anh hung ve Tavern an do de nhan EXP trong Super Fantasy Kingdom.
        /// </summary>
        public bool TryFeed(MinionInstance inst, int meatAmount)
        {
            if (inst == null || inst.data == null || meatAmount <= 0) return false;
            if (ResourceManager.Instance == null) return false;
            if (!ResourceManager.Instance.TrySpend(ResourceType.Meat, meatAmount)) return false;

            inst.storedMeat += meatAmount;

            bool leveled = false;
            while (inst.storedMeat >= inst.data.meatPerLevel)
            {
                inst.storedMeat -= inst.data.meatPerLevel;
                inst.level++;
                leveled = true;
            }

            if (leveled) OnMinionLeveledUp?.Invoke(inst);
            OnRosterChanged?.Invoke(); DongBoQuanSo();
            return true;
        }

        /// <summary>Nhung con dang ranh, dung de gan vao cong trinh hoac ra tran.</summary>
        public List<MinionInstance> GetIdle()
        {
            var list = new List<MinionInstance>();
            foreach (var m in roster)
                if (!m.IsWorking) list.Add(m);
            return list;
        }

        public int Count => roster.Count;

        private void Start() { DongBoQuanSo(); }

        /// <summary>O Quan So tren HUD luon bang so linh dang co.</summary>
        private void DongBoQuanSo()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;
            int lech = roster.Count - rm.Get(ResourceType.Army);
            if (lech > 0) rm.Add(ResourceType.Army, lech);
            else if (lech < 0) rm.TrySpend(ResourceType.Army, -lech);
        }
    }
}
