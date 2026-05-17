using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Lives on the same GameObject as the Animator (typically the rig root).
    /// Animation Events on attack clips invoke <see cref="OnAttackHitFrame"/>
    /// at the moment of impact, which forwards to the unit's
    /// UnitAttackController to resolve damage.
    ///
    /// <see cref="Hit"/> is an alias that matches ExplosiveLLC's stock
    /// animation event naming convention — clips imported from the
    /// ExplosiveLLC pack fire "Hit" at the impact frame. Both names route
    /// to the same handler so we can reuse those clips without re-authoring
    /// events.
    /// </summary>
    public class AttackHitEventReceiver : MonoBehaviour
    {
        private UnitAttackController attack;

        private void Awake()
        {
            attack = GetComponentInParent<UnitAttackController>();
        }

        /// <summary>Called by Animation Events on CapsuleWars-authored attack clips.</summary>
        public void OnAttackHitFrame()
        {
            if (attack != null) attack.OnHitFrame();
        }

        /// <summary>Alias for ExplosiveLLC stock clips (events named "Hit").</summary>
        public void Hit()
        {
            if (attack != null) attack.OnHitFrame();
        }

        /// <summary>No-op receiver for ExplosiveLLC footstep events. Silences "no receiver" warnings.</summary>
        public void FootR() { }

        /// <summary>No-op receiver for ExplosiveLLC footstep events.</summary>
        public void FootL() { }

        /// <summary>No-op receiver for ExplosiveLLC weapon enable/disable events.</summary>
        public void WeaponSwitch() { }

        /// <summary>No-op receiver for ExplosiveLLC shooting events. Routes to OnHitFrame so projectile timing works.</summary>
        public void Shoot()
        {
            if (attack != null) attack.OnHitFrame();
        }
    }
}

