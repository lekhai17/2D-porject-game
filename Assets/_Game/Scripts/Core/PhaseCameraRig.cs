using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Doi camera giua Kingdom View (top-down cuon tu do, kieu Super Fantasy Kingdom)
    /// va Battle View (co dinh nhin doc lane, kieu The King is Watching).
    /// Hai khu vuc nam trong cung mot scene o hai vi tri khac nhau nen khong phai load scene.
    /// </summary>
    public class PhaseCameraRig : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera kingdomCamera;
        [SerializeField] private Camera battleCamera;

        [Header("Chuyen canh")]
        [SerializeField] private float fadeDuration = 0.35f;

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

        private void Start()
        {
            // Dong bo trang thai ban dau
            if (GameManager.Instance != null)
                HandlePhaseChanged(GameManager.Instance.Phase);
            else
                Activate(kingdom: true);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Kingdom:
                    Activate(kingdom: true);
                    break;
                case GamePhase.Battle:
                    Activate(kingdom: false);
                    break;
                case GamePhase.GameOver:
                    // giu nguyen camera dang dung, UI GameOver se phu len
                    break;
            }
        }

        private void Activate(bool kingdom)
        {
            if (kingdomCamera != null) kingdomCamera.enabled = kingdom;
            if (battleCamera != null) battleCamera.enabled = !kingdom;
        }

        public float FadeDuration => fadeDuration;
        public Camera KingdomCamera => kingdomCamera;
        public Camera BattleCamera => battleCamera;
    }
}
