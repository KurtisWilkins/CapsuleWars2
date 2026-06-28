using System;
using System.Collections.Generic;
using CapsuleWars.Data.Classes;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Maps each unit class to the ability "kit" it spawns with (BTS-F part 2). Lives in the Abilities assembly —
    /// the LOWEST layer that can name BOTH <see cref="UnitClass_SO"/> (Data) and <see cref="Ability_SO"/> (Abilities);
    /// the map cannot be a field on <c>UnitClass_SO</c> because Data must not reference Abilities, nor live in
    /// <c>UnitFactory</c> (Persistence, which also can't see Abilities). <see cref="ClassAbilityLoader"/> reads this
    /// at spawn and installs the kit on the unit's <see cref="AbilityController"/>. Authored idempotently by
    /// the editor ClassAbilitySetupTool. Lookup is by class REFERENCE, falling back to the stable
    /// <see cref="UnitClass_SO.ClassId"/> so it survives asset re-import.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassAbilitySet", menuName = "CapsuleWars/Abilities/Class Ability Set", order = 10)]
    public class ClassAbilitySet_SO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public UnitClass_SO unitClass;
            public Ability_SO[] abilities;
        }

        [Tooltip("One entry per class → the abilities that class spawns with.")]
        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>The ability kit for a class (matched by reference, then by stable ClassId). Null if unmapped.</summary>
        public Ability_SO[] AbilitiesFor(UnitClass_SO unitClass)
        {
            if (unitClass == null) return null;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || e.unitClass == null) continue;
                if (e.unitClass == unitClass) return e.abilities;
            }
            return AbilitiesFor(unitClass.ClassId);
        }

        /// <summary>The ability kit for a class id. Null/empty id or unmapped → null.</summary>
        public Ability_SO[] AbilitiesFor(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return null;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e != null && e.unitClass != null && e.unitClass.ClassId == classId) return e.abilities;
            }
            return null;
        }
    }
}
