using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// A loot table (BTS-G, Docs/07): how many items drop, which definitions can drop (weighted), and at which
    /// tier (weighted). The stat roll itself is delegated to <see cref="EquipmentRoller"/> via the referenced
    /// <see cref="EquipmentRollConfig"/>. Pure DATA — it carries NO <c>NodeType</c> (that lives in the Run
    /// assembly): the Run-layer reward hook holds one table per node type and selects the right one, so this
    /// type stays Core-only and layering-clean. Consumed by <see cref="LootRoller"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable", menuName = "CapsuleWars/Equipment/Loot Table", order = 94)]
    public class LootTable_SO : ScriptableObject
    {
        [Serializable]
        public struct WeightedDrop
        {
            public Equipment_SO item;
            [Min(0f)] public float weight;
        }

        [Serializable]
        public struct WeightedTier
        {
            [Min(0)] public int tier;
            [Min(0f)] public float weight;
        }

        [Tooltip("Minimum items dropped (e.g. 0 so a combat node can drop nothing).")]
        [SerializeField, Min(0)] private int minDrops = 0;

        [Tooltip("Maximum items dropped.")]
        [SerializeField, Min(0)] private int maxDrops = 1;

        [Tooltip("Droppable definitions + selection weights. Empty (or all-null) = no drops.")]
        [SerializeField] private List<WeightedDrop> items = new();

        [Tooltip("Tier (index into the roll config's tiers) selection weights. Empty = tier 0.")]
        [SerializeField] private List<WeightedTier> tiers = new();

        [Tooltip("Stat-roll config handed to EquipmentRoller for each dropped item.")]
        [SerializeField] private EquipmentRollConfig rollConfig;

        public int MinDrops => minDrops;
        public int MaxDrops => maxDrops;
        public IReadOnlyList<WeightedDrop> Items => items;
        public IReadOnlyList<WeightedTier> Tiers => tiers;
        public EquipmentRollConfig RollConfig => rollConfig;
    }
}
