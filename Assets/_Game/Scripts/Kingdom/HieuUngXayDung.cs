using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Hieu ung luc xay: tho cam bua go, khoi bui bay len, xong moi hien cong trinh.
    /// Cong trinh bi tat trong luc thi cong nen chua san xuat duoc.
    /// </summary>
    public class HieuUngXayDung : MonoBehaviour
    {
        [Header("Hinh anh")]
        [SerializeField] private List<Sprite> frameBui = new List<Sprite>();
        [SerializeField] private List<Sprite> frameTho = new List<Sprite>();
        [SerializeField] private Material vatLieu;

        [Header("Thoi gian")]
        [SerializeField] private float thoiGianXay = 2.2f;
        [SerializeField] private float nhipTho = 0.16f;      // giay moi frame bua
        [SerializeField] private float nhipBui = 0.07f;      // giay moi frame bui
        [SerializeField] private float cachQuangBui = 0.34f; // giay giua hai cum bui

        [Header("Hien cong trinh")]
        [SerializeField] private float thoiGianBat = 0.35f;
        [SerializeField] private AnimationCurve duongBat =
            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.6f, 1.12f), new Keyframe(1f, 1f));

        /// <summary>Chay toan bo hieu ung cho mot cong trinh vua dat.</summary>
        public void Chay(ProductionBuilding ct, Vector3 tamODat, Vector2Int coODat)
        {
            if (ct == null) return;
            StartCoroutine(TienTrinh(ct, tamODat, coODat));
        }

        private IEnumerator TienTrinh(ProductionBuilding ct, Vector3 tam, Vector2Int coO)
        {
            var sr = ct.GetComponent<SpriteRenderer>();
            Vector3 coGoc = ct.transform.localScale;

            // Tat cong trinh trong luc thi cong
            ct.enabled = false;
            if (sr != null) sr.enabled = false;

            // Tho dung ben canh khu dat
            GameObject tho = null;
            if (frameTho.Count > 0)
            {
                tho = new GameObject("ThoXayDung");
                tho.transform.position = new Vector3(tam.x - coO.x * 0.5f + 0.7f, tam.y - coO.y * 0.5f + 0.5f, 0f);
                var srT = tho.AddComponent<SpriteRenderer>();
                srT.sprite = frameTho[0];
                if (vatLieu != null) srT.sharedMaterial = vatLieu;
                srT.sortingOrder = ThuTuVe.TheoY(tho.transform.position.y, 2);
                StartCoroutine(ChayVongLap(srT, frameTho, nhipTho));
            }

            // Khoi bui bung len tung cum quanh khu dat
            float troi = 0f;
            float lanBui = 0f;
            while (troi < thoiGianXay)
            {
                troi += Time.deltaTime;
                lanBui += Time.deltaTime;
                if (lanBui >= cachQuangBui && frameBui.Count > 0)
                {
                    lanBui = 0f;
                    var lech = new Vector3(
                        Random.Range(-coO.x * 0.35f, coO.x * 0.35f),
                        Random.Range(-coO.y * 0.3f, coO.y * 0.1f), 0f);
                    StartCoroutine(MotCumBui(tam + lech));
                }
                yield return null;
            }

            if (tho != null) Destroy(tho);

            // Cum bui lon luc cong trinh hien ra
            if (frameBui.Count > 0)
                for (int i = 0; i < 3; i++)
                    StartCoroutine(MotCumBui(tam + new Vector3((i - 1) * coO.x * 0.3f, -coO.y * 0.2f, 0f), 1.5f));

            // Cong trinh bat len
            if (sr != null) sr.enabled = true;
            float t = 0f;
            while (t < thoiGianBat)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / thoiGianBat);
                ct.transform.localScale = coGoc * duongBat.Evaluate(p);
                yield return null;
            }
            ct.transform.localScale = coGoc;

            ct.enabled = true;
        }

        private IEnumerator MotCumBui(Vector3 viTri, float coNhan = 1f)
        {
            var go = new GameObject("Bui");
            go.transform.position = viTri;
            go.transform.localScale = Vector3.one * coNhan * Random.Range(0.85f, 1.25f);
            var sr = go.AddComponent<SpriteRenderer>();
            if (vatLieu != null) sr.sharedMaterial = vatLieu;
            sr.sortingOrder = ThuTuVe.TheoY(viTri.y, 40);

            for (int i = 0; i < frameBui.Count; i++)
            {
                sr.sprite = frameBui[i];
                // Bui bay len nhe dan
                go.transform.position += Vector3.up * 0.04f;
                yield return new WaitForSeconds(nhipBui);
            }
            Destroy(go);
        }

        private IEnumerator ChayVongLap(SpriteRenderer sr, List<Sprite> frame, float nhip)
        {
            int i = 0;
            while (sr != null)
            {
                sr.sprite = frame[i % frame.Count];
                i++;
                yield return new WaitForSeconds(nhip);
            }
        }
    }
}
