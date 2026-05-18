using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Weapons;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Top-level ability definition. Composes one Trigger (when to fire),
    /// one Targeting strategy (who to collect), a chain of Filters
    /// (narrow the candidates), and a chain of Effects (what to do to
    /// the survivors). Authored as ScriptableObject assets under
    /// Assets/Data/Abilities/.
    ///
    /// M4 uses single strategies per slot (not evolution-indexed arrays
    /// as Docs/05 describes). Evolution-tier strategy arrays come back
    /// when evolution mechanics land.
    /// </summary>
    [CreateAssetMenu(fileName = "Ability", menuName = "CapsuleWars/Ability", order = 2)]
    public class Ability_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and the Database lookup.")]
        [SerializeField] private string abilityId;

        [Tooltip("I2 term key for the ability's display name (e.g. Ability.Backstab.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("I2 term key for the ability's description.")]
        [SerializeField] private string descTermKey;

        [Tooltip("UI icon. Sprite is fine; the icon-gen pipeline (post-MVP) produces these.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Maximum effective range for filters that respect it (e.g. InRangeFilter).")]
        [SerializeField, Min(0f)] private float range = 5f;

        [Tooltip("Weapon classes that satisfy this ability. Empty = any weapon (including Unarmed).")]
        [SerializeField] private WeaponClass_SO[] requiredWeaponClasses;

        [Header("Composition")]
        [SerializeField] private AbilityTriggerStrategy trigger;
        [SerializeField] private AbilityTargetingStrategy targeting;
        [SerializeField] private List<AbilityFilterStrategy> filters = new();
        [SerializeField] private List<AbilityEffectStrategy> effects = new();

        public string AbilityId => abilityId;
        public string NameTermKey => nameTermKey;
        public string DescTermKey => descTermKey;
        public Sprite Icon => icon;
        public float Range => range;
        public IReadOnlyList<WeaponClass_SO> RequiredWeaponClasses => requiredWeaponClasses;

        public AbilityTriggerStrategy Trigger => trigger;
        public AbilityTargetingStrategy Targeting => targeting;
        public IReadOnlyList<AbilityFilterStrategy> Filters => filters;
        public IReadOnlyList<AbilityEffectStrategy> Effects => effects;

        /// <summary>
        /// True if the equipped weapon (or null = unarmed) satisfies this
        /// ability's <see cref="RequiredWeaponClasses"/>. Empty requirements
        /// means any weapon is valid.
        /// </summary>
        public bool IsWeaponCompatible(WeaponClass_SO equipped)
        {
            if (requiredWeaponClasses == null || requiredWeaponClasses.Length == 0) return true;
            for (int i = 0; i < requiredWeaponClasses.Length; i++)
            {
                if (requiredWeaponClasses[i] == equipped) return true;
            }
            return false;
        }
    }
}
