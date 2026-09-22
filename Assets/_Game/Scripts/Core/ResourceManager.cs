using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Kho tai nguyen dung chung cho ca hai view. Day la nguon du lieu duy nhat -
    /// Kingdom View va Battle View chi la hai cach hien thi khac nhau cua cung mot state.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Serializable]
        public struct StartingAmount
        {
            public ResourceType type;
            public int amount;
        }

        [SerializeField]
        private List<StartingAmount> startingAmounts = new List<StartingAmount>
        {
            new StartingAmount { type = ResourceType.Souls, amount = 20 },
            new StartingAmount { type = ResourceType.Bone, amount = 30 },
            new StartingAmount { type = ResourceType.Meat, amount = 10 },
        };

        private readonly Dictionary<ResourceType, int> stock = new Dictionary<ResourceType, int>();

        /// <summary>Ban ra (loai, so luong moi) khi kho thay doi.</summary>
        public event Action<ResourceType, int> OnResourceChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
                stock[t] = 0;

            foreach (var s in startingAmounts)
                stock[s.type] = s.amount;
        }

        public int Get(ResourceType type) => stock.TryGetValue(type, out var v) ? v : 0;

        public void Add(ResourceType type, int amount)
        {
            if (amount == 0) return;
            stock[type] = Mathf.Max(0, Get(type) + amount);
            OnResourceChanged?.Invoke(type, stock[type]);
        }

        public bool CanAfford(ResourceType type, int amount) => Get(type) >= amount;

        public bool CanAfford(IEnumerable<ResourceCost> costs)
        {
            foreach (var c in costs)
                if (!CanAfford(c.type, c.amount)) return false;
            return true;
        }

        /// <summary>Tru tai nguyen. Tra ve false va khong tru gi neu khong du.</summary>
        public bool TrySpend(ResourceType type, int amount)
        {
            if (!CanAfford(type, amount)) return false;
            Add(type, -amount);
            return true;
        }

        public bool TrySpend(IEnumerable<ResourceCost> costs)
        {
            if (!CanAfford(costs)) return false;
            foreach (var c in costs)
                Add(c.type, -c.amount);
            return true;
        }
    }

    [Serializable]
    public struct ResourceCost
    {
        public ResourceType type;
        public int amount;

        public ResourceCost(ResourceType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
}
