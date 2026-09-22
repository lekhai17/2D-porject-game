using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KingdomRuins
{
    /// <summary>
    /// Lam toi Global Light 2D khi chuyen sang pha chien dau (dem),
    /// de den duong thuc su noi bat. Gan vao GameObject co Global Light 2D.
    /// </summary>
    public class DayNightLighting : MonoBehaviour
    {
        [SerializeField] private Light2D globalLight;
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensity = 0.32f;
        [SerializeField] private Color dayColor = Color.white;
        [SerializeField] private Color nightColor = new Color(0.45f, 0.52f, 0.78f);
        [SerializeField] private float fadeSpeed = 0.7f;

        private float target;
        private Color targetColor;

        private void Awake()
        {
            if (globalLight == null) globalLight = GetComponent<Light2D>();
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
            if (globalLight != null)
            {
                globalLight.intensity = target;
                globalLight.color = targetColor;
            }
        }

        private void HandlePhase(GamePhase phase)
        {
            bool night = phase == GamePhase.Battle;
            target = night ? nightIntensity : dayIntensity;
            targetColor = night ? nightColor : dayColor;
        }

        private void Update()
        {
            if (globalLight == null) return;
            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, target, fadeSpeed * Time.deltaTime);
            globalLight.color = Color.Lerp(globalLight.color, targetColor, fadeSpeed * Time.deltaTime);
        }
    }
}
