using System.Collections;
using CapsuleWars.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// One framed slot on the paper-doll. Either a GEAR slot (an <see cref="EquipmentSlot"/>, backed
    /// by UnitStatusController) or a BODY slot (a <see cref="PartSlot"/>, backed by UnitCustomization).
    /// Shows the equipped gear's icon, or the part/gear name, or a dim placeholder when empty.
    ///
    /// Interaction (paper-doll customization, extends ADR-012): TAP a filled slot → unequip; DROP a
    /// dragged bag item → equip if the item's slot matches this slot, else flash-reject. Self-builds
    /// its child visuals so it can be generated at runtime straight from the slot enums.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PaperDollSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        private CustomizationScreen screen;
        private bool isGear;
        private EquipmentSlot gearSlot;
        private PartSlot bodySlot;

        private Image frame;
        private Image icon;
        private Text label;
        private Text slotName;
        private bool filled;
        private Color emptyColor = new Color(0.16f, 0.18f, 0.24f, 1f);
        private Color filledColor = new Color(0.20f, 0.55f, 0.30f, 1f);

        public bool IsGear => isGear;
        public EquipmentSlot GearSlot => gearSlot;
        public PartSlot BodySlot => bodySlot;

        public void ConfigureGear(CustomizationScreen owner, EquipmentSlot slot)
        {
            screen = owner; isGear = true; gearSlot = slot; Build(slot.ToString());
        }

        public void ConfigureBody(CustomizationScreen owner, PartSlot slot)
        {
            screen = owner; isGear = false; bodySlot = slot; Build(slot.ToString());
        }

        public void SetThemeColors(Color empty, Color filledTint)
        {
            emptyColor = empty; filledColor = filledTint; ApplyTint();
        }

        private void Build(string name)
        {
            frame = GetComponent<Image>();

            icon = MakeChild<Image>("Icon", new Vector2(0.14f, 0.30f), new Vector2(0.86f, 0.94f));
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            icon.enabled = false;

            // The equipped item's display name (gear/part) — shown when there's no icon.
            label = MakeChild<Text>("Label", new Vector2(0.04f, 0.26f), new Vector2(0.96f, 0.96f));
            ConfigText(label, "", TextAnchor.MiddleCenter, 11);
            label.enabled = false;

            // The slot's own name, always shown along the bottom edge.
            slotName = MakeChild<Text>("SlotName", new Vector2(0f, 0f), new Vector2(1f, 0.26f));
            ConfigText(slotName, name, TextAnchor.LowerCenter, 9);

            ApplyTint();
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

        private static void ConfigText(Text t, string s, TextAnchor anchor, int size)
        {
            t.raycastTarget = false;
            t.alignment = anchor;
            t.fontSize = size;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.text = s;
        }

        /// <summary>Show an equipped gear item (icon + fallback name) or clear to the placeholder.</summary>
        public void SetGear(Sprite sprite, string text, bool isFilled)
        {
            filled = isFilled;
            if (isFilled && sprite != null)
            {
                icon.sprite = sprite; icon.enabled = true; label.enabled = false;
            }
            else
            {
                icon.enabled = false;
                label.text = isFilled ? text : "";
                label.enabled = isFilled;
            }
            ApplyTint();
        }

        /// <summary>Show an equipped body part (icon + fallback name) or clear to the placeholder.</summary>
        public void SetBody(Sprite sprite, string text, bool isFilled)
        {
            filled = isFilled;
            if (isFilled && sprite != null)
            {
                icon.sprite = sprite; icon.enabled = true; label.enabled = false;
            }
            else
            {
                icon.enabled = false;
                label.text = isFilled ? text : "";
                label.enabled = isFilled;
            }
            ApplyTint();
        }

        private void ApplyTint()
        {
            if (frame != null) frame.color = filled ? filledColor : emptyColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (screen == null || !filled) return;
            if (isGear) screen.UnequipGear(gearSlot);
            else screen.UnequipPart(bodySlot);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (screen == null || eventData.pointerDrag == null) return;
            var bag = eventData.pointerDrag.GetComponent<BagItemWidget>();
            if (bag == null) return;

            bool ok = isGear
                ? bag.IsGear && screen.TryEquipGearToSlot(bag.Gear, gearSlot)
                : !bag.IsGear && screen.TryEquipPartToSlot(bag.Part, bodySlot);

            if (!ok && isActiveAndEnabled) StartCoroutine(FlashReject());
        }

        private IEnumerator FlashReject()
        {
            var orig = frame != null ? frame.color : Color.white;
            if (frame != null) frame.color = new Color(0.80f, 0.22f, 0.22f, Mathf.Max(orig.a, 0.7f));
            yield return new WaitForSecondsRealtime(0.3f);
            ApplyTint();
        }
    }
}
