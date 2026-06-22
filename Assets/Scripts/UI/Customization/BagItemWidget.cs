using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// One owned item in the paper-doll bag — either a GEAR item (<see cref="Equipment_SO"/>,
    /// routed to its <see cref="EquipmentSlot"/> via UnitStatusController) or a BODY part
    /// (<see cref="BodyPart_SO"/>, routed to its <see cref="PartSlot"/> via UnitCustomization).
    ///
    /// Interaction (paper-doll customization, extends ADR-012): TAP auto-equips to the item's own
    /// slot (the slot is read from the item, never chosen by the player); DRAG shows a ghost and the
    /// drop is resolved by the slot/drop-zone under the pointer. Self-builds its visuals so the bag
    /// can be generated at runtime with no authored prefab. uGUI suppresses the click after a real
    /// drag (eligibleForClick), so tap and drag don't both fire.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class BagItemWidget : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CustomizationScreen screen;
        private Equipment_SO gear;
        private BodyPart_SO part;

        private CanvasGroup cg;
        private Image bg;
        private Image icon;
        private Text label;
        private Color normalColor = new Color(0.22f, 0.26f, 0.34f, 1f);
        private Color equippedColor = new Color(0.20f, 0.55f, 0.30f, 1f);

        public bool IsGear => gear != null;
        public Equipment_SO Gear => gear;
        public BodyPart_SO Part => part;
        public Sprite Icon => gear != null ? gear.Icon : null;
        public string Label { get; private set; }

        public void ConfigureGear(CustomizationScreen owner, Equipment_SO item, string text)
        {
            screen = owner; gear = item; part = null; Label = text;
            Build(item != null ? item.Icon : null, text);
        }

        public void ConfigureBody(CustomizationScreen owner, BodyPart_SO bodyPart, string text)
        {
            screen = owner; part = bodyPart; gear = null; Label = text;
            Build(null, text);
        }

        public void SetThemeColors(Color normal, Color equipped)
        {
            normalColor = normal; equippedColor = equipped;
        }

        private void Build(Sprite sprite, string text)
        {
            cg = GetComponent<CanvasGroup>();
            bg = GetComponent<Image>();
            bg.color = normalColor;

            icon = MakeChild<Image>("Icon", new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.95f));
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            if (sprite != null) icon.sprite = sprite; else icon.enabled = false;

            label = MakeChild<Text>("Label", new Vector2(0f, 0f),
                                    sprite != null ? new Vector2(1f, 0.30f) : new Vector2(1f, 1f));
            label.raycastTarget = false;
            label.alignment = sprite != null ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter;
            label.fontSize = sprite != null ? 10 : 11;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.text = text;
        }

        private T MakeChild<T>(string n, Vector2 aMin, Vector2 aMax) where T : Component
        {
            var go = new GameObject(n, typeof(RectTransform), typeof(T));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go.GetComponent<T>();
        }

        /// <summary>Highlight when this item is currently equipped (so the bag mirrors the doll).</summary>
        public void SetEquipped(bool equipped)
        {
            if (bg != null) bg.color = equipped ? equippedColor : normalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (screen == null) return;
            if (IsGear) screen.RouteEquipGear(gear);
            else screen.RouteEquipPart(part);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (screen == null) return;
            // Stop blocking raycasts so the drop target UNDER the pointer is hit, not this item.
            if (cg != null) cg.blocksRaycasts = false;
            screen.BeginDragVisual(Icon, Label, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (screen != null) screen.UpdateDragVisual(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (cg != null) cg.blocksRaycasts = true;
            if (screen != null) screen.EndDragVisual();
        }
    }
}
