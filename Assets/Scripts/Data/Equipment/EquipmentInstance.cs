using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// One actual item a player owns: a reference to its <see cref="Equipment_SO"/> definition
    /// (identity/visuals) plus the stat <see cref="modifiers"/> assigned/rolled at RUNTIME, an
    /// optional generated <see cref="displayName"/> ("Helmet of Health"), and roll metadata
    /// (<see cref="tier"/>, <see cref="seed"/>). This is the unit of stats + what gets saved —
    /// the same definition can back many instances with different stats.
    ///
    /// Lives in Data so both Units (runtime equip on UnitStatusController) and Persistence
    /// (UnitFactory save/load) can reference it without violating assembly layering.
    /// </summary>
    [Serializable]
    public class EquipmentInstance
    {
        public Equipment_SO definition;
        public List<StatBuff> modifiers = new List<StatBuff>();
        public string displayName;
        public int tier;
        public int seed;

        public EquipmentInstance() { }

        public EquipmentInstance(Equipment_SO definition, IEnumerable<StatBuff> modifiers = null,
                                 string displayName = null, int tier = 0, int seed = 0)
        {
            this.definition = definition;
            if (modifiers != null) this.modifiers = new List<StatBuff>(modifiers);
            this.displayName = displayName;
            this.tier = tier;
            this.seed = seed;
        }

        public IReadOnlyList<StatBuff> Modifiers => modifiers;

        public EquipmentSlot Slot => definition != null ? definition.Slot : default;

        /// <summary>Generated name if present, else the definition's name key / id.</summary>
        public string Name =>
            !string.IsNullOrEmpty(displayName) ? displayName
            : definition != null ? (string.IsNullOrEmpty(definition.NameTermKey) ? definition.EquipmentId : definition.NameTermKey)
            : "";

        /// <summary>
        /// Build an instance carrying the definition's LEGACY baked stats (migration/compat) —
        /// so pre-instance items and old saves keep their stats. New items roll modifiers instead.
        /// </summary>
        public static EquipmentInstance FromDefinitionDefault(Equipment_SO def) =>
            def == null ? null : new EquipmentInstance(def, def.BuildDefaultModifiers());
    }
}
