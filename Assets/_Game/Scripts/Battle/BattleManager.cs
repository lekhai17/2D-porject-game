using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Dieu khien mot tran danh: spawn tung dot theo WaveData, theo doi
    /// quan song sot hai ben, va bao cho GameManager khi tran ket thuc.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Duong di & o trieu hoi")]
        [SerializeField] private LanePath lanePath;
        [SerializeField] private List<SummonSlot> summonSlots = new List<SummonSlot>();

        [Tooltip("Het o thi linh du xep hang sau cong, bat dau tu diem nay")]
        [SerializeField] private Vector2 gocHangDuPhong = new Vector2(91.8f, -1.5f);

        [Header("Hinh anh")]
        [SerializeField] private KhoHoatHinh kho;
        [SerializeField] private Material vatLieuLinh;
        public KhoHoatHinh Kho => kho;
        public Material VatLieuLinh => vatLieuLinh;

        private readonly Dictionary<Combatant, MinionInstance> linhCuaTa = new Dictionary<Combatant, MinionInstance>();

        [Header("Cac dot tan cong theo ngay")]
        [Tooltip("Neu khong tim thay wave cho ngay hien tai, se dung wave cuoi cung va tang do kho")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        [Header("Do kho tu dong khi het wave dinh nghia san")]
        [Tooltip("Moi ngay vuot qua so wave co san se nhan them so luong quan nay")]
        [SerializeField] private float extraCountPerDay = 0.5f;

        private readonly List<Combatant> invaders = new List<Combatant>();
        private readonly List<Combatant> defenders = new List<Combatant>();

        private bool spawning;
        private Coroutine spawnRoutine;

        public IReadOnlyList<SummonSlot> Slots => summonSlots;
        public LanePath Path => lanePath;
        public WaveData CurrentWave { get; private set; }
        public int InvadersAlive => invaders.Count;
        public int DefendersAlive => defenders.Count;

        public event Action<WaveData> OnBattleStarted;
        public event Action<bool> OnBattleEnded;      // true = song sot
        public event Action OnCombatantsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (summonSlots.Count == 0)
                summonSlots.AddRange(GetComponentsInChildren<SummonSlot>());
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.Battle) StartBattle();
        }

        // ---------- Vong doi tran danh ----------

        public void StartBattle()
        {
            int day = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
            CurrentWave = PickWaveForDay(day);

            invaders.Clear();
            TrienKhaiQuan();

            if (CurrentWave == null)
            {
                Debug.LogWarning("[BattleManager] Chua co WaveData nao - bo qua tran danh.");
                EndBattle(survived: true);
                return;
            }

            OnBattleStarted?.Invoke(CurrentWave);

            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(SpawnWave(CurrentWave, day));
        }

        /// <summary>Du bao dot quai cua mot ngay: tung loai quai va so luong (da tinh phan tang them theo ngay).</summary>
        public List<KeyValuePair<MinionData, int>> DuBaoDot(int day)
        {
            var kq = new List<KeyValuePair<MinionData, int>>();
            var w = PickWaveForDay(day);
            if (w == null) return kq;
            int bonus = Mathf.Max(0, Mathf.FloorToInt((day - w.dayNumber) * extraCountPerDay));
            var dem = new Dictionary<MinionData, int>();
            var thuTu = new List<MinionData>();
            foreach (var e in w.entries)
            {
                if (e.minion == null) continue;
                if (!dem.ContainsKey(e.minion)) { dem[e.minion] = 0; thuTu.Add(e.minion); }
                dem[e.minion] += e.count + bonus;
            }
            foreach (var m in thuTu) kq.Add(new KeyValuePair<MinionData, int>(m, dem[m]));
            return kq;
        }

        /// <summary>Cap quai theo ngay, cung cong thuc luc sinh quai.</summary>
        public int CapQuaiTheoNgay(int day) => Mathf.Max(1, 1 + (day - 1) / 3);

        private WaveData PickWaveForDay(int day)
        {
            // Tim wave khai bao dung ngay nay
            foreach (var w in waves)
                if (w != null && w.dayNumber == day) return w;

            // Khong co thi dung wave cuoi cung lam khuon
            for (int i = waves.Count - 1; i >= 0; i--)
                if (waves[i] != null) return waves[i];

            return null;
        }

        private IEnumerator SpawnWave(WaveData wave, int day)
        {
            spawning = true;

            // Do kho tang dan khi da vuot qua so wave khai bao san
            int bonus = Mathf.Max(0, Mathf.FloorToInt((day - wave.dayNumber) * extraCountPerDay));

            foreach (var entry in wave.entries)
            {
                if (entry.minion == null) continue;

                if (entry.startDelay > 0f)
                    yield return WaitScaled(entry.startDelay);

                int total = entry.count + bonus;
                for (int i = 0; i < total; i++)
                {
                    SpawnInvader(entry.minion, day);
                    if (entry.intervalBetween > 0f)
                        yield return WaitScaled(entry.intervalBetween);
                }
            }

            spawning = false;
            CheckBattleOver();
        }

        /// <summary>Cho theo toc do game, dung lai khi pause.</summary>
        private IEnumerator WaitScaled(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                var gm = GameManager.Instance;
                if (gm == null || gm.Phase != GamePhase.Battle) yield break;
                t += gm.ScaledDelta;
                yield return null;
            }
        }

        private void SpawnInvader(MinionData data, int day)
        {
            Vector3 pos = lanePath != null ? lanePath.Start : transform.position;

            // Quan xam nhap manh dan theo ngay
            int level = Mathf.Max(1, 1 + (day - 1) / 3);
            var c = TaoCombatant(data, Faction.Invader, level, pos, null);
            c.SetPath(lanePath);

            invaders.Add(c);
            c.OnDied += HandleInvaderDied;
            OnCombatantsChanged?.Invoke();
        }

        /// <summary>Dua toan bo linh trong roster ra vi tri canh gac gan cong.</summary>
        private void TrienKhaiQuan()
        {
            linhCuaTa.Clear();
            var ro = MinionRoster.Instance;
            if (ro == null) return;
            var ds = new List<MinionInstance>(ro.All);
            for (int i = 0; i < ds.Count; i++)
            {
                var inst = ds[i];
                if (inst == null || inst.data == null) continue;
                Vector3 pos;
                if (i < summonSlots.Count && summonSlots[i] != null) pos = summonSlots[i].SpawnPosition;
                else
                {
                    int j = i - summonSlots.Count;
                    pos = new Vector3(gocHangDuPhong.x + (j / 4) * 0.9f, gocHangDuPhong.y + (j % 4) * 1.0f, 0f);
                }
                var c = TaoCombatant(inst.data, Faction.Dungeon, inst.level, pos, inst);
                c.ViTriGoc = pos;
                linhCuaTa[c] = inst;
                RegisterDefender(c);
            }
        }

        private Combatant TaoCombatant(MinionData data, Faction phe, int level, Vector3 pos, MinionInstance nguon)
        {
            var go = data.prefab != null
                ? Instantiate(data.prefab, pos, Quaternion.identity)
                : new GameObject((phe == Faction.Dungeon ? "Linh_" : "Dich_") + data.displayName);
            go.transform.position = pos;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            if (vatLieuLinh != null) sr.sharedMaterial = vatLieuLinh;
            var c = go.GetComponent<Combatant>();
            if (c == null) c = go.AddComponent<Combatant>();
            c.Setup(data, phe, level, nguon);
            return c;
        }

        private void HandleInvaderDied(Combatant c)
        {
            c.OnDied -= HandleInvaderDied;
            invaders.Remove(c);
            OnCombatantsChanged?.Invoke();
            CheckBattleOver();
        }

        private void CheckBattleOver()
        {
            if (spawning || invaders.Count > 0) return;
            if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Battle) return;

            EndBattle(survived: true);
        }

        private void EndBattle(bool survived)
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
            spawning = false;

            if (survived && CurrentWave != null && CurrentWave.clearRewards != null &&
                ResourceManager.Instance != null)
            {
                foreach (var r in CurrentWave.clearRewards)
                    ResourceManager.Instance.Add(r.type, r.amount);
            }

            // Thu hoi quai con song ve ham nguc, giu nguyen cap
            foreach (var slot in summonSlots)
            {
                if (slot == null || !slot.IsOccupied) continue;
                slot.Occupant.FullHeal();
                slot.Withdraw();
            }

            // Linh phe ta con song: hoi mau va rut ve thanh (giu trong roster)
            foreach (var d in defenders.ToArray())
            {
                if (d == null) continue;
                d.OnDied -= HandleDefenderDied;
                Destroy(d.gameObject);
            }
            linhCuaTa.Clear();

            // Don sach quan xam nhap con lai
            for (int i = invaders.Count - 1; i >= 0; i--)
                if (invaders[i] != null) Destroy(invaders[i].gameObject);
            invaders.Clear();
            defenders.Clear();
            OnCombatantsChanged?.Invoke();

            OnBattleEnded?.Invoke(survived);

            if (GameManager.Instance != null)
                GameManager.Instance.FinishBattle(survived);
        }

        // ---------- Dang ky quan phong thu ----------

        public void RegisterDefender(Combatant c)
        {
            if (c == null || defenders.Contains(c)) return;
            defenders.Add(c);
            c.OnDied += HandleDefenderDied;
            OnCombatantsChanged?.Invoke();
        }

        public void UnregisterDefender(Combatant c)
        {
            if (c == null) return;
            c.OnDied -= HandleDefenderDied;
            if (defenders.Remove(c)) OnCombatantsChanged?.Invoke();
        }

        private void HandleDefenderDied(Combatant c)
        {
            c.OnDied -= HandleDefenderDied;
            defenders.Remove(c);
            // Linh chet tran thi mat han khoi quan so
            if (linhCuaTa.TryGetValue(c, out var inst))
            {
                linhCuaTa.Remove(c);
                if (MinionRoster.Instance != null) MinionRoster.Instance.Remove(inst);
            }
            OnCombatantsChanged?.Invoke();
        }

        // ---------- Tim muc tieu ----------

        /// <summary>Tim dich gan nhat cua mot don vi. Dung boi Combatant moi frame.</summary>
        public Combatant FindNearestEnemy(Combatant self)
        {
            if (self == null) return null;

            var pool = self.Faction == Faction.Dungeon ? invaders : defenders;
            Combatant best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < pool.Count; i++)
            {
                var other = pool[i];
                if (other == null || !other.IsAlive) continue;

                float d = (other.transform.position - self.transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = other;
                }
            }
            return best;
        }

        /// <summary>O trong dau tien con dung duoc.</summary>
        public SummonSlot GetFreeSlot()
        {
            foreach (var s in summonSlots)
                if (s != null && !s.IsLocked && !s.IsOccupied) return s;
            return null;
        }

        /// <summary>Dat quai vao o trong dau tien. Tra ve null neu het o.</summary>
        public Combatant DeployToFreeSlot(MinionInstance instance)
        {
            var slot = GetFreeSlot();
            return slot != null ? slot.Deploy(instance) : null;
        }
    }
}
