#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Authors the 24 StatusEffect_SO assets (Docs/10) + the 5 damage behavior assets (BTS-D), wiring the
    /// behavioral statuses (Frozen/Marked/Protected/Shield/LastStand) to the BTS-B2 hook. Idempotent.
    /// Numbers are first-pass / tunable (Docs/10 gives behaviors + a few example durations, not full balance).
    /// [code] follow-ups noted inline: Unlucky's ÷2 crit + roll-skew, Madness's random targeting, LastStand's
    /// +Atk / one-time — those need attack-roll / targeting / conditional-stat hooks, not built here.
    /// </summary>
    public static class StatusSetupTool
    {
        private const string Dir = "Assets/Data/StatusEffects";
        private const string BehaviorDir = "Assets/Data/StatusEffects/Behaviors";

        private static int n;

        private static StatBuff P(StatType s, float amt) =>
            new StatBuff { stat = s, modType = StatBuffModType.Percent, amount = amt };

        private static List<StatBuff> B(params StatBuff[] b) => new List<StatBuff>(b);

        [MenuItem("Tools/Build-To-Spec/Author Status Effects")]
        public static void Author()
        {
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Data", "StatusEffects");
            if (!AssetDatabase.IsValidFolder(BehaviorDir)) AssetDatabase.CreateFolder(Dir, "Behaviors");
            n = 0;

            // --- 5 damage behaviors (BTS-B2 code classes → assets) ---
            var bMarked    = MakeBehavior<MarkedBehavior>("Behavior_Marked");
            var bFrozen    = MakeBehavior<FrozenBehavior>("Behavior_Frozen");
            var bProtected = MakeBehavior<ProtectedBehavior>("Behavior_Protected");
            var bShield    = MakeBehavior<ShieldBehavior>("Behavior_Shield");
            var bLastStand = MakeBehavior<LastStandBehavior>("Behavior_LastStand");

            // --- Control / debuffs (10) ---
            Make("Stunned",  StatusEffectKind.Control, 2f, preventsAction: true, preventsMovement: true);
            Make("Frozen",   StatusEffectKind.Control, 2f, preventsAction: true, behavior: bFrozen);     // ×1.5 physical
            Make("Trapped",  StatusEffectKind.Control, 3f, preventsMovement: true);
            Make("Marked",   StatusEffectKind.Debuff,  5f, behavior: bMarked);                           // +25% taken
            Make("Unlucky",  StatusEffectKind.Debuff,  6f, buffs: B(P(StatType.CritRate, -50f)));        // ÷2 crit approx; roll-skew = [code]
            Make("LastStand",StatusEffectKind.Buff,   -1f, behavior: bLastStand);                        // dmg reduction <20% HP; +Atk/one-time = [code]
            Make("Madness",  StatusEffectKind.Control, 4f);                                              // random targeting = [code]
            Make("Cursed",   StatusEffectKind.Debuff,  5f, buffs: B(
                P(StatType.Atk, -25f), P(StatType.Def, -25f), P(StatType.Speed, -25f),
                P(StatType.Accuracy, -25f), P(StatType.CritRate, -25f)));
            Make("Silenced", StatusEffectKind.Debuff,  4f, preventsAbilities: true);
            Make("Bleeding", StatusEffectKind.DoT,     5f, tickInterval: 1f, tickAmount: -5, tickPct: true);

            // --- Buffs / boons (4) ---
            Make("Protected",   StatusEffectKind.Buff, 8f,  behavior: bProtected);                       // negate next hit
            Make("Shield",      StatusEffectKind.Buff, 10f, behavior: bShield, behaviorMagnitude: 30f);  // absorb 30
            Make("Regenerating",StatusEffectKind.HoT,  5f,  tickInterval: 1f, tickAmount: 5, tickPct: true);
            Make("Empowered",   StatusEffectKind.Buff, 5f,  buffs: B(P(StatType.Atk, 25f)));

            // --- Stat buff / broken pairs (10) ---
            Make("AtkUp",       StatusEffectKind.Buff,   10f, buffs: B(P(StatType.Atk, 25f)));
            Make("AtkDown",     StatusEffectKind.Debuff, 10f, buffs: B(P(StatType.Atk, -25f)));
            Make("DefUp",       StatusEffectKind.Buff,   10f, buffs: B(P(StatType.Def, 25f)));
            Make("DefDown",     StatusEffectKind.Debuff, 10f, buffs: B(P(StatType.Def, -25f)));
            Make("SpeedUp",     StatusEffectKind.Buff,   10f, buffs: B(P(StatType.Speed, 25f)));
            Make("SpeedDown",   StatusEffectKind.Debuff, 10f, buffs: B(P(StatType.Speed, -25f)));
            Make("AccuracyUp",  StatusEffectKind.Buff,   10f, buffs: B(P(StatType.Accuracy, 25f)));
            Make("AccuracyDown",StatusEffectKind.Debuff, 10f, buffs: B(P(StatType.Accuracy, -25f)));
            Make("CritUp",      StatusEffectKind.Buff,   10f, buffs: B(P(StatType.CritRate, 25f)));
            Make("CritDown",    StatusEffectKind.Debuff, 10f, buffs: B(P(StatType.CritRate, -25f)));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[StatusSetupTool] Authored/updated {n} StatusEffect_SO assets + 5 behaviors under {Dir} " +
                      "(Docs/10, 2/4/6 ladder n/a — first-pass durations/magnitudes). Behavioral statuses wired to BTS-B2 hooks.");
        }

        private static T MakeBehavior<T>(string fileName) where T : StatusEffectBehavior
        {
            string path = $"{BehaviorDir}/{fileName}.asset";
            var so = AssetDatabase.LoadAssetAtPath<T>(path);
            if (so == null) { so = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(so, path); }
            return so;
        }

        private static void Make(string id, StatusEffectKind kind, float duration,
            List<StatBuff> buffs = null, float tickInterval = 1f, int tickAmount = 0, bool tickPct = false,
            bool preventsAction = false, bool preventsMovement = false, bool preventsAbilities = false,
            StatusEffectBehavior behavior = null, float behaviorMagnitude = 0f)
        {
            string path = $"{Dir}/Status_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<StatusEffect_SO>(path);
            if (so == null) { so = ScriptableObject.CreateInstance<StatusEffect_SO>(); AssetDatabase.CreateAsset(so, path); }

            SetField(so, "statusId", id);
            SetField(so, "nameTermKey", $"Status.{id}.Name");
            SetField(so, "descTermKey", $"Status.{id}.Desc");
            SetField(so, "kind", kind);
            SetField(so, "defaultDuration", duration);
            SetField(so, "tickInterval", tickInterval);
            SetField(so, "tickAmount", tickAmount);
            SetField(so, "tickIsPercentOfMaxHp", tickPct);
            SetField(so, "preventsAction", preventsAction);
            SetField(so, "preventsMovement", preventsMovement);
            SetField(so, "preventsAbilities", preventsAbilities);
            SetField(so, "behaviorSO", behavior);
            SetField(so, "behaviorMagnitude", behaviorMagnitude);
            SetField(so, "statBuffs", buffs ?? new List<StatBuff>());
            EditorUtility.SetDirty(so);
            n++;
        }

        private static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(target, value);
        }
    }
}
#endif
