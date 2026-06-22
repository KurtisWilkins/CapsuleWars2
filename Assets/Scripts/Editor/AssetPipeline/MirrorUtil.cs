using CapsuleWars.Core;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Sidedness helpers for the mirror/flip feature. Sidedness lives in the slot, not
    /// the PartType (feet both map to PartType.Foot), so this keys off category + slot.
    /// Pure — no I/O. Editor-only.
    /// </summary>
    public static class MirrorUtil
    {
        /// <summary>
        /// For a sided part (R/L hand or foot), returns the opposite slot and the side words.
        /// False for non-sided parts (helmet, torso, weapon, armor, body).
        /// </summary>
        public static bool TryGetOpposite(AssetCategory category, int slot,
            out int oppositeSlot, out string sideWord, out string oppositeWord)
        {
            oppositeSlot = slot;
            sideWord = "";
            oppositeWord = "";

            switch (category)
            {
                case AssetCategory.EquipmentArmor:
                    switch ((EquipmentSlot)slot)
                    {
                        case EquipmentSlot.RightHand:
                            oppositeSlot = (int)EquipmentSlot.LeftHand; sideWord = "Right"; oppositeWord = "Left"; return true;
                        case EquipmentSlot.LeftHand:
                            oppositeSlot = (int)EquipmentSlot.RightHand; sideWord = "Left"; oppositeWord = "Right"; return true;
                    }
                    return false;

                case AssetCategory.BodyPart:
                    switch ((PartSlot)slot)
                    {
                        case PartSlot.RightHand:
                            oppositeSlot = (int)PartSlot.LeftHand; sideWord = "Right"; oppositeWord = "Left"; return true;
                        case PartSlot.LeftHand:
                            oppositeSlot = (int)PartSlot.RightHand; sideWord = "Left"; oppositeWord = "Right"; return true;
                        case PartSlot.RightFoot:
                            oppositeSlot = (int)PartSlot.LeftFoot; sideWord = "Right"; oppositeWord = "Left"; return true;
                        case PartSlot.LeftFoot:
                            oppositeSlot = (int)PartSlot.RightFoot; sideWord = "Left"; oppositeWord = "Right"; return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>True if this request is a sided part with a mirror counterpart.</summary>
        public static bool IsSided(AssetRequest r) =>
            r != null && TryGetOpposite(r.category, r.targetSlot, out _, out _, out _);
    }
}
