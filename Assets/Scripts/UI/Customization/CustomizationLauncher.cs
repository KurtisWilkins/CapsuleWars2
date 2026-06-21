using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// Between-rounds entry point for the per-unit armor customization screen
    /// (map scene). An "Open" button reveals a party picker built from the current
    /// run party (<see cref="RunSession"/>.Current.Party); clicking a unit opens the
    /// <see cref="CustomizationScreen"/> for that unit. With no active run the picker
    /// is simply empty. Build the picker panel + buttons in-scene and wire the refs;
    /// the unit button prefab can reuse EquipButton.prefab (a button with a child Text).
    /// </summary>
    public class CustomizationLauncher : MonoBehaviour
    {
        [SerializeField] private Button openButton;        // "Customize" button in the map HUD
        [SerializeField] private GameObject pickerRoot;    // panel holding the generated party buttons
        [SerializeField] private Transform partyListRoot;  // parent for the generated unit buttons
        [SerializeField] private Button unitButtonPrefab;  // button with a child Text label
        [SerializeField] private Button closeButton;
        [SerializeField] private CustomizationScreen screen;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (pickerRoot != null) pickerRoot.SetActive(false);
        }

        /// <summary>Show the party picker, rebuilt from the current run party.</summary>
        public void Open()
        {
            if (pickerRoot != null) pickerRoot.SetActive(true);
            BuildPartyList();
        }

        public void Close()
        {
            if (pickerRoot != null) pickerRoot.SetActive(false);
        }

        private void BuildPartyList()
        {
            if (partyListRoot == null || unitButtonPrefab == null) return;

            for (int i = partyListRoot.childCount - 1; i >= 0; i--)
                Destroy(partyListRoot.GetChild(i).gameObject);

            if (!RunSession.IsActive) return;

            var party = RunSession.Current.Party;
            for (int i = 0; i < party.Count; i++)
            {
                var unit = party[i];
                if (unit == null) continue;

                var btn = Instantiate(unitButtonPrefab, partyListRoot);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = string.IsNullOrEmpty(unit.DisplayName) ? unit.Id : unit.DisplayName;

                string id = unit.Id;   // capture per-iteration for the closure
                btn.onClick.AddListener(() =>
                {
                    Close();
                    if (screen != null) screen.Show(id);
                });
            }
        }
    }
}
