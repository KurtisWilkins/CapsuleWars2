using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// Background drop catcher on the paper-doll root. uGUI bubbles a drop up to the nearest
    /// <see cref="IDropHandler"/> ancestor, so a bag item dropped anywhere on the doll that ISN'T a
    /// specific <see cref="PaperDollSlot"/> lands here → auto-routed to the item's own correct slot
    /// (the "drag it onto the paper-doll and it equips" path). Drops directly on a slot are handled
    /// by that slot (which validates + can reject), since it's the closer handler.
    ///
    /// Needs a raycast-target Image covering the root (added in <see cref="Reset"/>).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PaperDollDropZone : MonoBehaviour, IDropHandler
    {
        private CustomizationScreen screen;

        public void Configure(CustomizationScreen owner) => screen = owner;

        public void OnDrop(PointerEventData eventData)
        {
            if (screen == null || eventData.pointerDrag == null) return;
            var bag = eventData.pointerDrag.GetComponent<BagItemWidget>();
            if (bag == null) return;
            if (bag.IsGear) screen.RouteEquipGear(bag.Gear);
            else screen.RouteEquipPart(bag.Part);
        }
    }
}
