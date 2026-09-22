using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>
    /// Hien thi so ngay, trang thai ngay/dem va thoi gian da troi qua.
    /// Doc truc tiep tu GameManager nen khong can cau hinh them.
    /// </summary>
    public class HienThiNgayDem : MonoBehaviour
    {
        [Header("O chu")]
        [SerializeField] private Text oSoNgay;
        [SerializeField] private Text oNhan;        // chu DAY / NIGHT
        [SerializeField] private Text oBieuTuong;   // mat troi / mat trang
        [SerializeField] private Text oThoiGian;    // dong ho mm:ss

        [Header("Thanh tien do cua pha")]
        [SerializeField] private Image thanhTienDo;

        [Header("Cach tinh gio")]
        [Tooltip("Bat: dem thoi gian da troi cua pha hien tai. Tat: dem nguoc thoi gian con lai.")]
        [SerializeField] private bool demXuoi = true;
        [Tooltip("Bat: cong don ca van choi thay vi reset moi pha")]
        [SerializeField] private bool congDonCaVan = false;

        [Header("Chu hien thi")]
        [SerializeField] private string nhanBanNgay = "DAY";
        [SerializeField] private string nhanBanDem = "NIGHT";
        [SerializeField] private string bieuTuongNgay = "\u2600";
        [SerializeField] private string bieuTuongDem = "\u263D";

        [Header("Mau")]
        [SerializeField] private Color mauNgay = new Color32(0xF2, 0xC5, 0x4E, 255);
        [SerializeField] private Color mauDem = new Color32(0x8F, 0xA8, 0xE0, 255);

        private float dongHo;       // giay da troi trong pha hien tai
        private float tongVan;      // giay da troi ca van

        private void OnEnable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnDayChanged += DoiNgay;
            GameManager.Instance.OnPhaseChanged += DoiPha;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnDayChanged -= DoiNgay;
            GameManager.Instance.OnPhaseChanged -= DoiPha;
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[HienThiNgayDem] Khong tim thay GameManager");
                return;
            }
            DoiNgay(GameManager.Instance.DayNumber);
            DoiPha(GameManager.Instance.Phase);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            float dt = gm.ScaledDelta;
            dongHo += dt;
            tongVan += dt;

            if (thanhTienDo != null)
            {
                if (gm.Phase == GamePhase.Kingdom && gm.KingdomPhaseDuration > 0f)
                    thanhTienDo.fillAmount = 1f - Mathf.Clamp01(gm.PhaseTimeRemaining / gm.KingdomPhaseDuration);
                else if (gm.Phase == GamePhase.Battle)
                    thanhTienDo.fillAmount = 1f;
            }

            if (oThoiGian == null) return;

            float giay;
            if (congDonCaVan) giay = tongVan;
            else if (demXuoi) giay = dongHo;
            else giay = gm.Phase == GamePhase.Kingdom ? Mathf.Max(0f, gm.PhaseTimeRemaining) : dongHo;

            oThoiGian.text = DangGio(giay);
        }

        /// <summary>Doi giay thanh dang mm:ss, qua mot gio thi them phan gio.</summary>
        private string DangGio(float giay)
        {
            int tong = Mathf.FloorToInt(giay);
            int gio = tong / 3600;
            int phut = (tong % 3600) / 60;
            int gy = tong % 60;
            return gio > 0
                ? string.Format("{0}:{1:00}:{2:00}", gio, phut, gy)
                : string.Format("{0:00}:{1:00}", phut, gy);
        }

        private void DoiNgay(int ngay)
        {
            if (oSoNgay != null) oSoNgay.text = ngay.ToString("00");
        }

        private void DoiPha(GamePhase pha)
        {
            dongHo = 0f;    // moi pha dem lai tu dau

            bool dem = pha == GamePhase.Battle;
            if (oNhan != null) oNhan.text = dem ? nhanBanDem : nhanBanNgay;
            if (oBieuTuong != null)
            {
                oBieuTuong.text = dem ? bieuTuongDem : bieuTuongNgay;
                oBieuTuong.color = dem ? mauDem : mauNgay;
            }
            if (thanhTienDo != null) thanhTienDo.color = dem ? mauDem : mauNgay;
            if (oThoiGian != null) oThoiGian.color = dem ? mauDem : mauNgay;
        }
    }
}
