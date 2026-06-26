#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Classes;
using CapsuleWars.Data.StatusEffects;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Authors the 16 UnitClass_SO assets from the LOCKED roster (Docs/09 "Canonical Class Roster", 2/4/6 ladder).
    /// Idempotent. Only the [content] StatBuff tiers + globalBuffs are filled (numbers copied verbatim from the
    /// roster — first-pass/tunable, not "improved"). Tiers whose roster effect is [code] (armor-pen, DoT, splash,
    /// on-hit/kill heal, ramps, conditionals, ability-dmg %, healing-power, regen, strike-first, double-shot,
    /// reposition, pierce, backline-open) are authored as threshold + descTermKey with EMPTY buff lists —
    /// placeholders BTS-E2 fills once the ability/status/combat-hook slices land. Paladin is fully [content].
    /// </summary>
    public static class ClassSetupTool
    {
        private const string Dir = "Assets/Data/Classes";

        private static StatBuff P(StatType s, float amt) =>
            new StatBuff { stat = s, modType = StatBuffModType.Percent, amount = amt };

        private static List<StatBuff> B(params StatBuff[] b) => new List<StatBuff>(b);

        private static ClassSynergyTier Tier(int threshold, string descKey, List<StatBuff> team, List<StatBuff> global = null) =>
            new ClassSynergyTier
            {
                threshold = threshold,
                descTermKey = descKey,
                teamBuffs = team ?? new List<StatBuff>(),
                globalBuffs = global ?? new List<StatBuff>()
            };

        [MenuItem("Tools/Build-To-Spec/Author Unit Classes")]
        public static void Author()
        {
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Data", "Classes");

            int n = 0;

            // 1 Barbarian — Bloodrage  (T4 +[code] heal-on-kill · T6 +[code] +15% Atk while <50% HP)
            n += Make("Barbarian", new[]
            {
                Tier(2, "Class.Barbarian.Tier1.Desc", B(P(StatType.Atk, 12))),
                Tier(4, "Class.Barbarian.Tier2.Desc", B(P(StatType.Atk, 25))),
                Tier(6, "Class.Barbarian.Tier3.Desc", B(P(StatType.Atk, 40))),
            });

            // 2 Fighter — Flurry  (T6 +[code] every-3rd-hit-strikes-twice)
            n += Make("Fighter", new[]
            {
                Tier(2, "Class.Fighter.Tier1.Desc", B(P(StatType.Speed, 15))),
                Tier(4, "Class.Fighter.Tier2.Desc", B(P(StatType.Speed, 30))),
                Tier(6, "Class.Fighter.Tier3.Desc", B(P(StatType.Speed, 30))),
            });

            // 3 Archer — Volley  (T4/T6 +[code] atk-speed ramp)
            n += Make("Archer", new[]
            {
                Tier(2, "Class.Archer.Tier1.Desc", B(P(StatType.Atk, 12), P(StatType.Accuracy, 10))),
                Tier(4, "Class.Archer.Tier2.Desc", B(P(StatType.Atk, 25))),
                Tier(6, "Class.Archer.Tier3.Desc", B(P(StatType.Atk, 25))),
            });

            // 4 Spearman — Phalanx  (T2 +1 range = [code] · T4 +20% Def front-row = [code-cond] · T6 strike-first = [code])
            n += Make("Spearman", new[]
            {
                Tier(2, "Class.Spearman.Tier1.Desc", B(P(StatType.Accuracy, 10))),
                Tier(4, "Class.Spearman.Tier2.Desc", B()),
                Tier(6, "Class.Spearman.Tier3.Desc", B()),
            });

            // 5 Heavy — Bulwark  (T4 +[code] −15% ranged dmg · T6 global +8% Def team)
            n += Make("Heavy", new[]
            {
                Tier(2, "Class.Heavy.Tier1.Desc", B(P(StatType.Def, 20))),
                Tier(4, "Class.Heavy.Tier2.Desc", B(P(StatType.Def, 20), P(StatType.MaxHp, 15))),
                Tier(6, "Class.Heavy.Tier3.Desc", B(P(StatType.Def, 20), P(StatType.MaxHp, 15)), B(P(StatType.Def, 8))),
            });

            // 6 Wizard — Arcane  (all "elem ability dmg" + cooldown + global = [code])
            n += Make("Wizard", new[]
            {
                Tier(2, "Class.Wizard.Tier1.Desc", B()),
                Tier(4, "Class.Wizard.Tier2.Desc", B()),
                Tier(6, "Class.Wizard.Tier3.Desc", B()),
            });

            // 7 Javelin — Skirmish  (T4 +[code] reposition · T6 +[code] pierce)
            n += Make("Javelin", new[]
            {
                Tier(2, "Class.Javelin.Tier1.Desc", B(P(StatType.Atk, 12))),
                Tier(4, "Class.Javelin.Tier2.Desc", B(P(StatType.Atk, 12))),
                Tier(6, "Class.Javelin.Tier3.Desc", B(P(StatType.Atk, 25))),
            });

            // 8 Alchemist — Volatile  (all [code] DoT / splash / −Def debuff)
            n += Make("Alchemist", new[]
            {
                Tier(2, "Class.Alchemist.Tier1.Desc", B()),
                Tier(4, "Class.Alchemist.Tier2.Desc", B()),
                Tier(6, "Class.Alchemist.Tier3.Desc", B()),
            });

            // 9 Cleric — Blessing  (healing-power + team regen = [code]/global[code])
            n += Make("Cleric", new[]
            {
                Tier(2, "Class.Cleric.Tier1.Desc", B()),
                Tier(4, "Class.Cleric.Tier2.Desc", B()),
                Tier(6, "Class.Cleric.Tier3.Desc", B()),
            });

            // 10 Ambrosian — Elixir  (all [code] area-heal / cleanse)
            n += Make("Ambrosian", new[]
            {
                Tier(2, "Class.Ambrosian.Tier1.Desc", B()),
                Tier(4, "Class.Ambrosian.Tier2.Desc", B()),
                Tier(6, "Class.Ambrosian.Tier3.Desc", B()),
            });

            // 11 Assassin — Execute  (T4 +[code] +20% dmg vs <40% HP · T6 +[code] opens on backline)
            n += Make("Assassin", new[]
            {
                Tier(2, "Class.Assassin.Tier1.Desc", B(P(StatType.CritRate, 15), P(StatType.CritDmg, 20))),
                Tier(4, "Class.Assassin.Tier2.Desc", B(P(StatType.CritRate, 15), P(StatType.CritDmg, 20))),
                Tier(6, "Class.Assassin.Tier3.Desc", B(P(StatType.CritRate, 15), P(StatType.CritDmg, 40))),
            });

            // 12 Monk — Harmony  (heal-on-hit = [code] · T6 global +5% MaxHp team)
            n += Make("Monk", new[]
            {
                Tier(2, "Class.Monk.Tier1.Desc", B(P(StatType.Atk, 10))),
                Tier(4, "Class.Monk.Tier2.Desc", B(P(StatType.Atk, 15))),
                Tier(6, "Class.Monk.Tier3.Desc", B(P(StatType.Atk, 15)), B(P(StatType.MaxHp, 5))),
            });

            // 13 Crossbow — Pierce  (ignore-Def = [code]; T4/T6 add CritDmg [content])
            n += Make("Crossbow", new[]
            {
                Tier(2, "Class.Crossbow.Tier1.Desc", B()),
                Tier(4, "Class.Crossbow.Tier2.Desc", B(P(StatType.CritDmg, 15))),
                Tier(6, "Class.Crossbow.Tier3.Desc", B(P(StatType.CritDmg, 30))),
            });

            // 14 HandGunner — Gunline  (all [code]; T4's −10% Speed is deferred WITH its [code] dmg trade-off)
            n += Make("HandGunner", new[]
            {
                Tier(2, "Class.HandGunner.Tier1.Desc", B()),
                Tier(4, "Class.HandGunner.Tier2.Desc", B()),
                Tier(6, "Class.HandGunner.Tier3.Desc", B()),
            });

            // 15 Siegebreaker — Demolition  (all [code] bomb AoE / splash / armor-break + slow)
            n += Make("Siegebreaker", new[]
            {
                Tier(2, "Class.Siegebreaker.Tier1.Desc", B()),
                Tier(4, "Class.Siegebreaker.Tier2.Desc", B()),
                Tier(6, "Class.Siegebreaker.Tier3.Desc", B()),
            });

            // 16 Paladin — Aegis  (PURE [content] + globals)
            n += Make("Paladin", new[]
            {
                Tier(2, "Class.Paladin.Tier1.Desc", B(P(StatType.Def, 15), P(StatType.Resistance, 15))),
                Tier(4, "Class.Paladin.Tier2.Desc", B(P(StatType.Def, 15), P(StatType.Resistance, 15)), B(P(StatType.Resistance, 10))),
                Tier(6, "Class.Paladin.Tier3.Desc", B(P(StatType.Def, 20), P(StatType.Resistance, 20)), B(P(StatType.Def, 10), P(StatType.Resistance, 10))),
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ClassSetupTool] Authored/updated {n} UnitClass_SO assets under {Dir} (16-class roster, 2/4/6 ladder). " +
                      "[content] stat tiers + globalBuffs filled verbatim from Docs/09; [code] behavioral tiers left as desc-only placeholders (BTS-E2).");
        }

        private static int Make(string id, ClassSynergyTier[] tiers)
        {
            string path = $"{Dir}/Class_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<UnitClass_SO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<UnitClass_SO>();
                AssetDatabase.CreateAsset(so, path);
            }
            SetField(so, "classId", id);
            SetField(so, "nameTermKey", $"Class.{id}.Name");
            SetField(so, "descTermKey", $"Class.{id}.Desc");
            SetField(so, "tiers", new List<ClassSynergyTier>(tiers));
            EditorUtility.SetDirty(so);
            return 1;
        }

        private static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(target, value);
        }
    }
}
#endif
