using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Mot o trieu hoi trong Battle View (o luc giac / tron o goc man hinh,
    /// giong co che dat quan cua The King is Watching). Nguoi choi chon quai
    /// tu roster de dat vao day, quai se tu chien dau.
    /// </summary>
    public class SummonSlot : MonoBehaviour
    {
        [Tooltip("Vi tri quai se dung khi duoc dat vao o nay. De trong se dung chinh transform.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("O nay bi khoa cho den khi mo bang tai nguyen / tien do")]
        [SerializeField] private bool locked;

        public bool IsOccupied => Occupant != null;
        public bool IsLocked => locked;
        public Combatant Occupant { get; private set; }
        public MinionInstance BoundInstance { get; private set; }

        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        /// <summary>Dat mot con quai tu roster vao o nay.</summary>
        public Combatant Deploy(MinionInstance instance)
        {
            if (locked || IsOccupied || instance == null || instance.data == null) return null;

            GameObject go;
            if (instance.data.prefab != null)
            {
                go = Instantiate(instance.data.prefab, SpawnPosition, Quaternion.identity);
            }
            else
            {
                go = new GameObject("Minion_" + instance.data.displayName);
                go.transform.position = SpawnPosition;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = instance.data.icon;
                sr.sortingOrder = 5;
            }

            var c = go.GetComponent<Combatant>();
            if (c == null) c = go.AddComponent<Combatant>();
            c.Setup(instance.data, Faction.Dungeon, instance.level, instance);

            Occupant = c;
            BoundInstance = instance;
            c.OnDied += HandleOccupantDied;

            if (BattleManager.Instance != null)
                BattleManager.Instance.RegisterDefender(c);

            return c;
        }

        /// <summary>Thu hoi quai khoi o (khong tinh la chet).</summary>
        public void Withdraw()
        {
            if (Occupant == null) return;

            Occupant.OnDied -= HandleOccupantDied;
            if (BattleManager.Instance != null)
                BattleManager.Instance.UnregisterDefender(Occupant);

            Destroy(Occupant.gameObject);
            Clear();
        }

        private void HandleOccupantDied(Combatant c)
        {
            if (c != null) c.OnDied -= HandleOccupantDied;

            // Quai chet trong tran thi bi loai khoi roster
            if (BoundInstance != null && MinionRoster.Instance != null)
                MinionRoster.Instance.Remove(BoundInstance);

            Clear();
        }

        private void Clear()
        {
            Occupant = null;
            BoundInstance = null;
        }

        public void SetLocked(bool value) => locked = value;

        private void OnDrawGizmos()
        {
            Gizmos.color = locked ? new Color(0.4f, 0.4f, 0.4f, 0.5f)
                                  : new Color(0.6f, 0.3f, 0.9f, 0.5f);
            Gizmos.DrawWireSphere(SpawnPosition, 0.35f);
        }
    }
}
