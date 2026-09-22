using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Thanh tai nguyen tren dau man hinh. Tu dung cac cum o va cap nhat
    /// so lieu moi khi ResourceManager bao co thay doi.
    /// </summary>
    public class ThanhTaiNguyen : MonoBehaviour
    {
        [System.Serializable]
        public class OTaiNguyen
        {
            public ResourceType loai;
            public Sprite icon;
        }

        [System.Serializable]
        public class CumTaiNguyen
        {
            public string tenCum = "Cum";
            public List<OTaiNguyen> danhSach = new List<OTaiNguyen>();
        }

        [Header("Bo cuc")]
        [SerializeField] private List<CumTaiNguyen> cacCum = new List<CumTaiNguyen>();
        [SerializeField] private Sprite khungCum;

        [Header("Kich thuoc")]
        [SerializeField] private int caoThanh = 68;
        [SerializeField] private int coIcon = 40;
        [SerializeField] private int rongSo = 54;
        [SerializeField] private int demGiuaCum = 10;

        private readonly Dictionary<ResourceType, Text> oChu = new Dictionary<ResourceType, Text>();

        private void Start()
        {
            Dung();
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += CapNhat;
                foreach (var cap in oChu)
                    cap.Value.text = ResourceManager.Instance.Get(cap.Key).ToString();
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= CapNhat;
        }

        private void CapNhat(ResourceType loai, int soMoi)
        {
            if (oChu.TryGetValue(loai, out var chu)) chu.text = soMoi.ToString();
        }

        /// <summary>Dung toan bo thanh bang code, khong can keo tha trong Inspector.</summary>
        public void Dung()
        {
            foreach (Transform con in transform) Destroy(con.gameObject);
            oChu.Clear();

            var hangNgang = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (hangNgang == null) hangNgang = gameObject.AddComponent<HorizontalLayoutGroup>();
            hangNgang.spacing = demGiuaCum;
            hangNgang.childAlignment = TextAnchor.UpperLeft;
            hangNgang.childForceExpandWidth = false;
            hangNgang.childForceExpandHeight = false;
            hangNgang.padding = new RectOffset(10, 10, 6, 6);

            foreach (var cum in cacCum)
            {
                if (cum.danhSach.Count == 0) continue;

                var goCum = new GameObject("Cum_" + cum.tenCum, typeof(RectTransform));
                goCum.transform.SetParent(transform, false);

                var anhNen = goCum.AddComponent<Image>();
                anhNen.sprite = khungCum;
                anhNen.type = Image.Type.Sliced;
                anhNen.pixelsPerUnitMultiplier = 3.125f;  // vien 6px -> hien thi 12px

                var boCuc = goCum.AddComponent<HorizontalLayoutGroup>();
                boCuc.spacing = 6;
                boCuc.padding = new RectOffset(14, 14, 8, 8);
                boCuc.childAlignment = TextAnchor.MiddleLeft;
                boCuc.childForceExpandWidth = false;
                boCuc.childForceExpandHeight = false;

                var coGian = goCum.AddComponent<ContentSizeFitter>();
                coGian.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                coGian.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                foreach (var o in cum.danhSach)
                {
                    // Icon
                    var goIcon = new GameObject("Icon_" + o.loai, typeof(RectTransform));
                    goIcon.transform.SetParent(goCum.transform, false);
                    var anhIcon = goIcon.AddComponent<Image>();
                    anhIcon.sprite = o.icon;
                    anhIcon.preserveAspect = true;
                    var leIcon = goIcon.AddComponent<LayoutElement>();
                    leIcon.preferredWidth = coIcon;
                    leIcon.preferredHeight = coIcon;

                    // So luong
                    var goSo = new GameObject("So_" + o.loai, typeof(RectTransform));
                    goSo.transform.SetParent(goCum.transform, false);
                    var chu = goSo.AddComponent<Text>();
                    chu.font = ChuUI.Dam;
                    chu.fontSize = 28;
                    chu.fontStyle = FontStyle.Normal;
                    chu.alignment = TextAnchor.MiddleRight;
                    chu.color = new Color32(0xF4, 0xE6, 0xC0, 255);
                    chu.text = "0";
                    var leSo = goSo.AddComponent<LayoutElement>();
                    leSo.preferredWidth = rongSo;
                    leSo.preferredHeight = coIcon;

                    ChuUI.ApDung(chu, true);

                    if (!oChu.ContainsKey(o.loai)) oChu.Add(o.loai, chu);
                }
            }

            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0f, -caoThanh);
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}
