using System;
using System.Collections.Generic;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Customization / unlock store (M9, Docs/12_RoguelikeRun.md §82-89). Shows the
    /// player's unlock-point balance and the catalog parts; spending points unlocks
    /// a part, after which it becomes available to the random unit generator and
    /// customization picker. Same fixed-slot pattern as the other panels; closes
    /// itself (an external PanelOpenButton opens it).
    /// </summary>
    public class CustomizationPanel : MonoBehaviour
    {
        [Serializable]
        public class Slot
        {
            public Button button;
            public Text label;
        }

        [SerializeField] private PartCatalog_SO catalog;
        [SerializeField] private Text pointsText;
        [SerializeField] private Slot[] slots;
        [SerializeField] private Button closeButton;

        private readonly List<string> partIds = new List<string>();

        private void OnEnable()
        {
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    int idx = i;
                    if (slots[i]?.button != null)
                    {
                        slots[i].button.onClick.RemoveAllListeners();
                        slots[i].button.onClick.AddListener(() => OnUnlock(idx));
                    }
                }
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            }

            BuildList();
            Refresh();
        }

        private void BuildList()
        {
            partIds.Clear();
            if (catalog == null) return;

            // Make sure starter parts are owned before listing (idempotent).
            CustomizationUnlocks.SeedStarters(catalog, LegacyStore.Current?.PlayerProfile);

            foreach (var entry in catalog.Parts)
                if (entry?.part != null) partIds.Add(entry.part.PartId);
        }

        private void OnUnlock(int index)
        {
            if (index < 0 || index >= partIds.Count) return;
            var profile = LegacyStore.Current?.PlayerProfile;
            if (profile == null) return;

            if (CustomizationUnlocks.TryUnlockPart(catalog, profile, partIds[index]))
                LegacyStore.Save();
            Refresh();
        }

        private void Refresh()
        {
            var profile = LegacyStore.Current?.PlayerProfile;
            if (pointsText != null)
                pointsText.text = $"UNLOCK POINTS: {(profile != null ? profile.UnlockPoints : 0)}";

            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                bool has = i < partIds.Count;
                if (slot.button != null) slot.button.gameObject.SetActive(has);
                if (!has) continue;

                string id = partIds[i];
                int cost = catalog != null ? catalog.GetPartCost(id) : -1;
                bool owned = profile != null && profile.HasPart(id);
                bool affordable = profile != null && cost >= 0 && profile.UnlockPoints >= cost;

                if (slot.label != null)
                    slot.label.text = owned ? $"{id}   —   OWNED" : $"{id}   —   {cost} pts";
                if (slot.button != null)
                    slot.button.interactable = !owned && affordable;
            }
        }
    }
}
