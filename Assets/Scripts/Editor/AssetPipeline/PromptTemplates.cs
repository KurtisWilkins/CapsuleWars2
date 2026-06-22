using System.Text;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Builds the prompts for each pipeline stage with the project's art direction
    /// baked in, so generated assets stay visually coherent (ADR-001 + Docs/16).
    /// The designer fills in only the variant text (the request / chosen concept);
    /// the full prompt is constructed here. Pure string assembly — no I/O.
    /// </summary>
    public static class PromptTemplates
    {
        /// <summary>Locked style sentence prepended to image/3D prompts.</summary>
        public const string StyleLine =
            "Rayman-style low-poly 3D game asset, matching the AssetHunts \"Capsule\" soldier kit: " +
            "smooth rounded simple forms, clean flat-shaded look, bold readable silhouette, soft neutral palette, " +
            "minimal surface detail, friendly toy-like proportions.";

        private static string CategoryNoun(AssetCategory c) => c switch
        {
            AssetCategory.Weapon => "handheld weapon prop",
            AssetCategory.EquipmentArmor => "wearable armor / equipment piece",
            AssetCategory.BodyPart => "capsule-soldier body part",
            _ => "single game prop"
        };

        /// <summary>
        /// Internal brief for the concept step (what Claude produces and writes back
        /// into the request). Also handy to paste into a chat if working manually.
        /// </summary>
        public static string ConceptBrief(AssetRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Design 3 distinct concept options for a {CategoryNoun(r.category)} for CapsuleWars.");
            sb.AppendLine($"Request: \"{Clean(r.requestText)}\"");
            sb.AppendLine();
            sb.AppendLine($"Art direction (must fit): {StyleLine}");
            sb.AppendLine();
            sb.AppendLine("For each option give: a short Name, a Visual description (shape/materials/colors,");
            sb.AppendLine("one clean asset — no scene), and a one-line note on how it fits the capsule-soldier style.");
            sb.AppendLine("Keep options visually distinct from each other.");
            return sb.ToString();
        }

        /// <summary>
        /// Grok image-gen prompt for the chosen concept: one clean isolated asset on a
        /// plain background — tuned to feed cleanly into Meshy's image-to-3D.
        /// </summary>
        public static string GrokImagePrompt(AssetRequest r)
        {
            var concept = r.ChosenConcept;
            string subject = concept != null
                ? $"{concept.name} — {Clean(concept.visualDesc)}"
                : Clean(r.requestText);

            var sb = new StringBuilder();
            sb.AppendLine($"A single {CategoryNoun(r.category)}: {subject}.");
            sb.AppendLine(StyleLine);
            sb.AppendLine(
                "Composition for image-to-3D: ONE object only, fully in frame and centered, " +
                "three-quarter view, no perspective distortion, even soft studio lighting, no harsh shadows, " +
                "plain solid light-grey background, no scene, no ground plane, no other objects, " +
                "no text, no watermark, no UI. High clarity, consistent readable shapes.");
            return sb.ToString();
        }

        /// <summary>
        /// Meshy image-to-3D request text + the settings that import cleanly to Unity.
        /// </summary>
        public static string MeshyPrompt(AssetRequest r)
        {
            var concept = r.ChosenConcept;
            string subject = concept != null ? concept.name : (string.IsNullOrEmpty(r.title) ? "asset" : r.title);

            var sb = new StringBuilder();
            sb.AppendLine($"Image to 3D: convert the reference image into a game-ready {CategoryNoun(r.category)} (\"{subject}\").");
            sb.AppendLine(StyleLine);
            sb.AppendLine();
            sb.AppendLine("Targets / settings:");
            sb.AppendLine("- Low-poly, clean topology, single watertight mesh; symmetry where the shape is symmetric.");
            sb.AppendLine("- Simple/flat materials (no heavy PBR) to match the kit's look.");
            sb.AppendLine("- Export: FBX, Y-up, real-world scale to roughly match the AssetHunts Capsule parts (a held weapon ~0.3-0.8 units).");
            sb.AppendLine("- No rig / no animation (static prop).");
            return sb.ToString();
        }

        private static string Clean(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
