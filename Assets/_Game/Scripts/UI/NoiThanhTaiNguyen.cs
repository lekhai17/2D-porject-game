using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Noi cac o so co san trong thanh HUD vao ResourceManager.
    /// Gan vao chinh doi tuong TopHUD, khai bao tung cap o chu - loai tai nguyen.
    /// </summary>
    public class NoiThanhTaiNguyen : MonoBehaviour
    {
        [System.Serializable]
        public class CapNoi
        {
            [Tooltip("O chu hien so, vi du GEMSV")]
            public Text oSo;
            public ResourceType loai;
            [Tooltip("Chu hien khi chua co du lieu")]
            public string dangKhiTrong = "0";
        }

        [SerializeField] private List<CapNoi> danhSach = new List<CapNoi>();

        private void Start()
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("[NoiThanhTaiNguyen] Khong tim thay ResourceManager");
                return;
            }

            ResourceManager.Instance.OnResourceChanged += CapNhat;
            foreach (var c in danhSach)
                if (c.oSo != null)
                    c.oSo.text = ResourceManager.Instance.Get(c.loai).ToString();
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= CapNhat;
        }

        private void CapNhat(ResourceType loai, int soMoi)
        {
            foreach (var c in danhSach)
                if (c.loai == loai && c.oSo != null)
                    c.oSo.text = soMoi.ToString();
        }
    }
}
