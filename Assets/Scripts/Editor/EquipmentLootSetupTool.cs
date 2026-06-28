#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Authors first-pass BTS-G loot data: one <see cref="EquipmentRollConfig"/> (stat pool + 3 tiers) and
    /// per-node-type <see cref="LootTable_SO"/>s (Combat = 0–1 drops mostly low-tier; Elite = 1 guaranteed,
    /// rarity-skewed), referencing the existing droppable Equipment_SO items. Idempotent. Numbers are
    /// first-pass / tunable in the inspector. After running, assign the tables to BattleNodeReturn in the
    /// battle scene — the drop loop then fires on combat/elite wins via LootGrant.
    /// </summary>
    public static class EquipmentLootSetupTool
    {
        private const string Dir = "Assets/Data/Equipment";
        private const string ConfigPath = Dir + "/EquipmentRollConfig.asset";
        private const string CombatPath = Dir + "/LootTable_Combat.asset";
        private const string ElitePath = Dir + "/LootTable_Elite.asset";

        private static readonly string[] DroppableItemPaths =
        {
            Dir + "/Equip_StarterHelm.asset",
            Dir + "/Equip_StarterPlate.asset",
            Dir + "/Equip_StarterSword.asset",
            Dir + "/Equip_StarterShield.asset",
            // BTS-G armor-slot items (EquipmentContentTool) — identity-only until meshed, but droppable for stats.
            Dir + "/Eq_Shoulderpad.asset",
            Dir + "/Eq_Cape.asset",
            Dir + "/Eq_Bracers.asset",
            Dir + "/Eq_Greaves.asset",
        };

        [MenuItem("Tools/Build-To-Spec/Author Loot Tables + Roll Config")]
        public static void Author()
        {
            var config = CreateOrLoad<EquipmentRollConfig>(ConfigPath);
            config.pool = new List<EquipmentRollConfig.RollableStat>
            {
                Stat(StatType.MaxHp, 10, 30, 1.5f, "Health"),
                Stat(StatType.Atk,    3, 10, 1.5f, "Power"),
                Stat(StatType.Def,    2,  8, 1f,   "Warding"),
                Stat(StatType.Speed,  2,  6, 1f,   "Swiftness"),
            };
            config.tiers = new List<EquipmentRollConfig.TierRule>
            {
                new EquipmentRollConfig.TierRule { modifierCount = 1, magnitudeScale = 1f },   // tier 0
                new EquipmentRollConfig.TierRule { modifierCount = 2, magnitudeScale = 1.4f }, // tier 1
                new EquipmentRollConfig.TierRule { modifierCount = 3, magnitudeScale = 1.9f }, // tier 2
            };
            EditorUtility.SetDirty(config);

            var items = LoadItems();

            var combat = CreateOrLoad<LootTable_SO>(CombatPath);
            SetField(combat, "minDrops", 0);
            SetField(combat, "maxDrops", 1);
            SetField(combat, "items", WeightedDrops(items));
            SetField(combat, "tiers", Tiers((0, 3f), (1, 1f)));
            SetField(combat, "rollConfig", config);
            EditorUtility.SetDirty(combat);

            var elite = CreateOrLoad<LootTable_SO>(ElitePath);
            SetField(elite, "minDrops", 1);
            SetField(elite, "maxDrops", 1);
            SetField(elite, "items", WeightedDrops(items));
            SetField(elite, "tiers", Tiers((0, 1f), (1, 3f), (2, 1.5f)));   // rarity-skewed
            SetField(elite, "rollConfig", config);
            EditorUtility.SetDirty(elite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EquipmentLootSetupTool] Authored EquipmentRollConfig + LootTable_Combat (0-1) + LootTable_Elite " +
                      $"(1, rarity-skewed) referencing {items.Count} droppable items. First-pass numbers — assign the " +
                      "tables to BattleNodeReturn in the battle scene to fire drops on win.");
        }

        private static EquipmentRollConfig.RollableStat Stat(StatType s, float min, float max, float w, string suffix) =>
            new EquipmentRollConfig.RollableStat { stat = s, modType = StatBuffModType.Flat, minMagnitude = min, maxMagnitude = max, weight = w, nameSuffix = suffix };

        private static List<Equipment_SO> LoadItems()
        {
            var list = new List<Equipment_SO>();
            foreach (var p in DroppableItemPaths)
            {
                var e = AssetDatabase.LoadAssetAtPath<Equipment_SO>(p);
                if (e != null) list.Add(e);
            }
            return list;
        }

        private static List<LootTable_SO.WeightedDrop> WeightedDrops(List<Equipment_SO> items)
        {
            var list = new List<LootTable_SO.WeightedDrop>();
            foreach (var e in items) list.Add(new LootTable_SO.WeightedDrop { item = e, weight = 1f });
            return list;
        }

        private static List<LootTable_SO.WeightedTier> Tiers(params (int tier, float weight)[] ts)
        {
            var list = new List<LootTable_SO.WeightedTier>();
            foreach (var t in ts) list.Add(new LootTable_SO.WeightedTier { tier = t.tier, weight = t.weight });
            return list;
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
            return a;
        }

        private static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(target, value);
        }
    }
}
#endif
