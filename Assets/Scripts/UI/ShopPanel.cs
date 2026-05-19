using CapsuleWars.Data.Equipment;
using CapsuleWars.Run;
using CapsuleWars.Units.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Minimal shop panel: up to 3 buyable items at fixed prices. Items
    /// are wired in the inspector; clicking buy applies the item to the
    /// selected unit's chest slot (M7 simplification — full equipment
    /// management UI is M9+).
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [System.Serializable]
        public class ShopSlot
        {
            public Equipment_SO item;
            public int price;
            public Button buyButton;
            public Text labelText;
            public Text priceText;
        }

        [SerializeField] private ShopSlot[] slots;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text goldText;

        [Tooltip("Target unit prefab tag. Buying applies the item to the first matching unit found in the scene at battle setup time. Shop persists choice via PlayerPrefs for M7.")]
        [SerializeField] private string buyTargetUnitId = "player_01";

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    int idx = i;
                    var slot = slots[i];
                    if (slot.buyButton != null)
                    {
                        slot.buyButton.onClick.RemoveAllListeners();
                        slot.buyButton.onClick.AddListener(() => OnBuy(idx));
                    }
                }
            }
            Refresh();
        }

        private void Refresh()
        {
            var state = RunSession.Current;
            int gold = state != null ? state.Gold : 0;
            if (goldText != null) goldText.text = $"Gold: {gold}";

            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.labelText != null) slot.labelText.text = slot.item != null ? slot.item.name : "(empty)";
                if (slot.priceText != null) slot.priceText.text = $"{slot.price}g";
                if (slot.buyButton != null) slot.buyButton.interactable = slot.item != null && gold >= slot.price;
            }
        }

        private void OnBuy(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            var slot = slots[slotIndex];
            if (slot.item == null) return;

            var state = RunSession.Current;
            if (state == null || !state.SpendGold(slot.price)) return;

            // Apply to the matching unit in any loaded scene. For M7 the unit
            // lives in the Battle scene which isn't loaded here, so the
            // simplest behavior is: stash the purchase in a side dictionary
            // and apply it when the battle scene loads. M9+ will replace this
            // with the proper pre-battle deployment UI.
            PurchasedItems.Add(buyTargetUnitId, slot.item);
            slot.item = null; // sold
            Refresh();
        }

        private void OnContinue()
        {
            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.CompleteCurrentNode();
        }
    }
}
