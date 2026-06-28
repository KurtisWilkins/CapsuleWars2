using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

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

        // --- Runtime tint (tint milestone). Per-unit, RENDER-TIME only — never baked into the definition (which
        // stays the neutral grayscale template). primaryTint = Color.clear means "untinted" (grayscale passes through);
        // accentTints override the primary on specific part slots. Cross-run persistence is out of scope this milestone.
        public Color primaryTint = Color.clear;
        public List<PartTint> accentTints = new List<PartTint>();

        /// <summary>The effective tint for a part slot — its accent override if set, else the primary tint.</summary>
        public Color TintFor(PartSlot slot)
        {
            if (accentTints != null)
                for (int i = 0; i < accentTints.Count; i++)
                    if (accentTints[i].slot == slot) return accentTints[i].color;
            return primaryTint;
        }

        /// <summary>Set (or overwrite) a per-slot accent override.</summary>
        public void SetAccent(PartSlot slot, Color color)
        {
            accentTints ??= new List<PartTint>();
            for (int i = 0; i < accentTints.Count; i++)
                if (accentTints[i].slot == slot) { accentTints[i] = new PartTint(slot, color); return; }
            accentTints.Add(new PartTint(slot, color));
        }

        /// <summary>Remove a per-slot accent override (the slot falls back to the primary tint). True if removed.</summary>
        public bool ClearAccent(PartSlot slot)
        {
            if (accentTints == null) return false;
            for (int i = 0; i < accentTints.Count; i++)
                if (accentTints[i].slot == slot) { accentTints.RemoveAt(i); return true; }
            return false;
        }

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
