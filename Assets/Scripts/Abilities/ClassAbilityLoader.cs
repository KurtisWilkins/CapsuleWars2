using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// At spawn, installs a unit's CLASS ability kit onto its <see cref="AbilityController"/> (BTS-F part 2).
    /// Sits on the shared base unit prefab, so BOTH player (BattlePartySpawner) and enemy (EnemyEncounterSpawner)
    /// units — which clone that prefab through <c>UnitFactory.Spawn</c> — get the right moves with ZERO spawner
    /// edits. Reads the unit's class from <see cref="UnitStatusController.UnitClass"/> (the same source
    /// SynergyResolver uses) and looks the kit up in a <see cref="ClassAbilitySet_SO"/>.
    ///
    /// Order-independent: <see cref="AbilityController.SetAbilities"/> replaces + rebuilds idempotently, so whether
    /// this Awake runs before or after the controller's own Awake, the unit ends with its class kit. No-op without a
    /// set / controller / class (the controller keeps whatever abilities it was authored with).
    /// </summary>
    [RequireComponent(typeof(AbilityController))]
    public class ClassAbilityLoader : MonoBehaviour
    {
        [Tooltip("Registry mapping each class to its spawn ability kit (ClassAbilitySet asset).")]
        [SerializeField] private ClassAbilitySet_SO abilitySet;

        private void Awake() => Load();

        /// <summary>Resolve this unit's class kit and install it on the AbilityController. Idempotent.</summary>
        public void Load()
        {
            if (abilitySet == null) return;
            var controller = GetComponent<AbilityController>();
            if (controller == null) return;

            var root = GetComponentInParent<UnitRoot>();
            var unitClass = root != null && root.Status != null ? root.Status.UnitClass : null;
            if (unitClass == null) return;

            var kit = abilitySet.AbilitiesFor(unitClass);
            if (kit != null && kit.Length > 0) controller.SetAbilities(kit);
        }
    }
}
