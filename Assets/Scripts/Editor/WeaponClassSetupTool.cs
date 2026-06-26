#if UNITY_EDITOR
using CapsuleWars.Core;
using CapsuleWars.Data.Weapons;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// One-shot authoring of the WeaponClass_SO assets the 16-class roster needs (BTS-C; Docs/09 roster).
    /// Idempotent — re-running updates the existing assets. Also fixes the mislabeled WC_1HSword (its id/name
    /// said "Unarmed") by repurposing it as the generic Sword1H. Stats are FIRST-PASS / tunable (the roster gives
    /// synergy tiers, not weapon stats; these mirror the authored WC_Unarmed style). weaponTypeId selects the
    /// Animator sub-state machine — distinct ids are assigned for when per-weapon anims are authored; no unit
    /// references these yet, so animation is unaffected until BTS-E assigns classes/weapons.
    /// </summary>
    public static class WeaponClassSetupTool
    {
        private const string Dir = "Assets/Data/Weapons";

        private readonly struct WC
        {
            public readonly string id; public readonly string file;
            public readonly int typeId, count; public readonly float range, cooldown;
            public readonly WeaponHandedness hand;
            public WC(string id, string file, int typeId, float range, float cooldown, int count, WeaponHandedness hand)
            { this.id = id; this.file = file; this.typeId = typeId; this.range = range; this.cooldown = cooldown; this.count = count; this.hand = hand; }
        }

        [MenuItem("Tools/Build-To-Spec/Author Weapon Classes")]
        public static void Author()
        {
            var defs = new[]
            {
                // id, file (WC_1HSword reused for the generic Sword1H fix), typeId, range, cooldown, attackCount, handedness
                new WC("Sword1H",             "WC_1HSword",            1, 2.0f, 1.4f, 2, WeaponHandedness.OneHanded),
                new WC("Melee2H",             "WC_Melee2H",            2, 2.2f, 1.9f, 2, WeaponHandedness.TwoHanded),  // Barbarian
                new WC("Dual1H",              "WC_Dual1H",            1, 2.0f, 0.9f, 2, WeaponHandedness.Dual),       // Fighter
                new WC("Bow",                 "WC_Bow",               3, 9.0f, 1.6f, 1, WeaponHandedness.TwoHanded),  // Archer
                new WC("Spear",               "WC_Spear",             1, 3.2f, 1.5f, 1, WeaponHandedness.TwoHanded),  // Spearman
                new WC("TowerShield",         "WC_TowerShield",       1, 2.0f, 1.8f, 1, WeaponHandedness.Shield),     // Heavy
                new WC("Staff",               "WC_Staff",             4, 7.0f, 2.0f, 1, WeaponHandedness.TwoHanded),  // Wizard/Cleric/Monk
                new WC("Wand",                "WC_Wand",              4, 7.0f, 1.7f, 1, WeaponHandedness.OneHanded),  // Wizard alt
                new WC("HolyFocus",           "WC_HolyFocus",         4, 6.0f, 2.0f, 1, WeaponHandedness.OneHanded),  // Cleric
                new WC("ThrownJavelin",       "WC_ThrownJavelin",     3, 6.0f, 1.5f, 1, WeaponHandedness.OneHanded),  // Javelin
                new WC("ThrownPotion",        "WC_ThrownPotion",      3, 6.0f, 2.0f, 1, WeaponHandedness.OneHanded),  // Alchemist
                new WC("ThrownPotionSupport", "WC_ThrownPotionSupport",3, 6.0f, 2.0f, 1, WeaponHandedness.OneHanded), // Ambrosian
                new WC("Dagger",              "WC_Dagger",            1, 1.8f, 0.8f, 2, WeaponHandedness.OneHanded),  // Assassin
                new WC("Crossbow",            "WC_Crossbow",          3, 8.0f, 2.2f, 1, WeaponHandedness.TwoHanded),  // Crossbow
                new WC("Musket",              "WC_Musket",            3, 10.0f, 2.6f, 1, WeaponHandedness.TwoHanded), // HandGunner
                new WC("ThrownBomb",          "WC_ThrownBomb",        3, 6.5f, 2.4f, 1, WeaponHandedness.OneHanded),  // Siegebreaker
                new WC("HolyShield",          "WC_HolyShield",        1, 2.0f, 1.7f, 1, WeaponHandedness.Shield),     // Paladin
            };

            int created = 0, updated = 0;
            foreach (var d in defs)
            {
                if (Make(d)) created++; else updated++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WeaponClassSetupTool] Weapon classes authored under {Dir}: {created} created, {updated} updated " +
                      $"(of {defs.Length}). WC_1HSword relabeled to Sword1H. (Fist = existing WC_Unarmed, Monk.)");
        }

        private static bool Make(WC d)
        {
            string path = $"{Dir}/{d.file}.asset";
            var so = AssetDatabase.LoadAssetAtPath<WeaponClass_SO>(path);
            bool isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<WeaponClass_SO>();
                AssetDatabase.CreateAsset(so, path);
            }

            var s = new SerializedObject(so);
            s.FindProperty("weaponClassId").stringValue = d.id;
            s.FindProperty("nameTermKey").stringValue = $"Weapon.{d.id}.Name";
            s.FindProperty("weaponTypeId").intValue = d.typeId;
            s.FindProperty("attackCount").intValue = d.count;
            s.FindProperty("attackRange").floatValue = d.range;
            s.FindProperty("attackCooldown").floatValue = d.cooldown;
            s.FindProperty("handedness").enumValueIndex = (int)d.hand;   // enum values are sequential 0..4 → index == value
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(so);
            return isNew;
        }
    }
}
#endif
