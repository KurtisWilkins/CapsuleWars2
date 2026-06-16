using UnityEngine;

namespace CapsuleWars.UI.Theme
{
    /// <summary>
    /// Shared color palette for the game's UI. One asset defines the whole
    /// game's look; <see cref="UIThemeApplier"/> components read it to recolor
    /// panels, text, and buttons consistently. Edit this asset to retheme every
    /// panel at once.
    /// </summary>
    [CreateAssetMenu(fileName = "UIThemePalette", menuName = "CapsuleWars/UI Theme Palette", order = 3)]
    public class UIThemePalette : ScriptableObject
    {
        [Header("Panels")]
        [Tooltip("Background color applied to a panel root's own Image.")]
        public Color panelBackground = new Color(0.06f, 0.07f, 0.10f, 0.96f);

        [Header("Text")]
        [Tooltip("Color for body/label text that is not inside a button.")]
        public Color primaryText = new Color(0.90f, 0.92f, 0.96f, 1f);

        [Header("Buttons")]
        [Tooltip("Background color for button graphics (the button's target Image).")]
        public Color buttonNormal = new Color(0.22f, 0.26f, 0.34f, 1f);

        [Tooltip("Text color for labels inside buttons.")]
        public Color buttonText = new Color(0.92f, 0.94f, 0.98f, 1f);

        [Tooltip("Accent color (primary actions / highlights). Not auto-applied; use where a button should stand out.")]
        public Color accent = new Color(0.20f, 0.55f, 0.30f, 1f);
    }
}
