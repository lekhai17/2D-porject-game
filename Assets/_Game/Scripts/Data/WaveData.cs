using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Mot dot tan cong. Timeline tren dau man hinh Battle View doc du lieu tu day
    /// de bao truoc nguoi choi sap gap loai sat thuong nao.
    /// Assets > Create > Kingdom Ruins > Wave Data
    /// </summary>
    [CreateAssetMenu(fileName = "Wave_", menuName = "Kingdom Ruins/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Serializable]
        public struct SpawnEntry
        {
            public MinionData minion;
            [Min(1)] public int count;
            [Tooltip("Giay giua tung con trong nhom nay")]
            [Min(0f)] public float intervalBetween;
            [Tooltip("Giay cho truoc khi nhom nay bat dau vao")]
            [Min(0f)] public float startDelay;
        }

        [Header("Thong tin")]
        public string waveName = "Dot tan cong";
        [Tooltip("Ngay thu may thi dot nay xuat hien")]
        [Min(1)] public int dayNumber = 1;

        [Header("Doi hinh")]
        public List<SpawnEntry> entries = new List<SpawnEntry>();

        [Header("Phan thuong khi song sot qua dot nay")]
        public ResourceCost[] clearRewards;

        /// <summary>Tong so quan xam nhap cua dot nay.</summary>
        public int TotalCount
        {
            get
            {
                int total = 0;
                foreach (var e in entries)
                    if (e.minion != null) total += e.count;
                return total;
            }
        }

        /// <summary>Loai sat thuong chiem uu the, dung cho canh bao tren timeline.</summary>
        public DamageType DominantDamageType()
        {
            int phys = 0, magic = 0;
            foreach (var e in entries)
            {
                if (e.minion == null) continue;
                if (e.minion.damageType == DamageType.Physical) phys += e.count;
                else magic += e.count;
            }
            return magic > phys ? DamageType.Magic : DamageType.Physical;
        }
    }
}
