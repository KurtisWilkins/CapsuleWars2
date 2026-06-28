#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Abilities;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Data.Weapons;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// BTS-F: authors the first-pass ability MOVE KITS — 32 Ability_SO (2 per class for the 16-class roster) +
    /// the ~54 new strategy assets they compose (DamageEffect / HealEffect / ApplyStatusEffect / TimeBasedTrigger /
    /// event triggers / targeting / filters), reusing the 6 existing strategy instances. Designed + adversarially
    /// verified by a multi-agent workflow against the real strategy library, the 24 statuses, and the 16 weapon
    /// classes; numbers are first-pass / tunable. Idempotent. Does NOT wire abilities to units — per-class
    /// assignment onto AbilityController is BTS-F part 2 (a separate, Play-gated step).
    /// </summary>
    public static class AbilitySetupTool
    {
        private const string GenDir = "Assets/Data/Abilities/Generated";
        private const string StatusDir = "Assets/Data/StatusEffects";

        private static readonly Dictionary<string, ScriptableObject> reg = new();

        [MenuItem("Tools/Build-To-Spec/Author Ability Move Kits")]
        public static void Author()
        {
            reg.Clear();
            if (!AssetDatabase.IsValidFolder("Assets/Data/Abilities")) AssetDatabase.CreateFolder("Assets/Data", "Abilities");
            if (!AssetDatabase.IsValidFolder(GenDir)) AssetDatabase.CreateFolder("Assets/Data/Abilities", "Generated");

            // ---- DamageEffect (name, basePower, addAttackerAtk) ----
            Dmg("Eff_Damage28_Atk", 28, true); Dmg("Eff_Damage50_Atk", 50, true);
            Dmg("Eff_Damage18", 18, false); Dmg("Eff_Damage22", 22, false); Dmg("Eff_Damage24", 24, false);
            Dmg("Eff_Damage40", 40, false); Dmg("Eff_Damage16", 16, false); Dmg("Eff_Damage20", 20, false);
            Dmg("Eff_Damage32", 32, false);
            Dmg("Eff_Damage20Atk", 20, true); Dmg("Eff_Damage30Atk", 30, true); Dmg("Eff_Damage45Atk", 45, true);
            Dmg("Eff_Damage24Atk", 24, true); Dmg("Eff_Damage32Atk", 32, true); Dmg("Eff_Damage38Atk", 38, true);
            Dmg("Eff_Damage70Atk", 70, true); Dmg("Eff_Damage28Atk", 28, true);

            // ---- HealEffect (name, basePower, revivesDowned) ----
            Heal("Eff_HealMend40", 40, false); Heal("Eff_HealOnHit6", 6, false);

            // ---- ApplyStatusEffect (name, statusAssetName) ----
            Status("Eff_ApplyDefDown", "Status_DefDown"); Status("Eff_ApplyDefUp", "Status_DefUp");
            Status("Eff_ApplyAtkUp", "Status_AtkUp"); Status("Eff_ApplySpeedUp", "Status_SpeedUp");
            Status("Eff_ApplySpeedDown", "Status_SpeedDown"); Status("Eff_ApplyMarked", "Status_Marked");
            Status("Eff_ApplyFrozen", "Status_Frozen"); Status("Eff_ApplyBleeding", "Status_Bleeding");
            Status("Eff_ApplyRegenerating", "Status_Regenerating"); Status("Eff_ApplyShield", "Status_Shield");

            // ---- TimeBasedTrigger (name, cooldown, initialDelay) ----
            Tb("TimeBasedTriggerC11D00", 1.1f, 0f); Tb("TimeBasedTriggerC10D00", 1.0f, 0f);
            Tb("TimeBasedTriggerC12D00", 1.2f, 0f); Tb("TimeBasedTriggerC08D00fast", 0.8f, 0f);
            Tb("TimeBasedTriggerC06D10", 6f, 1f); Tb("TimeBasedTriggerC06D15", 6f, 1.5f);
            Tb("TimeBasedTriggerC04D10", 4f, 1f); Tb("TimeBasedTriggerC05D10", 5f, 1f);
            Tb("TimeBasedTriggerC35D05", 3.5f, 0.5f); Tb("TimeBasedTriggerC06D30", 6f, 3f);
            Tb("TimeBasedTriggerC07D20", 7f, 2f); Tb("TimeBasedTriggerC12D03", 1.2f, 0.3f);
            Tb("TimeBasedTriggerC12D025", 1.2f, 0.25f); Tb("TimeBasedTriggerC11D02", 1.1f, 0.2f);
            Tb("TimeBasedTriggerC10D02", 1.0f, 0.2f);

            // ---- event triggers ----
            Make<OnHitTrigger_SO>("OnHitTrigger"); Make<OnBattleStartTrigger_SO>("OnBattleStartTrigger");
            Set(Make<OnLowHpTrigger_SO>("OnLowHpTrigger50"), "hpThreshold", 0.5f);

            // ---- targeting ----
            Make<GetSelfTarget_SO>("GetSelfTarget"); Make<GetCurrentTarget_SO>("GetCurrentTarget");
            Set(Make<GetAllyTargets_SO>("GetAllyTargets"), "includeDowned", false);

            // ---- filters ----
            Set(Make<ClosestNFilter_SO>("Filter_Closest3"), "n", 3);
            Make<InRangeFilter_SO>("Filter_InRange");
            Set(Make<LowestHpFilter_SO>("Filter_LowestHp1"), "n", 1);
            Make<KeepCurrentTargetFilter_SO>("Filter_KeepCurrentTarget");

            // ---- existing reusables (load by name) ----
            foreach (var name in new[] { "Eff_Damage25", "Eff_ApplyStun", "Filter_Closest1",
                                         "GetEnemyTargets", "TimeBasedTriggerC05D00", "TimeBasedTriggerC08D00" })
                LoadExisting(name);

            int strategyCount = reg.Count;

            // ---- abilities (id, display, trigger, targeting, filters[], effects[], range, weapon) ----
            int n = 0;
            n += Ab("barbarian_basic_cleave", "Cleave", "TimeBasedTriggerC11D00", "GetEnemyTargets", A("Filter_Closest1"), A("Eff_Damage28_Atk"), 2.2f, "WC_Melee2H");
            n += Ab("barbarian_sig_brutalstrike", "Brutal Strike", "TimeBasedTriggerC06D10", "GetEnemyTargets", A("Filter_Closest1"), A("Eff_Damage50_Atk", "Eff_ApplyDefDown"), 2.2f, "WC_Melee2H");
            n += Ab("fighter_basic_jab", "Jab", "TimeBasedTriggerC08D00fast", "GetEnemyTargets", A("Filter_Closest1"), A("Eff_Damage18"), 2f, "WC_Dual1H");
            n += Ab("fighter_sig_flurry", "Flurry", "TimeBasedTriggerC05D00", "GetSelfTarget", A(), A("Eff_ApplyAtkUp", "Eff_ApplySpeedUp"), 0f, "WC_Dual1H");
            n += Ab("archer_basic_shot", "Aimed Shot", "TimeBasedTriggerC10D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage22"), 9f, "WC_Bow");
            n += Ab("archer_sig_volley", "Volley", "TimeBasedTriggerC06D15", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest3"), A("Eff_Damage18", "Eff_ApplyMarked"), 9f, "WC_Bow");
            n += Ab("spearman_basic_thrust", "Thrust", "TimeBasedTriggerC11D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage24"), 3.2f, "WC_Spear");
            n += Ab("spearman_sig_skewer", "Skewer", "TimeBasedTriggerC05D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage40", "Eff_ApplySpeedDown"), 3.5f, "WC_Spear");
            n += Ab("heavy_shield_bash", "Shield Bash", "TimeBasedTriggerC12D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage20Atk"), 2f, "WC_TowerShield");
            n += Ab("heavy_crushing_guard", "Crushing Guard", "TimeBasedTriggerC08D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage30Atk", "Eff_ApplyStun"), 2f, "WC_TowerShield");
            n += Ab("wizard_arcane_bolt", "Arcane Bolt", "TimeBasedTriggerC10D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage22"), 7f, "WC_Staff");
            n += Ab("wizard_elemental_blast", "Elemental Blast", "TimeBasedTriggerC08D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage45Atk", "Eff_ApplyFrozen"), 7f, "WC_Staff");
            n += Ab("javelin_throw", "Javelin Throw", "TimeBasedTriggerC10D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage24Atk"), 6f, "WC_ThrownJavelin");
            n += Ab("javelin_piercing_mark", "Piercing Mark", "TimeBasedTriggerC08D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage32Atk", "Eff_ApplyMarked"), 6f, "WC_ThrownJavelin");
            n += Ab("alchemist_potion_lob", "Potion Lob", "TimeBasedTriggerC12D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage18"), 6f, "WC_ThrownPotion");
            n += Ab("alchemist_corrosive_flask", "Corrosive Flask", "TimeBasedTriggerC08D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage20", "Eff_ApplyBleeding", "Eff_ApplyDefDown"), 6f, "WC_ThrownPotion");
            n += Ab("cleric_smite", "Smite", "TimeBasedTriggerC12D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage18"), 6f, "WC_HolyFocus");
            n += Ab("cleric_blessing_mend", "Mend", "TimeBasedTriggerC04D10", "GetAllyTargets", A("Filter_LowestHp1"), A("Eff_HealMend40"), 0f, "WC_HolyFocus");
            n += Ab("ambrosian_splash_vial", "Splash Vial", "TimeBasedTriggerC12D00", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage16"), 6f, "WC_ThrownPotionSupport");
            n += Ab("ambrosian_elixir", "Healing Elixir", "TimeBasedTriggerC05D10", "GetAllyTargets", A("Filter_LowestHp1"), A("Eff_ApplyRegenerating"), 0f, "WC_ThrownPotionSupport");
            n += Ab("assassin_quick_cut", "Quick Cut", "TimeBasedTriggerC08D00fast", "GetEnemyTargets", A("Filter_Closest1"), A("Eff_Damage25"), 5f, "WC_Dagger");
            n += Ab("assassin_rupture", "Rupture", "TimeBasedTriggerC35D05", "GetCurrentTarget", A("Filter_KeepCurrentTarget"), A("Eff_Damage22", "Eff_ApplyBleeding"), 5f, "WC_Dagger");
            n += Ab("monk_palm_strike", "Palm Strike", "TimeBasedTriggerC10D00", "GetEnemyTargets", A("Filter_Closest1"), A("Eff_Damage22"), 5f, "WC_Unarmed");
            n += Ab("monk_harmony_flow", "Harmony Flow", "OnHitTrigger", "GetSelfTarget", A(), A("Eff_HealOnHit6"), 0f, "WC_Unarmed");
            n += Ab("crossbow_bolt", "Crossbow Bolt", "TimeBasedTriggerC11D02", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage30Atk"), 8f, "WC_Crossbow");
            n += Ab("crossbow_piercing_bolt", "Piercing Bolt", "TimeBasedTriggerC06D30", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage45Atk", "Eff_ApplyDefDown", "Eff_ApplyBleeding"), 8f, "WC_Crossbow");
            n += Ab("handgunner_shot", "Musket Shot", "TimeBasedTriggerC12D03", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage38Atk"), 10f, "WC_Musket");
            n += Ab("handgunner_opening_volley", "Opening Volley", "OnBattleStartTrigger", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage70Atk", "Eff_ApplyDefDown"), 10f, "WC_Musket");
            n += Ab("siegebreaker_throw", "Bomb Toss", "TimeBasedTriggerC12D025", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage28Atk"), 6.5f, "WC_ThrownBomb");
            n += Ab("siegebreaker_demolition_bomb", "Demolition Bomb", "TimeBasedTriggerC07D20", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest3"), A("Eff_Damage32", "Eff_ApplyDefDown", "Eff_ApplySpeedDown"), 6.5f, "WC_ThrownBomb");
            n += Ab("paladin_strike", "Shield Bash", "TimeBasedTriggerC10D02", "GetEnemyTargets", A("Filter_InRange", "Filter_Closest1"), A("Eff_Damage20Atk"), 2f, "WC_HolyShield");
            n += Ab("paladin_aegis", "Aegis", "OnLowHpTrigger50", "GetAllyTargets", A("Filter_LowestHp1"), A("Eff_ApplyShield", "Eff_ApplyDefUp"), 0f, "WC_HolyShield");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AbilitySetupTool] Authored {strategyCount} strategy assets + {n} abilities (16 classes x 2) under {GenDir}. " +
                      "First-pass numbers. Wire abilities to units via AbilityController (per-class assignment) — BTS-F part 2.");
        }

        // ---- strategy creators ----
        private static void Dmg(string name, int bp, bool atk)
        {
            var d = Make<DamageEffect_SO>(name);
            Set(d, "basePower", bp); Set(d, "addAttackerAtk", atk);
        }
        private static void Heal(string name, int bp, bool rev)
        {
            var h = Make<HealEffect_SO>(name);
            Set(h, "basePower", bp); Set(h, "revivesDowned", rev);
        }
        private static void Status(string name, string statusAsset)
        {
            var s = Make<ApplyStatusEffect_SO>(name);
            var status = AssetDatabase.LoadAssetAtPath<StatusEffect_SO>($"{StatusDir}/{statusAsset}.asset");
            if (status == null) Debug.LogError($"[AbilitySetupTool] status {statusAsset} not found for {name}");
            Set(s, "statusEffect", status);
        }
        private static void Tb(string name, float cd, float delay)
        {
            var t = Make<TimeBasedTrigger_SO>(name);
            Set(t, "cooldown", cd); Set(t, "initialDelay", delay);
        }

        // ---- ability composer ----
        private static int Ab(string id, string display, string trigger, string targeting, string[] filters, string[] effects, float range, string weapon)
        {
            var a = Make<Ability_SO>($"Ability_{id}");
            Set(a, "abilityId", id);
            Set(a, "nameTermKey", $"Ability.{id}.Name");
            Set(a, "descTermKey", $"Ability.{id}.Desc");
            Set(a, "range", range);

            var wc = LoadWeapon(weapon);
            Set(a, "requiredWeaponClasses", wc != null ? new[] { wc } : new WeaponClass_SO[0]);

            Set(a, "trigger", reg.TryGetValue(trigger, out var tg) ? tg : null);
            Set(a, "targeting", reg.TryGetValue(targeting, out var tt) ? tt : null);

            var fl = new List<AbilityFilterStrategy>();
            foreach (var f in filters) if (reg.TryGetValue(f, out var so)) fl.Add(so as AbilityFilterStrategy);
            Set(a, "filters", fl);

            var ef = new List<AbilityEffectStrategy>();
            foreach (var e in effects) if (reg.TryGetValue(e, out var so)) ef.Add(so as AbilityEffectStrategy);
            Set(a, "effects", ef);
            return 1;
        }

        // ---- helpers ----
        private static string[] A(params string[] names) => names;

        private static T Make<T>(string name) where T : ScriptableObject
        {
            string path = $"{GenDir}/{name}.asset";
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
            reg[name] = a;
            EditorUtility.SetDirty(a);
            return a;
        }

        private static void LoadExisting(string name)
        {
            foreach (var guid in AssetDatabase.FindAssets(name))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith($"/{name}.asset")) continue;
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null) { reg[name] = so; return; }
            }
            Debug.LogWarning($"[AbilitySetupTool] reusable {name} not found.");
        }

        private static WeaponClass_SO LoadWeapon(string name)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:WeaponClass_SO {name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith($"/{name}.asset")) return AssetDatabase.LoadAssetAtPath<WeaponClass_SO>(path);
            }
            Debug.LogError($"[AbilitySetupTool] weapon class {name} not found.");
            return null;
        }

        private static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) { Debug.LogError($"[AbilitySetupTool] field '{field}' not found on {target.GetType().Name}"); return; }
            f.SetValue(target, value);
            EditorUtility.SetDirty((Object)target);
        }
    }
}
#endif
