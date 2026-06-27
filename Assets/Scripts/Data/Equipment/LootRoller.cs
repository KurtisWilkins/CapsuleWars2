using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Deterministically rolls the dropped <see cref="EquipmentInstance"/>s for a <see cref="LootTable_SO"/>
    /// (BTS-G). Same seed → identical drops. Pure + UI-free (Data layer): it PRODUCES instances but never stores
    /// them — the Run-layer reward hook adds the result to the run inventory and saves. The per-item stat roll is
    /// delegated to <see cref="EquipmentRoller.Roll"/>; everything (count, item pick, tier pick, per-item seed) is
    /// drawn from one <see cref="System.Random"/> seeded by <paramref name="seed"/>, so it's fully reproducible.
    /// </summary>
    public static class LootRoller
    {
        public static List<EquipmentInstance> Roll(LootTable_SO table, int seed)
        {
            var result = new List<EquipmentInstance>();
            if (table == null) return result;

            var rng = new System.Random(seed);
            int min = Mathf.Min(table.MinDrops, table.MaxDrops);
            int max = Mathf.Max(table.MinDrops, table.MaxDrops);
            int count = min + (max > min ? rng.Next(max - min + 1) : 0);

            for (int i = 0; i < count; i++)
            {
                var def = PickItem(table, rng);
                if (def == null) continue;
                int tier = PickTier(table, rng);
                int rollSeed = rng.Next();
                result.Add(EquipmentRoller.Roll(def, table.RollConfig, tier, rollSeed));
            }
            return result;
        }

        private static Equipment_SO PickItem(LootTable_SO table, System.Random rng)
        {
            var items = table.Items;
            if (items == null || items.Count == 0) return null;

            float total = 0f;
            for (int i = 0; i < items.Count; i++)
                if (items[i].item != null) total += Mathf.Max(0f, items[i].weight);

            if (total <= 0f)
            {
                // All weights zero → uniform over the non-null items.
                var nonNull = new List<Equipment_SO>();
                for (int i = 0; i < items.Count; i++)
                    if (items[i].item != null) nonNull.Add(items[i].item);
                return nonNull.Count == 0 ? null : nonNull[rng.Next(nonNull.Count)];
            }

            double r = rng.NextDouble() * total;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item == null) continue;
                r -= Mathf.Max(0f, items[i].weight);
                if (r <= 0) return items[i].item;
            }
            for (int i = items.Count - 1; i >= 0; i--)
                if (items[i].item != null) return items[i].item;
            return null;
        }

        private static int PickTier(LootTable_SO table, System.Random rng)
        {
            var tiers = table.Tiers;
            if (tiers == null || tiers.Count == 0) return 0;

            float total = 0f;
            for (int i = 0; i < tiers.Count; i++) total += Mathf.Max(0f, tiers[i].weight);
            if (total <= 0f) return tiers[rng.Next(tiers.Count)].tier;

            double r = rng.NextDouble() * total;
            for (int i = 0; i < tiers.Count; i++)
            {
                r -= Mathf.Max(0f, tiers[i].weight);
                if (r <= 0) return tiers[i].tier;
            }
            return tiers[tiers.Count - 1].tier;
        }
    }
}
