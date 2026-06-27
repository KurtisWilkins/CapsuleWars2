using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// One-click seeder for the shared <see cref="StyleProfile"/> + the per-part
    /// <see cref="PartTemplate"/>s under <c>Assets/Editor/AssetPipeline/Style/</c>.
    /// Re-running is safe: an existing StyleProfile or a template with the same
    /// <see cref="PartType"/> is left untouched (so your tuning is never overwritten).
    /// </summary>
    public static class StyleSetupTool
    {
        private const string StyleFolder = "Assets/Editor/AssetPipeline/Style";

        [MenuItem("Tools/CapsuleWars/Create Default Style + Templates")]
        public static void CreateDefaults()
        {
            AssetPipelineImporter.EnsureFolder(StyleFolder);

            // Single shared profile (field defaults already hold the cartoony spine).
            var profile = StyleComposer.ResolveProfile();
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<StyleProfile>();
                AssetDatabase.CreateAsset(profile, $"{StyleFolder}/StyleProfile.asset");
            }

            int created = 0;
            created += MaybeCreate(PartType.Head, "Head",
                "A single floating spherical HEAD ONLY — no body, neck, or shoulders. A smooth simplified cartoon sphere head (Rayman/Rabbids style) with minimal readable facial features; chunky and rounded; detached, meant to float above the torso.",
                "");
            created += MaybeCreate(PartType.Helmet, "Helmet",
                "A head-covering helmet ONLY — no head, face, neck, or body inside. Show the hollow opening where a head would go; chunky simplified rounded shell.",
                "");
            created += MaybeCreate(PartType.RightHand, "Right Hand",
                "A single RIGHT hand as a CLOSED rounded fist in a cartoon mitten / classic white-glove style; chunky, no individual fine fingers.",
                "Floating, cut cleanly at the wrist with a smooth rounded stump.");
            created += MaybeCreate(PartType.LeftHand, "Left Hand",
                "A single LEFT hand as a CLOSED rounded fist in a cartoon mitten / classic white-glove style; chunky; a mirror of the right hand.",
                "Floating, cut cleanly at the wrist with a smooth rounded stump.");
            created += MaybeCreate(PartType.Foot, "Foot",
                "A single foot or chunky rounded shoe/boot, cartoon proportions, simplified form.",
                "Floating, cut cleanly at the ankle with a smooth rounded stump.");
            created += MaybeCreate(PartType.Torso, "Torso",
                "A capsule-soldier torso / chest piece: chunky simplified rounded body form, clearly separated material regions.",
                "");
            created += MaybeCreate(PartType.Weapon, "Weapon",
                "A single handheld weapon prop with chunky simplified cartoon proportions and a clear grip; readable silhouette.",
                "");
            created += MaybeCreate(PartType.Armor, "Armor",
                "A single wearable armor piece (shoulder, back, arm, or leg guard): chunky simplified rounded plates.",
                "");
            created += MaybeCreate(PartType.Generic, "Generic",
                "A single game asset matching the request, in the shared cartoony style.",
                "");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
            Debug.Log($"[AssetPipeline] Style ready: StyleProfile + {created} new PartTemplate(s) under {StyleFolder} " +
                      "(existing ones left untouched). Edit the StyleProfile to restyle all future generations.");
        }

        private static int MaybeCreate(PartType type, string displayName, string criteria, string limbCut)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(PartTemplate)}"))
            {
                var existing = AssetDatabase.LoadAssetAtPath<PartTemplate>(AssetDatabase.GUIDToAssetPath(guid));
                if (existing != null && existing.partType == type) return 0;   // keep the user's tuning
            }

            var t = ScriptableObject.CreateInstance<PartTemplate>();
            t.partType = type;
            t.displayName = displayName;
            t.criteria = criteria;
            t.limbCut = limbCut;
            AssetDatabase.CreateAsset(t, $"{StyleFolder}/PartTemplate_{type}.asset");
            return 1;
        }
    }
}
