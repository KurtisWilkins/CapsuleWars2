using System;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Tracks current HP and the downed flag. Fires events for damage taken,
    /// health changed, and downed transitions. Consumers (UI, stats, audio,
    /// VFX) subscribe; this controller never calls them directly.
    /// </summary>
    public class UnitHealthController : MonoBehaviour
    {
        public event Action<DamageEvent> OnDamageTaken;
        public event Action<DownedEvent> OnDowned;
        public event Action<int> OnHealthChanged;

        // [SerializeField] keeps HP visible in the inspector during Play mode.
        // Initialization is lazy (see EnsureInitialized) so the controller
        // reports correct HP even when Awake hasn't run — e.g. EditMode tests
        // that AddComponent in edit mode, where Unity does not invoke Awake.
        [SerializeField] private int currentHp;
        private bool initialized;

        public int CurrentHp
        {
            get { EnsureInitialized(); return currentHp; }
            private set { currentHp = value; }
        }

        public int MaxHp
        {
            get { EnsureInitialized(); return status != null ? status.MaxHp : 0; }
        }

        [field: SerializeField] public bool IsDowned { get; private set; }

        private UnitStatusController status;
        private UnitRoot root;

        private void Awake() => EnsureInitialized();

        /// <summary>
        /// Resolve sibling controllers and seed CurrentHp from MaxHp exactly
        /// once. Safe to call from Awake or lazily from any accessor; the
        /// guard makes it idempotent so it never resets HP mid-battle.
        /// </summary>
        private void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;
            if (status == null) status = GetComponentInParent<UnitStatusController>();
            if (root == null) root = GetComponentInParent<UnitRoot>();
            if (status != null) currentHp = status.MaxHp;
        }

        /// <summary>
        /// Apply damage from <paramref name="source"/>. Always deals at least
        /// 1 to avoid stalemates. No-op if already downed.
        /// </summary>
        public void TakeDamage(int amount, IUnitRef source)
        {
            EnsureInitialized();
            if (IsDowned) return;
            int actual = Math.Max(1, amount);
            CurrentHp = Math.Max(0, CurrentHp - actual);

            IUnitRef self = root;
            OnDamageTaken?.Invoke(new DamageEvent(source, self, actual));
            OnHealthChanged?.Invoke(CurrentHp);

            if (CurrentHp <= 0)
            {
                IsDowned = true;
                OnDowned?.Invoke(new DownedEvent(source, self));
            }
        }

        /// <summary>
        /// Restore the unit to a percentage of MaxHp and clear the downed flag.
        /// Used between battles to apply the -50% returning-from-downed rule.
        /// Percent is clamped to [0, 1].
        /// </summary>
        public void RestoreToPercent(float percent)
        {
            EnsureInitialized();
            if (status == null) return;
            float p = Mathf.Clamp01(percent);
            CurrentHp = Mathf.Max(1, Mathf.RoundToInt(status.MaxHp * p));
            IsDowned = false;
            OnHealthChanged?.Invoke(CurrentHp);
        }
    }
}
