using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Translates gameplay state into Animator parameters. Code never sets
    /// Animator parameters directly — it always goes through this controller
    /// so parameter names are owned in one place.
    /// Parameter contract is documented in Docs/03_AnimationController.md.
    ///
    /// Missing parameters are logged once at Awake (so setup mistakes are
    /// loud and visible) and then skipped silently at runtime (so we don't
    /// spam the console 60 times per second).
    /// </summary>
    public class UnitAnimationController : MonoBehaviour
    {
        // Parameter hashes — match Docs/03_AnimationController.md.
        private static readonly int WeaponTypeParam = Animator.StringToHash("WeaponType");
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int AttackTriggerParam = Animator.StringToHash("AttackTrigger");
        private static readonly int AttackIndexParam = Animator.StringToHash("AttackIndex");
        private static readonly int DeathTriggerParam = Animator.StringToHash("DeathTrigger");
        private static readonly int ReviveTriggerParam = Animator.StringToHash("ReviveTrigger");
        private static readonly int HitTriggerParam = Animator.StringToHash("HitTrigger");
        private static readonly int BlockingParam = Animator.StringToHash("Blocking");

        private static readonly (int hash, string name)[] ExpectedParams =
        {
            (WeaponTypeParam, "WeaponType"),
            (SpeedParam, "Speed"),
            (AttackTriggerParam, "AttackTrigger"),
            (AttackIndexParam, "AttackIndex"),
            (DeathTriggerParam, "DeathTrigger"),
            (ReviveTriggerParam, "ReviveTrigger"),
            (HitTriggerParam, "HitTrigger"),
            (BlockingParam, "Blocking"),
        };

        [Tooltip("Animator on the rig. Auto-found in children if left empty.")]
        [SerializeField] private Animator animator;

        public Animator Animator => animator;

        private HashSet<int> existingParams;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            CacheExistingParams();
        }

        private void CacheExistingParams()
        {
            existingParams = new HashSet<int>();
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var p in animator.parameters) existingParams.Add(p.nameHash);

            // Log missing once so setup mistakes are visible.
            List<string> missing = null;
            foreach (var (hash, name) in ExpectedParams)
            {
                if (!existingParams.Contains(hash))
                {
                    missing ??= new List<string>();
                    missing.Add(name);
                }
            }
            if (missing != null)
            {
                Debug.LogWarning(
                    $"[UnitAnimationController] Animator on {animator.gameObject.name} is missing parameters: {string.Join(", ", missing)}. " +
                    "See Docs/03_AnimationController.md for the full parameter contract.",
                    this);
            }
        }

        private bool Has(int paramHash) => existingParams != null && existingParams.Contains(paramHash);

        public void SetWeaponType(int weaponTypeId)
        {
            if (animator != null && Has(WeaponTypeParam)) animator.SetInteger(WeaponTypeParam, weaponTypeId);
        }

        /// <summary>0 = idle, 0.5 = run, 1 = stunned (per Docs/03).</summary>
        public void SetSpeed(float value)
        {
            if (animator != null && Has(SpeedParam)) animator.SetFloat(SpeedParam, value);
        }

        public void PlayAttack(int attackIndex)
        {
            if (animator == null) return;
            if (Has(AttackIndexParam)) animator.SetInteger(AttackIndexParam, attackIndex);
            if (Has(AttackTriggerParam)) animator.SetTrigger(AttackTriggerParam);
        }

        public void PlayDeath()
        {
            if (animator != null && Has(DeathTriggerParam)) animator.SetTrigger(DeathTriggerParam);
        }

        public void PlayRevive()
        {
            if (animator != null && Has(ReviveTriggerParam)) animator.SetTrigger(ReviveTriggerParam);
        }

        public void PlayHit()
        {
            if (animator != null && Has(HitTriggerParam)) animator.SetTrigger(HitTriggerParam);
        }

        public void SetBlocking(bool blocking)
        {
            if (animator != null && Has(BlockingParam)) animator.SetBool(BlockingParam, blocking);
        }
    }
}

