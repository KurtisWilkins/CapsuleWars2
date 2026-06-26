using UnityEngine;

namespace CapsuleWars.Data.Arena
{
    /// <summary>
    /// A floor/biome theme for a battle encounter. For now it just selects which <see cref="ThemeBlockSet"/>
    /// the arena builds from, so forest / plains / volcanic / ship / cave are different block sets on different
    /// themes (no code change). Skybox/lighting/props hang off this later. Which theme a run-floor uses is a
    /// Slice C (encounter builder) concern; for now a theme is assigned directly on the ArenaBuilder.
    /// </summary>
    [CreateAssetMenu(menuName = "CapsuleWars/Arena/Encounter Theme", fileName = "EncounterTheme")]
    public class EncounterTheme : ScriptableObject
    {
        [SerializeField] private string themeId = "grass";
        [SerializeField] private string displayName = "Grasslands";
        [Tooltip("The block set this theme builds the board from.")]
        [SerializeField] private ThemeBlockSet blockSet;

        public string ThemeId => themeId;
        public string DisplayName => displayName;
        public ThemeBlockSet BlockSet => blockSet;
    }
}
