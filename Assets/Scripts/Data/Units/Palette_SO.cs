using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Three-color palette applied to a unit at spawn. Slot mounts opt in
    /// via their PaletteRole (Body / Limbs / Accent / None).
    /// </summary>
    [CreateAssetMenu(fileName = "Palette", menuName = "CapsuleWars/Palette", order = 11)]
    public class Palette_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and the Database lookup.")]
        [SerializeField] private string paletteId;

        [Tooltip("I2 term key for the palette's display name (e.g. Palette.Sunset.Name).")]
        [SerializeField] private string nameTermKey;

        [SerializeField] private Color body = Color.white;
        [SerializeField] private Color limbs = Color.white;
        [SerializeField] private Color accent = Color.white;

        public string PaletteId => paletteId;
        public string NameTermKey => nameTermKey;
        public Color Body => body;
        public Color Limbs => limbs;
        public Color Accent => accent;
    }
}
