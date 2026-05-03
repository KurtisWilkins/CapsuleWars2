using System;
using CapsuleWars.Core;
using CapsuleWars.Data.Weapons;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Decides when to swing and resolves damage on the animation hit-frame.
    /// Movement asks <see cref="TryAttack"/> when in range; if the cooldown
    /// has elapsed, plays the next attack via the AnimationController.
    /// Damage resolves when <see cref="OnHitFrame"/> fires from the attack
    /// clip's animation event (forwarded by AttackHitEventReceiver).
    /// </summary>
    public class UnitAttackController : MonoBehaviour
    {
        [Tooltip("Weapon class for this unit. Drives attack range, cooldown, and Animator sub-state-machine selection.")]
        [SerializeField] private WeaponClass_SO weaponClass;

        public WeaponClass_SO WeaponClass => weaponClass;
        public float AttackRange => weaponClass != null ? weaponClass.AttackRange : 2f;

        private UnitRoot root;
        private float lastAttackTime = -999f;
        private int nextAttackIndex = 1;
        private IUnitRef pendingTarget;

        private void Awake()
        {
            root = GetComponentInParent<UnitRoot>();
        }

        private void Start()
        {
            if (root?.Animation != null && weaponClass != null)
                root.Animation.SetWeaponType(weaponClass.WeaponTypeId);
        }

        /// <summary>Try to start an attack against <paramref name="target"/>. Returns true if a swing was started.</summary>
        public bool TryAttack(IUnitRef target)
        {
            if (root == null || root.Health == null || root.Health.IsDowned) return false;
            if (weaponClass == null || target == null || target.IsDowned) return false;
            if (Time.time - lastAttackTime < weaponClass.AttackCooldown) return false;

            lastAttackTime = Time.time;
            pendingTarget = target;

            int idx = nextAttackIndex;
            int count = Math.Max(1, weaponClass.AttackCount);
            nextAttackIndex = (nextAttackIndex % count) + 1;

            root.Animation?.PlayAttack(idx);
            return true;
        }

        /// <summary>
        /// Called by AttackHitEventReceiver from the animation event on an attack clip.
        /// Resolves damage against the target locked in at swing-start.
        /// </summary>
        public void OnHitFrame()
        {
            var target = pendingTarget;
            pendingTarget = null;
            if (target == null || target.IsDowned) return;
            if (root == null || root.Status == null) return;

            var targetRoot = target.GameObject != null ? target.GameObject.GetComponentInParent<UnitRoot>() : null;
            if (targetRoot == null || targetRoot.Status == null || targetRoot.Health == null) return;

            int raw = root.Status.Atk - targetRoot.Status.Def;
            int damage = Math.Max(1, raw);
            targetRoot.Health.TakeDamage(damage, root);
        }
    }
}
