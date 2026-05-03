using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Lives on the same GameObject as the Animator (typically the rig root).
    /// Animation Events on attack clips invoke <see cref="OnAttackHitFrame"/>
    /// at the moment of impact, which forwards to the unit's
    /// UnitAttackController to resolve damage.
    /// </summary>
    public class AttackHitEventReceiver : MonoBehaviour
    {
        private UnitAttackController attack;

        private void Awake()
        {
            attack = GetComponentInParent<UnitAttackController>();
        }

        /// <summary>Called by Animation Events on attack clips.</summary>
        public void OnAttackHitFrame()
        {
            if (attack != null) attack.OnHitFrame();
        }
    }
}
