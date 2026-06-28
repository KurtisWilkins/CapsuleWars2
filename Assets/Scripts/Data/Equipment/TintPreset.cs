using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// A reusable, saveable race/faction tint as a project asset — the REGION-TINT model (ADR-040, supersedes the
    /// earlier shadow/mid/high ramp). Three player-facing color slots — <see cref="primaryColor"/> /
    /// <see cref="secondaryColor"/> / <see cref="accentColor"/> — assigned across a grayscale part by a region
    /// <see cref="regionMask"/> (white = secondary/marking region, black = primary/base; null mask = solid primary).
    /// The grayscale luminance shades WITHIN each region. A pattern (stripes / spots / rosettes) is simply the
    /// secondary region of a mask, so markings are plain data — not a special case and not deferred.
    ///
    /// This asset is pure DATA + a mask reference; live rendering is consumed by the (pending) region-tint shader
    /// milestone. Color is NEVER baked into geometry — the grayscale library stays the single source of truth.
    /// </summary>
    [CreateAssetMenu(fileName = "TintPreset", menuName = "CapsuleWars/Tint Preset", order = 20)]
    public class TintPreset : ScriptableObject
    {
        [Tooltip("Base/body color — fills the mask's BLACK (primary) region, luminance-shaded.")]
        [SerializeField] private Color primaryColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("Marking/secondary color — fills the mask's WHITE region (stripes/spots/rosettes).")]
        [SerializeField] private Color secondaryColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("Accent color — small highlight regions (eyes, claws, trim) per the shader.")]
        [SerializeField] private Color accentColor = Color.white;

        [Tooltip("Grayscale region mask: white = secondary/marking region, black = primary/base. Null = solid primary.")]
        [SerializeField] private Texture2D regionMask;

        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public Color AccentColor => accentColor;
        public Texture2D RegionMask => regionMask;
        public bool HasMask => regionMask != null;

        public void SetColors(Color primary, Color secondary, Color accent)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            accentColor = accent;
        }

        public void SetMask(Texture2D mask) => regionMask = mask;
    }
}
