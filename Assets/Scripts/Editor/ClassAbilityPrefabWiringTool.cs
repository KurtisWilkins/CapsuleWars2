#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Abilities;
using CapsuleWars.Data.Classes;
using CapsuleWars.Units.Controllers;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// BTS-F part 2 ACTIVATION (uniform first-pass): wires the base unit prefab so spawned units cast a real class
    /// kit instead of the placeholder QuickStrike. Repoints <see cref="UnitStatusController"/>.unitClass to
    /// Class_Monk (WC_Unarmed → its abilities fire with no equipped weapon, sidestepping the weapon-gate), clears
    /// the serialized <see cref="AbilityController"/>.abilities (the <see cref="ClassAbilityLoader"/> becomes the
    /// sole source), and adds a ClassAbilityLoader wired to ClassAbilitySet.asset. Unit_Enemy.prefab inherits this
    /// via its variant, so BOTH teams are covered. Idempotent + re-runnable. Per-unit class VARIETY is the follow-up.
    /// </summary>
    public static class ClassAbilityPrefabWiringTool
    {
        private const string PrefabPath = "Assets/Prefabs/Unit_Sample_Prefab.prefab";
        private const string ClassPath = "Assets/Data/Classes/Class_Monk.asset";
        private const string SetPath = "Assets/Data/Abilities/ClassAbilitySet.asset";

        [MenuItem("Tools/Build-To-Spec/Activate Class Abilities On Base Prefab")]
        public static void Wire()
        {
            var monk = AssetDatabase.LoadAssetAtPath<UnitClass_SO>(ClassPath);
            var set = AssetDatabase.LoadAssetAtPath<ClassAbilitySet_SO>(SetPath);
            if (monk == null || set == null)
            {
                Debug.LogError($"[ClassAbilityWiring] missing assets: monk={monk != null}, set={set != null}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var ac = root.GetComponentInChildren<AbilityController>(true);
                var sc = root.GetComponentInChildren<UnitStatusController>(true);
                if (ac == null || sc == null)
                {
                    Debug.LogError($"[ClassAbilityWiring] AbilityController={ac != null}, UnitStatusController={sc != null} on {PrefabPath}");
                    return;
                }

                const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
                typeof(UnitStatusController).GetField("unitClass", F)?.SetValue(sc, monk);
                typeof(AbilityController).GetField("abilities", F)?.SetValue(ac, new List<Ability_SO>());

                var loaderGo = ac.gameObject;
                var loader = loaderGo.GetComponent<ClassAbilityLoader>();
                if (loader == null) loader = loaderGo.AddComponent<ClassAbilityLoader>();
                typeof(ClassAbilityLoader).GetField("abilitySet", F)?.SetValue(loader, set);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[ClassAbilityWiring] base prefab → class={monk.ClassId}; ClassAbilityLoader on '{loaderGo.name}'; placeholder abilities cleared. Unit_Enemy inherits via the variant.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
