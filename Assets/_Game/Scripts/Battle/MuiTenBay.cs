using UnityEngine;

namespace KingdomRuins
{
    /// <summary>Mui ten bay toi dich, trung moi gay sat thuong. Dich chet giua duong thi cam xuong dat.</summary>
    public class MuiTenBay : MonoBehaviour
    {
        public Combatant dich;
        public int satThuong;
        public DamageType loai;
        public float tocDo = 11f;
        private Vector3 diemCuoi;

        public static void Ban(Sprite sp, Vector3 tu, Combatant dich, int st, DamageType loai, Material vl)
        {
            if (sp == null || dich == null) return;
            var go = new GameObject("MuiTen");
            go.transform.position = tu;
            go.transform.localScale = Vector3.one * 0.7f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            if (vl != null) sr.sharedMaterial = vl;
            sr.sortingOrder = ThuTuVe.TrenCung - 100;
            var m = go.AddComponent<MuiTenBay>();
            m.dich = dich; m.satThuong = st; m.loai = loai;
            m.diemCuoi = dich.transform.position + Vector3.up * 0.45f;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            float dt = gm != null ? gm.ScaledDelta : Time.deltaTime;
            if (dich != null && dich.IsAlive) diemCuoi = dich.transform.position + Vector3.up * 0.45f;

            var huong = diemCuoi - transform.position;
            float buoc = tocDo * dt;
            if (huong.magnitude <= buoc)
            {
                if (dich != null && dich.IsAlive) dich.TakeDamage(satThuong, loai);
                Destroy(gameObject);
                return;
            }
            transform.position += huong.normalized * buoc;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(huong.y, huong.x) * Mathf.Rad2Deg);
        }
    }
}
