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
    /// BTS-G equipment content: fills the blank rarity ids (Database lookup keys) and authors the 4 previously
    /// empty armor-slot items (Shoulders / Back / Arms / Legs) so all 8 slots have at least one definition and the
    /// loot pool can cover every slot. Items are IDENTITY-ONLY (id + name + slot) — meshes/icons are a Meshy/Grok
    /// follow-up; stats are rolled at drop time via EquipmentRoller, so empty legacy stats are correct (ADR-019).
    /// Idempotent. Registers each item in the EquipmentCatalog.
    /// </summary>
    public static class EquipmentContentTool
    {
        private const string Dir = "Assets/Data/Equipment";
        private const string RarityDir = Dir + "/EquipmentRarity";

        [MenuItem("Tools/Build-To-Spec/Author Equipment Content (armor slots + rarity ids)")]
        public static void Author()
        {
            int rar = 0;
            foreach (var (file, id, name) in new[]
            {
                ("Rarity_Common",    "common",    "Rarity.Common.Name"),
                ("Rarity_Uncommon",  "uncommon",  "Rarity.Uncommon.Name"),
                ("Rarity_Rare",      "rare",      "Rarity.Rare.Name"),
                ("Rarity_Epic",      "epic",      "Rarity.Epic.Name"),
                ("Rarity_Legendary", "legendary", "Rarity.Legendary.Name"),
            })
            {
                var r = AssetDatabase.LoadAssetAtPath<Rarity_SO>($"{RarityDir}/{file}.asset");
                if (r == null) continue;
                SetField(r, "rarityId", id);
                SetField(r, "nameTermKey", name);
                EditorUtility.SetDirty(r);
                rar++;
            }

            int items = 0;
            items += MakeItem("Eq_Shoulderpad", "shoulderpad", "Equipment.Shoulderpad.Name", EquipmentSlot.Shoulders);
            items += MakeItem("Eq_Cape",        "cape",        "Equipment.Cape.Name",        EquipmentSlot.Back);
            items += MakeItem("Eq_Bracers",     "bracers",     "Equipment.Bracers.Name",     EquipmentSlot.Arms);
            items += MakeItem("Eq_Greaves",     "greaves",     "Equipment.Greaves.Name",     EquipmentSlot.Legs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EquipmentContentTool] Filled {rar} rarity ids; authored/ensured {items} armor-slot items " +
                      "(identity-only — meshes/icons are a Meshy follow-up) + registered them in the EquipmentCatalog. " +
                      "Re-run 'Author Loot Tables + Roll Config' to include them in the drop pool.");
        }

        private static int MakeItem(string assetName, string id, string nameKey, EquipmentSlot slot)
        {
            string path = $"{Dir}/{assetName}.asset";
            var e = AssetDatabase.LoadAssetAtPath<Equipment_SO>(path);
            if (e == null)
            {
                e = ScriptableObject.CreateInstance<Equipment_SO>();
                AssetDatabase.CreateAsset(e, path);
            }
            SetField(e, "equipmentId", id);
            SetField(e, "nameTermKey", nameKey);
            SetField(e, "slot", slot);
            EditorUtility.SetDirty(e);
            AddToCatalog(e);
            return 1;
        }

        private static void AddToCatalog(Equipment_SO item)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:EquipmentCatalog_SO"))
            {
                var cat = AssetDatabase.LoadAssetAtPath<EquipmentCatalog_SO>(AssetDatabase.GUIDToAssetPath(guid));
                if (cat == null) continue;
                var list = GetField<List<Equipment_SO>>(cat, "items");
                if (list == null || list.Contains(item)) continue;
                list.Add(item);
                EditorUtility.SetDirty(cat);
            }
        }

        private static void SetField(object t, string f, object v)
        {
            var fi = t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance);
            fi?.SetValue(t, v);
        }

        private static T GetField<T>(object t, string f) where T : class
        {
            var fi = t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance);
            return fi?.GetValue(t) as T;
        }
    }
}
#endif
