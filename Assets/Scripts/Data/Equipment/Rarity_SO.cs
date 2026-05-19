using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Rarity tier for equipment. Drives a flat stat multiplier on top
    /// of the equipment's authored stat buffs — same item at higher
    /// rarity hits harder.
    /// </summary>
    [CreateAssetMenu(fileName = "Rarity", menuName = "CapsuleWars/Equipment/Rarity", order = 90)]
    public class Rarity_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and Database lookup (e.g. 'common', 'epic').")]
        [SerializeField] private string rarityId;

        [Tooltip("I2 term key for display name (e.g. Rarity.Epic.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("Tier number 1..5. Higher = rarer.")]
        [SerializeField, Range(1, 5)] private int tier = 1;

        [Tooltip("UI accent color used to outline equipment cards, ability icons, etc.")]
        [SerializeField] private Color uiColor = Color.white;

        [Tooltip("Multiplier applied to all stat buffs on items of this rarity. Default 1 = no scaling.")]
        [SerializeField, Min(0.01f)] private float statMultiplier = 1.0f;

        public string RarityId => rarityId;
        public string NameTermKey => nameTermKey;
        public int Tier => tier;
        public Color UiColor => uiColor;
        public float StatMultiplier => statMultiplier;
    }
}
