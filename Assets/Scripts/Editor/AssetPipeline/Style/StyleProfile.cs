using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// The single source of truth for the SHARED Grok art style. Every image prompt is
    /// composed as: <see cref="basePrompt"/> + the part's <see cref="PartTemplate"/> criteria +
    /// the concept text + <see cref="finishRules"/> + "Avoid: " + <see cref="avoidList"/>
    /// (see <see cref="StyleComposer"/>). Editing this asset restyles every FUTURE generation —
    /// nothing re-types the shared style by hand. Editor-only; stored under
    /// <c>Assets/Editor/AssetPipeline/Style/</c> (not shipped). Create one via
    /// Tools ▸ CapsuleWars ▸ Create Default Style + Templates (keep a single active profile).
    /// </summary>
    [CreateAssetMenu(fileName = "StyleProfile", menuName = "CapsuleWars/Asset Pipeline/Style Profile", order = 200)]
    public class StyleProfile : ScriptableObject
    {
        [Header("Shared style (the spine prepended to every prompt)")]
        [TextArea(4, 12)] public string basePrompt =
            "cartoony, soft, rounded, playful 3D character style like Rayman / classic Mickey-Mouse glove style; " +
            "chunky simplified forms, no realistic anatomy, toy-like; matte NEUTRAL GRAYSCALE only, zero saturation, " +
            "clearly separated material regions for later recoloring; single isolated object centered on a plain flat " +
            "white background, soft even lighting, no ground shadow, no text, no props; clean silhouette suitable for image-to-3D.";

        [Header("Finish rules (appended after the concept)")]
        [TextArea(2, 8)] public string finishRules =
            "Render one single isolated subject, fully in frame and centered, three-quarter view, no perspective " +
            "distortion, plain flat white background, soft even lighting, no ground shadow. Grayscale only, clean " +
            "readable silhouette, distinct material regions. No text, no watermark, no UI, no extra props.";

        [Header("Avoid list (folded into the prompt as 'Avoid: ...')")]
        [TextArea(2, 6)] public string avoidList =
            "color, saturation, realistic textures, busy background, scene, multiple objects, drop shadow, " +
            "text, watermark, logo, clutter, photorealism, gore.";

        [Header("API framing (xAI supported params — keep fixed for consistency)")]
        [Tooltip("xAI aspect_ratio, e.g. 1:1, 16:9, auto.")]
        public string aspectRatio = "1:1";
        [Tooltip("xAI resolution: 1k or 2k.")]
        public string resolution = "1k";

        [Header("Reference image (optional, EXPERIMENTAL)")]
        [Tooltip("If on AND a reference is set, generation routes through xAI's image-EDIT endpoint using this " +
                 "image as a style anchor. Note: edits modify the source image, so this is best-effort for a " +
                 "different part — leave OFF to rely on the shared prompt for consistency.")]
        public bool useReferenceImage = false;
        public Texture2D referenceImage;

        // NOTE: xAI's image API has NO seed parameter as of June 2026, so reproducible-by-seed is not
        // available. Consistency comes from this shared prompt + fixed aspectRatio/resolution + grayscale rules.

        [Header("Notes")]
        [TextArea(2, 5)] public string notes;
    }
}
