using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

namespace KingdomRuins
{
    /// <summary>
    /// Ban do thu nho. Quet cac lop tilemap trong scene roi ve ra mot texture,
    /// nen luon khop voi ban do that ma khong can ve tay.
    /// Kem mot khung cho biet camera dang nhin vung nao.
    /// </summary>
    public class BanDoThuNho : MonoBehaviour
    {
        [System.Serializable]
        public class LopVe
        {
            [Tooltip("Ten doi tuong tilemap, vi du Tilemap_Ground")]
            public string tenLop;
            public Color mau = Color.white;
        }

        [Header("Nguon du lieu")]
        [SerializeField] private Grid luoi;
        [SerializeField] private Vector2Int kichThuoc = new Vector2Int(56, 52);

        [Tooltip("Ve theo thu tu tu tren xuong, lop sau de len lop truoc")]
        [SerializeField]
        private List<LopVe> cacLop = new List<LopVe>();

        [Header("Cham danh dau cong trinh")]
        [SerializeField] private string tenNhomCongTrinh = "KhuLauDai";
        [SerializeField] private Color mauCongTrinh = new Color32(0x4C, 0x6F, 0xB0, 255);
        [SerializeField] private int coCham = 2;

        [Header("Hien thi")]
        [SerializeField] private RawImage anhBanDo;
        [SerializeField] private RectTransform khungNhin;
        [SerializeField] private Camera camTheoDoi;

        private Texture2D texture;

        private void Start()
        {
            Dung();
        }

        /// <summary>Quet lai toan bo tilemap va ve lai texture.</summary>
        public void Dung()
        {
            if (luoi == null)
            {
                var g = GameObject.Find("BuildGrid");
                if (g != null) luoi = g.GetComponent<Grid>();
            }
            if (luoi == null) { Debug.LogWarning("[BanDoThuNho] Khong tim thay luoi"); return; }

            int W = kichThuoc.x, H = kichThuoc.y;
            if (texture == null || texture.width != W || texture.height != H)
            {
                texture = new Texture2D(W, H, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
            }

            var diem = new Color[W * H];
            for (int i = 0; i < diem.Length; i++) diem[i] = new Color(0, 0, 0, 0);

            // Ve tung lop theo thu tu khai bao
            foreach (var lop in cacLop)
            {
                if (string.IsNullOrEmpty(lop.tenLop)) continue;
                var t = luoi.transform.Find(lop.tenLop);
                if (t == null) continue;
                var tm = t.GetComponent<Tilemap>();
                if (tm == null) continue;

                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                        if (tm.GetTile(new Vector3Int(x, y, 0)) != null)
                            diem[y * W + x] = lop.mau;
            }

            // Cham cho tung cong trinh
            var nhom = GameObject.Find(tenNhomCongTrinh);
            if (nhom != null)
                foreach (Transform ct in nhom.transform)
                {
                    int cx = Mathf.RoundToInt(ct.position.x);
                    int cy = Mathf.RoundToInt(ct.position.y);
                    for (int dy = 0; dy < coCham; dy++)
                        for (int dx = 0; dx < coCham; dx++)
                        {
                            int x = cx + dx, y = cy + dy;
                            if (x < 0 || x >= W || y < 0 || y >= H) continue;
                            diem[y * W + x] = mauCongTrinh;
                        }
                }

            texture.SetPixels(diem);
            texture.Apply();

            if (anhBanDo != null)
            {
                anhBanDo.texture = texture;
                anhBanDo.color = Color.white;
            }
        }

        private void Update()
        {
            if (khungNhin == null || anhBanDo == null) return;

            var cam = camTheoDoi;
            if (cam == null)
            {
                // Uu tien camera dang bat trong CameraRig
                var rig = GameObject.Find("CameraRig");
                if (rig != null)
                    foreach (var c in rig.GetComponentsInChildren<Camera>(true))
                        if (c.enabled) { cam = c; break; }
                if (cam == null) cam = Camera.main;
                camTheoDoi = cam;
            }
            if (cam == null || !cam.orthographic) return;

            var vung = anhBanDo.rectTransform.rect;
            float tyLeX = vung.width / kichThuoc.x;
            float tyLeY = vung.height / kichThuoc.y;

            float rongNhin = cam.orthographicSize * 2f * cam.aspect;
            float caoNhin = cam.orthographicSize * 2f;

            khungNhin.sizeDelta = new Vector2(rongNhin * tyLeX, caoNhin * tyLeY);
            khungNhin.anchoredPosition = new Vector2(
                cam.transform.position.x * tyLeX,
                cam.transform.position.y * tyLeY);
        }
    }
}
