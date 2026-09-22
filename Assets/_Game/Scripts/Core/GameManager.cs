using System;
using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Bo dieu khien trung tam: quan ly state machine 2 pha (Kingdom / Battle),
    /// dem ngay, va mau cua ham nguc. Moi he thong khac lang nghe event tu day.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Chu ky ngay / dem")]
        [Tooltip("So giay cua pha xay dung truoc khi bi tan cong")]
        [SerializeField] private float kingdomPhaseDuration = 60f;

        [Tooltip("So giay canh bao truoc khi chuyen sang Battle View")]
        [SerializeField] private float warningLeadTime = 3f;

        [Header("Ham nguc")]
        [SerializeField] private int maxDungeonHp = 100;

        [Header("Toc do game")]
        [Range(0.5f, 4f)]
        [SerializeField] private float speedMultiplier = 1f;

        public GamePhase Phase { get; private set; } = GamePhase.Kingdom;
        public int DayNumber { get; private set; } = 1;
        public float PhaseTimeRemaining { get; private set; }
        public float KingdomPhaseDuration => kingdomPhaseDuration;
        public int DungeonHp { get; private set; }
        public int MaxDungeonHp => maxDungeonHp;
        public bool IsPaused { get; private set; }

        /// <summary>Delta time da nhan toc do game va bo qua khi pause.</summary>
        public float ScaledDelta => IsPaused ? 0f : Time.deltaTime * speedMultiplier;

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnDayChanged;
        public event Action<int, int> OnDungeonHpChanged;   // current, max
        public event Action OnBattleIncoming;               // canh bao truoc khi danh

        private bool warningFired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DungeonHp = maxDungeonHp;
        }

        private void Start()
        {
            EnterKingdomPhase(resetDay: false);
            OnDungeonHpChanged?.Invoke(DungeonHp, maxDungeonHp);
            OnDayChanged?.Invoke(DayNumber);
        }

        private void Update()
        {
            if (IsPaused || Phase != GamePhase.Kingdom) return;

            PhaseTimeRemaining -= ScaledDelta;

            if (!warningFired && PhaseTimeRemaining <= warningLeadTime)
            {
                warningFired = true;
                OnBattleIncoming?.Invoke();
            }

            if (PhaseTimeRemaining <= 0f)
                EnterBattlePhase();
        }

        // ---------- Dieu khien pha ----------

        public void EnterKingdomPhase(bool resetDay = true)
        {
            if (resetDay)
            {
                DayNumber++;
                OnDayChanged?.Invoke(DayNumber);
            }

            PhaseTimeRemaining = kingdomPhaseDuration;
            warningFired = false;
            SetPhase(GamePhase.Kingdom);
        }

        public void EnterBattlePhase()
        {
            if (Phase == GamePhase.Battle) return;
            SetPhase(GamePhase.Battle);
        }

        /// <summary>BattleManager goi khi het quan xam nhap.</summary>
        public void FinishBattle(bool survived)
        {
            if (!survived)
            {
                SetPhase(GamePhase.GameOver);
                return;
            }
            EnterKingdomPhase(resetDay: true);
        }

        /// <summary>Bo qua thoi gian con lai cua pha xay dung, danh som.</summary>
        public void SkipToBattle()
        {
            if (Phase == GamePhase.Kingdom)
                PhaseTimeRemaining = 0f;
        }

        private void SetPhase(GamePhase next)
        {
            Phase = next;
            OnPhaseChanged?.Invoke(next);
            Debug.Log($"[GameManager] Chuyen sang pha {next} (ngay {DayNumber})");
        }

        // ---------- Mau ham nguc ----------

        public void DamageDungeon(int amount)
        {
            if (amount <= 0 || Phase == GamePhase.GameOver) return;

            DungeonHp = Mathf.Max(0, DungeonHp - amount);
            OnDungeonHpChanged?.Invoke(DungeonHp, maxDungeonHp);

            if (DungeonHp <= 0)
                FinishBattle(survived: false);
        }

        public void HealDungeon(int amount)
        {
            DungeonHp = Mathf.Min(maxDungeonHp, DungeonHp + Mathf.Max(0, amount));
            OnDungeonHpChanged?.Invoke(DungeonHp, maxDungeonHp);
        }

        // ---------- Tien ich ----------

        public void SetPaused(bool paused) => IsPaused = paused;
        public void TogglePause() => IsPaused = !IsPaused;
        public void SetSpeed(float multiplier) => speedMultiplier = Mathf.Clamp(multiplier, 0.5f, 4f);
        public float GetSpeed() => speedMultiplier;
    }
}
