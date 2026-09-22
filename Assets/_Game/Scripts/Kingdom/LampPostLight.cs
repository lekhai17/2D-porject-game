using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KingdomRuins
{
    /// <summary>
    /// Den duong: tat vao pha xay dung (ban ngay), sang len vao pha chien dau (ban dem).
    /// Dieu khien ca Light2D that lan sprite quang sang.
    /// </summary>
    public class LampPostLight : MonoBehaviour
    {
        [Header("Do sang theo pha")]
        [SerializeField] private float dayIntensity = 0f;
        [SerializeField] private float nightIntensity = 1.15f;
        [SerializeField] private float fadeSpeed = 1.8f;

        [Header("Nhap nhay")]
        [SerializeField] private float flickerAmount = 0.07f;
        [SerializeField] private float flickerSpeed = 5.5f;

        [Header("Tham chieu")]
        [SerializeField] private Light2D lamp2D;
        [SerializeField] private SpriteRenderer glow;

        private float target;
        private float current;
        private float seed;
        private Color glowBaseColor = Color.white;

        private void Awake()
        {
            if (lamp2D == null) lamp2D = GetComponentInChildren<Light2D>();
            seed = Random.value * 100f;
            if (glow != null) glowBaseColor = glow.color;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged += HandlePhase;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged -= HandlePhase;
        }

        private void Start()
        {
            HandlePhase(GameManager.Instance != null ? GameManager.Instance.Phase : GamePhase.Kingdom);
            current = target;
            Apply(current);
        }

        private void HandlePhase(GamePhase phase)
        {
            // Chi sang vao pha chien dau (dem)
            target = phase == GamePhase.Battle ? nightIntensity : dayIntensity;
        }

        private void Update()
        {
            if (!Mathf.Approximately(current, target))
                current = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);

            float flicker = 1f + Mathf.Sin((Time.time + seed) * flickerSpeed) * flickerAmount;
            Apply(current * flicker);
        }

        private void Apply(float value)
        {
            if (lamp2D != null) lamp2D.intensity = Mathf.Max(0f, value);

            if (glow != null)
            {
                var c = glowBaseColor;
                c.a = Mathf.Clamp01(value / Mathf.Max(0.01f, nightIntensity));
                glow.color = c;
            }
        }
    }
}
