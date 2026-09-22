using System;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Mot chien binh tren san dau. Tu tim dich gan nhat.
    /// Phe ta (Dungeon): giu vi tri goc, chi xong len khi dich vao vung canh.
    /// Phe dich (Invader): di theo duong vao cong thanh, gap linh thi danh.
    /// Co hoat hinh dung / chay / danh, thanh mau tren dau, mui ten neu danh xa.
    /// </summary>
    public class Combatant : MonoBehaviour
    {
        [SerializeField] private MinionData data;
        [SerializeField] private Faction faction = Faction.Dungeon;
        [SerializeField] private int level = 1;

        [Tooltip("Linh phe ta chi duoi theo dich trong ban kinh nay tinh tu vi tri goc")]
        public float banKinhCanh = 6f;

        private int currentHp;
        private int maxHp;
        private int damage;
        private float attackCooldown;
        private float henDanh;
        private LanePath path;
        private int waypointIndex;

        private SpriteRenderer sr;
        private HoatHinhKhung hh;
        private SpriteRenderer mauNen, mauDay;
        private static Sprite pixelTrang;

        public Faction Faction => faction;
        public MinionData Data => data;
        public int Level => level;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public bool IsAlive => currentHp > 0;
        public float HpPercent => maxHp > 0 ? (float)currentHp / maxHp : 0f;
        public MinionInstance SourceInstance { get; private set; }
        public Vector3 ViTriGoc { get; set; }

        public event Action<Combatant> OnDied;
        public event Action<Combatant> OnHpChanged;

        public void Setup(MinionData minionData, Faction side, int lvl = 1, MinionInstance source = null)
        {
            data = minionData;
            faction = side;
            level = Mathf.Max(1, lvl);
            SourceInstance = source;

            maxHp = data != null ? data.GetHpAtLevel(level) : 1;
            damage = data != null ? data.GetDamageAtLevel(level) : 1;
            currentHp = maxHp;
            attackCooldown = 0f;
            ViTriGoc = transform.position;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            if (data != null)
            {
                transform.localScale = Vector3.one * (data.coHinh > 0f ? data.coHinh : 1f);
                if (data.khungDung != null && data.khungDung.Length > 0)
                {
                    hh = GetComponent<HoatHinhKhung>();
                    if (hh == null) hh = gameObject.AddComponent<HoatHinhKhung>();
                    hh.Dat(data.khungDung, 8f, true);
                }
                else if (data.icon != null && sr.sprite == null) sr.sprite = data.icon;
            }

            if (mauNen == null) TaoThanhMau();
            CapNhatThanhMau();
            OnHpChanged?.Invoke(this);
        }

        public void SetPath(LanePath lanePath)
        {
            path = lanePath;
            waypointIndex = 0;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (!IsAlive || data == null) return;
            if (gm == null || gm.Phase != GamePhase.Battle || gm.IsPaused) return;

            float dt = gm.ScaledDelta;
            CapNhatThuTuVe();
            if (attackCooldown > 0f) attackCooldown -= dt;

            // Dang ra don thi dung yen cho het dong tac
            if (henDanh > 0f)
            {
                henDanh -= dt;
                if (henDanh <= 0f) DatDung();
                return;
            }

            var target = BattleManager.Instance != null ? BattleManager.Instance.FindNearestEnemy(this) : null;

            if (target != null && Vector3.Distance(transform.position, target.transform.position) <= data.attackRange)
            {
                QuayVe(target.transform.position);
                TryAttack(target);
                return;
            }

            if (faction == Faction.Invader)
            {
                MoveAlongPath(dt, target);
                return;
            }

            // Phe ta: giu vi tri, dich vao vung canh moi xong len
            if (target != null && Vector3.Distance(target.transform.position, ViTriGoc) <= banKinhCanh)
                MoveToward(target.transform.position, dt);
            else if (Vector3.Distance(transform.position, ViTriGoc) > 0.08f)
                MoveToward(ViTriGoc, dt);
            else
                DatDung();
        }

        private void MoveAlongPath(float dt, Combatant nearbyTarget)
        {
            if (nearbyTarget != null &&
                Vector3.Distance(transform.position, nearbyTarget.transform.position) <= data.attackRange * 3f)
            {
                MoveToward(nearbyTarget.transform.position, dt);
                return;
            }

            if (path == null || path.Count == 0) { ReachDungeonCore(); return; }

            Vector3 goal = path.GetPoint(waypointIndex);
            MoveToward(goal, dt);

            if (Vector3.Distance(transform.position, goal) <= 0.08f)
            {
                if (path.IsLastIndex(waypointIndex)) ReachDungeonCore();
                else waypointIndex++;
            }
        }

        private void MoveToward(Vector3 goal, float dt)
        {
            if (data.moveSpeed <= 0f) return;
            transform.position = Vector3.MoveTowards(transform.position, goal, data.moveSpeed * dt);
            QuayVe(goal);
            DatChay();
        }

        private void QuayVe(Vector3 diem)
        {
            float dx = diem.x - transform.position.x;
            if (sr != null && Mathf.Abs(dx) > 0.01f) sr.flipX = dx < 0f;
        }

        private void TryAttack(Combatant target)
        {
            if (attackCooldown > 0f) { if (henDanh <= 0f) DatDung(); return; }
            attackCooldown = data.attackInterval;

            // Dong tac danh
            if (hh != null && data.khungDanh != null && data.khungDanh.Length > 0)
            {
                float thoiGian = Mathf.Min(0.6f, data.attackInterval * 0.8f);
                hh.Dat(null, 1f, true);                            // ep reset bo khung
                hh.Dat(data.khungDanh, data.khungDanh.Length / thoiGian, false);
                henDanh = thoiGian;
            }

            if (data.danhXa && data.muiTen != null)
                MuiTenBay.Ban(data.muiTen, transform.position + Vector3.up * 0.55f, target, damage, data.damageType,
                              BattleManager.Instance != null ? BattleManager.Instance.VatLieuLinh : null);
            else
                target.TakeDamage(damage, data.damageType);
        }

        private void ReachDungeonCore()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.DamageDungeon(Mathf.Max(1, damage));
            Die(rewardKill: false);
        }

        public void TakeDamage(int amount, DamageType type)
        {
            if (!IsAlive || data == null) return;
            float resist = data.GetResist(type);
            int final = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - resist)));
            currentHp = Mathf.Max(0, currentHp - final);
            CapNhatThanhMau();
            OnHpChanged?.Invoke(this);
            if (currentHp <= 0) Die(rewardKill: true);
        }

        private void Die(bool rewardKill)
        {
            if (rewardKill && faction == Faction.Invader && data != null &&
                data.killRewards != null && ResourceManager.Instance != null)
                foreach (var r in data.killRewards)
                    ResourceManager.Instance.Add(r.type, r.amount);

            // Khoi bui luc nga xuong
            var kho = BattleManager.Instance != null ? BattleManager.Instance.Kho : null;
            if (kho != null)
                HatHieuUng.Tao(kho.khoi, transform.position + Vector3.up * 0.3f, 0.6f, Vector3.up * 0.3f, 0.5f,
                               new Color(0.9f, 0.9f, 0.9f, 0.9f), 0.5f, kho.vatLieuThuong,
                               ThuTuVe.TheoY(transform.position.y, 5), null);

            currentHp = 0;
            OnDied?.Invoke(this);
            Destroy(gameObject);
        }

        public void FullHeal()
        {
            currentHp = maxHp;
            CapNhatThanhMau();
            OnHpChanged?.Invoke(this);
        }

        // ---------- Hoat hinh ----------
        private void DatDung()
        {
            if (hh != null && data.khungDung != null && data.khungDung.Length > 0) hh.Dat(data.khungDung, 8f, true);
        }

        private void DatChay()
        {
            if (hh == null) return;
            var k = data.khungChay != null && data.khungChay.Length > 0 ? data.khungChay : data.khungDung;
            if (k != null && k.Length > 0) hh.Dat(k, 10f, true);
        }

        private void CapNhatThuTuVe()
        {
            if (sr == null) return;
            sr.sortingOrder = ThuTuVe.TheoY(transform.position.y);
            if (mauNen != null) { mauNen.sortingOrder = sr.sortingOrder + 30; mauDay.sortingOrder = sr.sortingOrder + 31; }
        }

        // ---------- Thanh mau ----------
        private void TaoThanhMau()
        {
            if (pixelTrang == null)
            {
                var t = new Texture2D(4, 4) { filterMode = FilterMode.Point };
                var px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                t.SetPixels(px); t.Apply();
                pixelTrang = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
            }
            float s = Mathf.Max(0.01f, transform.localScale.x);
            float rong = 0.7f;
            float cao = data != null ? data.caoThanhMau : 1f;

            var nen = new GameObject("ThanhMau");
            nen.transform.SetParent(transform, false);
            nen.transform.localPosition = new Vector3(-rong * 0.5f / s, cao / s, 0f);
            nen.transform.localScale = new Vector3(rong / s, 0.09f / s, 1f);
            mauNen = nen.AddComponent<SpriteRenderer>();
            mauNen.sprite = pixelTrang;
            mauNen.color = new Color(0.08f, 0.05f, 0.03f, 0.85f);

            var day = new GameObject("Mau");
            day.transform.SetParent(nen.transform, false);
            mauDay = day.AddComponent<SpriteRenderer>();
            mauDay.sprite = pixelTrang;

            var kho = BattleManager.Instance != null ? BattleManager.Instance.Kho : null;
            if (kho != null && kho.vatLieuSang != null) { mauNen.sharedMaterial = kho.vatLieuSang; mauDay.sharedMaterial = kho.vatLieuSang; }
        }

        private void CapNhatThanhMau()
        {
            if (mauDay == null) return;
            mauDay.color = faction == Faction.Dungeon ? new Color(0.45f, 0.85f, 0.35f) : new Color(0.92f, 0.3f, 0.25f);
            mauDay.transform.localScale = new Vector3(Mathf.Max(0f, HpPercent), 1f, 1f);
            bool hien = currentHp < maxHp && currentHp > 0;
            mauNen.enabled = hien;
            mauDay.enabled = hien;
        }
    }
}
