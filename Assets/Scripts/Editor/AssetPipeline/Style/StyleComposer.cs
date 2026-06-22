using System.Text;
using CapsuleWars.Core;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// The single place that composes a Grok image prompt from the shared
    /// <see cref="StyleProfile"/> + the matching <see cref="PartTemplate"/> + the request's
    /// concept. Because the style is referenced (not copied), editing the profile restyles
    /// every future generation. Falls back to <see cref="PromptTemplates"/> when no profile
    /// exists yet (run the seeder to create one). Editor-only.
    /// </summary>
    public static class StyleComposer
    {
        /// <summary>The single active StyleProfile, or null if none has been created.</summary>
        public static StyleProfile ResolveProfile()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(StyleProfile)}");
            if (guids.Length == 0) return null;
            if (guids.Length > 1)
                Debug.LogWarning($"[AssetPipeline] {guids.Length} StyleProfile assets found — using the first. " +
                                 "Keep a single active profile so the style stays one source of truth.");
            return AssetDatabase.LoadAssetAtPath<StyleProfile>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Map a request's category + slot to the part type a template targets.</summary>
        public static PartType PartTypeFor(AssetCategory category, int slot)
        {
            switch (category)
            {
                case AssetCategory.Weapon:
                    return PartType.Weapon;

                case AssetCategory.EquipmentArmor:
                    switch ((EquipmentSlot)slot)
                    {
                        case EquipmentSlot.Helmet: return PartType.Helmet;
                        case EquipmentSlot.Chest: return PartType.Torso;
                        case EquipmentSlot.RightHand: return PartType.RightHand;
                        case EquipmentSlot.LeftHand: return PartType.LeftHand;
                        default: return PartType.Armor;   // Shoulders / Back / Arms / Legs
                    }

                case AssetCategory.BodyPart:
                    switch ((PartSlot)slot)
                    {
                        case PartSlot.HeadProp: return PartType.Helmet;
                        case PartSlot.Body: return PartType.Torso;
                        case PartSlot.RightHand: return PartType.RightHand;
                        case PartSlot.LeftHand: return PartType.LeftHand;
                        case PartSlot.LeftFoot:
                        case PartSlot.RightFoot: return PartType.Foot;
                        default: return PartType.Generic;
                    }

                default:
                    return PartType.Generic;
            }
        }

        /// <summary>The PartTemplate matching the request (exact part type, else the Generic one, else null).</summary>
        public static PartTemplate ResolveTemplate(AssetRequest r)
        {
            PartType want = PartTypeFor(r.category, r.targetSlot);
            PartTemplate generic = null;
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(PartTemplate)}"))
            {
                var t = AssetDatabase.LoadAssetAtPath<PartTemplate>(AssetDatabase.GUIDToAssetPath(guid));
                if (t == null) continue;
                if (t.partType == want) return t;
                if (t.partType == PartType.Generic) generic = t;
            }
            return generic;
        }

        /// <summary>base + part criteria + concept + finish + avoid (the composed Grok prompt).</summary>
        public static string ComposeImagePrompt(AssetRequest r)
        {
            var profile = ResolveProfile();
            if (profile == null) return PromptTemplates.GrokImagePrompt(r);   // fallback until a profile is seeded

            var template = ResolveTemplate(r);
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(profile.basePrompt)) sb.AppendLine(profile.basePrompt.Trim());
            if (template != null)
            {
                if (!string.IsNullOrWhiteSpace(template.criteria)) sb.AppendLine(template.criteria.Trim());
                if (!string.IsNullOrWhiteSpace(template.limbCut)) sb.AppendLine(template.limbCut.Trim());
            }
            string concept = ConceptText(r);
            if (!string.IsNullOrWhiteSpace(concept)) sb.AppendLine("Concept: " + concept);
            if (!string.IsNullOrWhiteSpace(profile.finishRules)) sb.AppendLine(profile.finishRules.Trim());
            if (!string.IsNullOrWhiteSpace(profile.avoidList)) sb.AppendLine("Avoid: " + profile.avoidList.Trim());
            return sb.ToString();
        }

        /// <summary>The Meshy image-to-3D prompt, ready for the next stage.</summary>
        public static string ComposeMeshyPrompt(AssetRequest r) => PromptTemplates.MeshyPrompt(r);

        private static string ConceptText(AssetRequest r)
        {
            var c = r.ChosenConcept;
            string s = c != null ? $"{c.name} — {c.visualDesc}" : r.requestText;
            return string.IsNullOrEmpty(s) ? "" : s.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
