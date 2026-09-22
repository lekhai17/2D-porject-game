using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Dinh nghia mot don vi chien dau - dung cho ca quan cua Hung Than
    /// va quan xam nhap (chi khac field faction).
    /// Assets > Create > Kingdom Ruins > Minion Data
    /// </summary>
    [CreateAssetMenu(fileName = "Minion_", menuName = "Kingdom Ruins/Minion Data")]
    public class MinionData : ScriptableObject
    {
        [Header("Hien thi")]
        public string displayName = "Quai moi";
        public Sprite icon;
        public GameObject prefab;

        [Header("Hoat hinh (Tiny Swords)")]
        public Sprite[] khungDung;
        public Sprite[] khungChay;
        public Sprite[] khungDanh;
        [Tooltip("Ti le hinh tren san dau")]
        public float coHinh = 0.85f;
        [Tooltip("Cao thanh mau tinh tu chan (don vi the gioi)")]
        public float caoThanhMau = 1.05f;
        [Tooltip("Danh xa: ban mui ten thay vi chem")]
        public bool danhXa;
        public Sprite muiTen;

        [Header("Phe")]
        public Faction faction = Faction.Dungeon;

        [Header("Chi so goc (cap 1)")]
        [Min(1)] public int maxHp = 20;
        [Min(1)] public int damage = 3;
        public DamageType damageType = DamageType.Physical;
        [Tooltip("So giay giua hai lan danh")]
        [Min(0.1f)] public float attackInterval = 1f;
        [Min(0.1f)] public float attackRange = 0.8f;
        [Min(0f)] public float moveSpeed = 1.5f;

        [Header("Khang sat thuong (0 = khong khang, 0.5 = giam 50%)")]
        [Range(0f, 0.9f)] public float physicalResist = 0f;
        [Range(0f, 0.9f)] public float magicResist = 0f;

        [Header("Trieu hoi (chi ap dung cho phe Dungeon)")]
        public ResourceType summonResource = ResourceType.Souls;
        [Min(0)] public int summonCost = 5;

        [Header("Len cap")]
        [Tooltip("Luong thit can de len 1 cap o Dien Tho")]
        [Min(1)] public int meatPerLevel = 5;
        [Tooltip("He so nhan chi so moi cap")]
        public float statGrowthPerLevel = 0.25f;

        [Header("Phan thuong khi bi tieu diet (phe Invader)")]
        public ResourceCost[] killRewards;

        /// <summary>Tinh HP theo cap (cap 1 = chi so goc).</summary>
        public int GetHpAtLevel(int level)
            => Mathf.RoundToInt(maxHp * (1f + statGrowthPerLevel * (level - 1)));

        /// <summary>Tinh sat thuong theo cap.</summary>
        public int GetDamageAtLevel(int level)
            => Mathf.RoundToInt(damage * (1f + statGrowthPerLevel * (level - 1)));

        public float GetResist(DamageType type)
            => type == DamageType.Physical ? physicalResist : magicResist;
    }
}
