#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Abilities;
using CapsuleWars.Data.Classes;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// BTS-F part 2: authors the <see cref="ClassAbilitySet_SO"/> that maps each of the 16 roster classes to the
    /// 2 ability move kits AbilitySetupTool generated for it (basic + signature). Idempotent — loads-or-creates
    /// Assets/Data/Abilities/ClassAbilitySet.asset and overwrites its entries in place. Excludes the legacy
    /// placeholder Class_Warrior. Does NOT touch any prefab — wiring the loader onto the base prefab + Play-verify
    /// is the deliberate, Play-gated activation step (see Docs/PROJECT_STATE).
    /// </summary>
    public static class ClassAbilitySetupTool
    {
        private const string AssetPath = "Assets/Data/Abilities/ClassAbilitySet.asset";
        private const string ClassDir = "Assets/Data/Classes/";
        private const string AbilityDir = "Assets/Data/Abilities/Generated/";

        // class asset (no ext) -> its two ability assets (no ext). Grouped by the class prefix on each Ability_SO;
        // basic first, signature second (order is cosmetic — both go in the kit). Mirrors AbilitySetupTool's output.
        private static readonly (string cls, string a1, string a2)[] Table =
        {
            ("Class_Archer",       "Ability_archer_basic_shot",       "Ability_archer_sig_volley"),
            ("Class_Barbarian",    "Ability_barbarian_basic_cleave",  "Ability_barbarian_sig_brutalstrike"),
            ("Class_Spearman",     "Ability_spearman_basic_thrust",   "Ability_spearman_sig_skewer"),
            ("Class_Fighter",      "Ability_fighter_basic_jab",       "Ability_fighter_sig_flurry"),
            ("Class_Monk",         "Ability_monk_palm_strike",        "Ability_monk_harmony_flow"),
            ("Class_Paladin",      "Ability_paladin_strike",          "Ability_paladin_aegis"),
            ("Class_Cleric",       "Ability_cleric_smite",            "Ability_cleric_blessing_mend"),
            ("Class_Wizard",       "Ability_wizard_arcane_bolt",      "Ability_wizard_elemental_blast"),
            ("Class_Assassin",     "Ability_assassin_quick_cut",      "Ability_assassin_rupture"),
            ("Class_Crossbow",     "Ability_crossbow_bolt",           "Ability_crossbow_piercing_bolt"),
            ("Class_HandGunner",   "Ability_handgunner_shot",         "Ability_handgunner_opening_volley"),
            ("Class_Heavy",        "Ability_heavy_shield_bash",       "Ability_heavy_crushing_guard"),
            ("Class_Javelin",      "Ability_javelin_throw",           "Ability_javelin_piercing_mark"),
            ("Class_Alchemist",    "Ability_alchemist_potion_lob",    "Ability_alchemist_corrosive_flask"),
            ("Class_Siegebreaker", "Ability_siegebreaker_throw",      "Ability_siegebreaker_demolition_bomb"),
            ("Class_Ambrosian",    "Ability_ambrosian_elixir",        "Ability_ambrosian_splash_vial"),
        };

        [MenuItem("Tools/Build-To-Spec/Author Class Ability Sets")]
        public static void Author()
        {
            var set = AssetDatabase.LoadAssetAtPath<ClassAbilitySet_SO>(AssetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ClassAbilitySet_SO>();
                AssetDatabase.CreateAsset(set, AssetPath);
            }

            var entries = new List<ClassAbilitySet_SO.Entry>();
            int missing = 0;
            foreach (var row in Table)
            {
                var cls = AssetDatabase.LoadAssetAtPath<UnitClass_SO>(ClassDir + row.cls + ".asset");
                if (cls == null) { Debug.LogWarning($"[ClassAbilitySetup] missing class asset {row.cls}"); missing++; continue; }

                var a1 = AssetDatabase.LoadAssetAtPath<Ability_SO>(AbilityDir + row.a1 + ".asset");
                var a2 = AssetDatabase.LoadAssetAtPath<Ability_SO>(AbilityDir + row.a2 + ".asset");
                if (a1 == null) { Debug.LogWarning($"[ClassAbilitySetup] missing ability {row.a1}"); missing++; }
                if (a2 == null) { Debug.LogWarning($"[ClassAbilitySetup] missing ability {row.a2}"); missing++; }

                var kit = new List<Ability_SO>();
                if (a1 != null) kit.Add(a1);
                if (a2 != null) kit.Add(a2);
                entries.Add(new ClassAbilitySet_SO.Entry { unitClass = cls, abilities = kit.ToArray() });
            }

            typeof(ClassAbilitySet_SO).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(set, entries);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ClassAbilitySetup] authored {entries.Count} class kits ({missing} missing refs) at {AssetPath}");
        }
    }
}
#endif
