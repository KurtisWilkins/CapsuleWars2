using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// A named "race coat": the body-part set plus the procedural-coat parameters (primary / marking colour,
    /// pattern, eye colour) that turn the shared felid base into a specific big-cat race (Tigerfolk,
    /// Pantherfolk, Lionfolk…). Authored data the Unit Builder loads in one click; the same data can drive a
    /// spawnable race later. Pattern: 0 = solid, 1 = stripes, 2 = spots (matches the ProceduralPattern shader).
    /// </summary>
    [CreateAssetMenu(fileName = "Coat", menuName = "CapsuleWars/Coat Preset", order = 12)]
    public class CoatPreset_SO : ScriptableObject
    {
        [Tooltip("Stable race id, e.g. Tigerfolk.")]
        [SerializeField] private string raceId;
        [Tooltip("Display name shown on the preset button (falls back to raceId).")]
        [SerializeField] private string displayName;
        [Tooltip("The body parts that make up this race (felid base set, or base + a race-specific head).")]
        [SerializeField] private List<BodyPart_SO> parts = new List<BodyPart_SO>();

        [Header("Coat")]
        [Tooltip("0 = solid, 1 = stripes, 2 = spots.")]
        [SerializeField] private int pattern;
        [Tooltip("Pattern scale; higher = finer/denser (cheetah high, jaguar low).")]
        [SerializeField] private float patternFrequency = 9f;
        [SerializeField] private Color primary = new Color(0.80f, 0.60f, 0.40f);
        [Tooltip("Marking colour (the dark of stripes/spots).")]
        [SerializeField] private Color secondary = new Color(0.10f, 0.08f, 0.06f);
        [SerializeField] private Color eyeColor = new Color(0.93f, 0.76f, 0.18f);

        public string RaceId => raceId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? raceId : displayName;
        public IReadOnlyList<BodyPart_SO> Parts => parts;
        public int Pattern => pattern;
        public float PatternFrequency => patternFrequency;
        public Color Primary => primary;
        public Color Secondary => secondary;
        public Color EyeColor => eyeColor;
    }
}
