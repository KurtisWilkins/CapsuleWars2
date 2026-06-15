using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// One-click "promote a unit to legacy" button. Reads the
    /// configured Id + DisplayName and adds an entry to the legacy
    /// profile if one doesn't already exist with that Id.
    /// Saves to disk on add. Refreshes any sibling LegacyRosterPanel.
    /// </summary>
    public class LegacyPromoteButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private InputField idField;
        [SerializeField] private InputField nameField;
        [SerializeField] private LegacyRosterPanel rosterPanel;

        [Tooltip("Fallback Id if no InputField is wired. Useful for a quick demo button.")]
        [SerializeField] private string defaultId = "player_01";

        [Tooltip("Fallback name if no InputField is wired.")]
        [SerializeField] private string defaultName = "Hero";

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnPromote);
            }
        }

        private void OnPromote()
        {
            string id = idField != null && !string.IsNullOrWhiteSpace(idField.text) ? idField.text : defaultId;
            string displayName = nameField != null && !string.IsNullOrWhiteSpace(nameField.text) ? nameField.text : defaultName;
            if (string.IsNullOrWhiteSpace(id)) return;

            var profile = LegacyStore.Current;
            if (profile == null) return;
            if (profile.FindById(id) != null) return; // already promoted

            // Capture the live unit's UnitDefinition (visuals) so the draft-into-run
            // flow can reconstruct it later. Falls back to null (identity-only) when
            // no matching scene unit is found.
            string definitionId = null;
            var root = FindUnitRootById(id);
            if (root != null) definitionId = UnitFactory.FromUnit(root)?.UnitDefinitionId;

            profile.Units.Add(new LegacyUnitDTO(id, displayName, definitionId));
            LegacyStore.Save();

            if (rosterPanel != null) rosterPanel.Refresh();
        }

        private static UnitRoot FindUnitRootById(string id)
        {
            var roots = Object.FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].UnitId == id) return roots[i];
            }
            return null;
        }
    }
}
