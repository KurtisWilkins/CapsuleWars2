using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Translates gameplay state into Animator parameters. Code never sets
    /// Animator parameters directly — it always goes through this controller
    /// so parameter names are owned in one place.
    /// Parameter contract is documented in Docs/03_AnimationController.md.
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

        [Tooltip("Animator on the rig. Auto-found in children if left empty.")]
        [SerializeField] private Animator animator;

        public Animator Animator => animator;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public void SetWeaponType(int weaponTypeId)
        {
            if (animator != null) animator.SetInteger(WeaponTypeParam, weaponTypeId);
        }

        /// <summary>0 = idle, 0.5 = run, 1 = stunned (per Docs/03).</summary>
        public void SetSpeed(float value)
        {
            if (animator != null) animator.SetFloat(SpeedParam, value);
        }

        public void PlayAttack(int attackIndex)
        {
            if (animator == null) return;
            animator.SetInteger(AttackIndexParam, attackIndex);
            animator.SetTrigger(AttackTriggerParam);
        }

        public void PlayDeath()
        {
            if (animator != null) animator.SetTrigger(DeathTriggerParam);
        }

        public void PlayRevive()
        {
            if (animator != null) animator.SetTrigger(ReviveTriggerParam);
        }

        public void PlayHit()
        {
            if (animator != null) animator.SetTrigger(HitTriggerParam);
        }

        public void SetBlocking(bool blocking)
        {
            if (animator != null) animator.SetBool(BlockingParam, blocking);
        }
    }
}
