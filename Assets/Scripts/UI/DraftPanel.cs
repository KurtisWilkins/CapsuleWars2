using System;
using System.Collections.Generic;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Run-start draft screen. Lists the persistent legacy roster across a
    /// fixed set of inspector-wired slots (same pattern as ShopPanel), lets the
    /// player toggle up to <see cref="maxPartySize"/> units, then builds the run
    /// party and hands it to <see cref="RunController.StartRunWithParty"/>.
    ///
    /// Selecting zero units is allowed: the run then starts with an empty party
    /// and the battle scene falls back to its scene-placed player units — so the
    /// flow works even before any unit has been promoted to legacy.
    /// </summary>
    public class DraftPanel : MonoBehaviour
    {
        [Serializable]
        public class DraftSlot
        {
            public Button selectButton;
            public Text labelText;
        }

        [SerializeField] private DraftSlot[] slots;
        [SerializeField] private Button startButton;
        [SerializeField] private Text headerText;

        [Tooltip("Maximum number of units the player can draft into a run.")]
        [SerializeField, Min(1)] private int maxPartySize = 3;

        private readonly List<LegacyUnitDTO> roster = new();
        private readonly HashSet<int> selected = new();

        private void OnEnable()
        {
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    int idx = i;
                    var slot = slots[i];
                    if (slot != null && slot.selectButton != null)
                    {
                        slot.selectButton.onClick.RemoveAllListeners();
                        slot.selectButton.onClick.AddListener(() => ToggleSelect(idx));
                    }
                }
            }
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(OnStart);
            }

            ReloadRoster();
            Refresh();
        }

        private void ReloadRoster()
        {
            roster.Clear();
            selected.Clear();
            var profile = LegacyStore.Current;
            if (profile?.Units != null)
            {
                for (int i = 0; i < profile.Units.Count; i++)
                    if (profile.Units[i] != null) roster.Add(profile.Units[i]);
            }
        }

        private void ToggleSelect(int rosterIndex)
        {
            if (rosterIndex < 0 || rosterIndex >= roster.Count) return;

            if (!selected.Remove(rosterIndex))
            {
                if (selected.Count >= maxPartySize) return; // at cap
                selected.Add(rosterIndex);
            }
            Refresh();
        }

        private void Refresh()
        {
            if (headerText != null)
                headerText.text = $"DRAFT YOUR PARTY ({selected.Count}/{maxPartySize})";

            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    if (slot == null) continue;

                    bool hasUnit = i < roster.Count;
                    if (slot.selectButton != null)
                    {
                        slot.selectButton.gameObject.SetActive(hasUnit);
                        // Allow deselect even at cap; block selecting new ones past the cap.
                        slot.selectButton.interactable = hasUnit && (selected.Contains(i) || selected.Count < maxPartySize);
                    }
                    if (slot.labelText != null && hasUnit)
                    {
                        var u = roster[i];
                        string mark = selected.Contains(i) ? "[x] " : "[ ] ";
                        slot.labelText.text = $"{mark}{u.DisplayName} [{u.Id}]";
                    }
                }
            }

            if (startButton != null) startButton.interactable = true; // empty party allowed (fallback)
        }

        private void OnStart()
        {
            var party = new List<UnitDTO>(selected.Count);
            foreach (int idx in selected)
            {
                if (idx >= 0 && idx < roster.Count)
                {
                    var dto = UnitDTO.FromLegacy(roster[idx]);
                    if (dto != null) party.Add(dto);
                }
            }

            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.StartRunWithParty(party);
        }
    }
}
