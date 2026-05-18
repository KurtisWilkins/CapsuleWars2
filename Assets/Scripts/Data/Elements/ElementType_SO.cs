using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Elements
{
    /// <summary>
    /// One of the 15 element types. Each belongs to one of 5 families
    /// (Fire / Water / Earth / Spirit / Air) — the family drives matchup
    /// math via ElementChart_SO. Sub-types within a family share matchups
    /// but differ in damage profile (post-MVP).
    /// </summary>
    [CreateAssetMenu(fileName = "ElementType", menuName = "CapsuleWars/Elements/Element Type", order = 70)]
    public class ElementType_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and Database lookup (e.g. 'flame', 'frost').")]
        [SerializeField] private string elementId;

        [Tooltip("I2 term key for the element's display name (e.g. Element.Flame.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("I2 term key for the element's description.")]
        [SerializeField] private string descTermKey;

        [Tooltip("Family this element belongs to. Drives matchup multipliers via ElementChart_SO.")]
        [SerializeField] private ElementFamily family;

        [Tooltip("UI accent color for this element. Damage numbers, ability icons, status overlays inherit from this.")]
        [SerializeField] private Color uiColor = Color.white;

        [Tooltip("Icon used in tooltips and UI.")]
        [SerializeField] private Sprite icon;

        public string ElementId => elementId;
        public string NameTermKey => nameTermKey;
        public string DescTermKey => descTermKey;
        public ElementFamily Family => family;
        public Color UiColor => uiColor;
        public Sprite Icon => icon;
    }
}
