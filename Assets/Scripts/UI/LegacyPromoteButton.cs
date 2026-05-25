using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
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

            profile.Units.Add(new LegacyUnitDTO(id, displayName));
            LegacyStore.Save();

            if (rosterPanel != null) rosterPanel.Refresh();
        }
    }
}
