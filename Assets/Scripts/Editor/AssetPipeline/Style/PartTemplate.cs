using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Part types templates can target. <see cref="StyleComposer"/> maps an
    /// AssetRequest's category + slot to one of these and picks the matching template.
    /// </summary>
    public enum PartType
    {
        Generic = 0,
        Helmet = 1,
        RightHand = 2,
        LeftHand = 3,
        Foot = 4,
        Torso = 5,
        Weapon = 6,
        Armor = 7,
        Head = 8
    }

    /// <summary>
    /// Per-part-type prompt fragment: ONLY this part's specific form criteria + the correct
    /// floating-limb cut. The shared look comes from <see cref="StyleProfile"/> (referenced,
    /// never copied), so every part reads as one game. Editor-only; stored under
    /// <c>Assets/Editor/AssetPipeline/Style/</c>. Seeded by Tools ▸ CapsuleWars ▸ Create Default
    /// Style + Templates; tune the text afterwards.
    /// </summary>
    [CreateAssetMenu(fileName = "PartTemplate", menuName = "CapsuleWars/Asset Pipeline/Part Template", order = 201)]
    public class PartTemplate : ScriptableObject
    {
        [Tooltip("Which part type this template supplies criteria for (matched from the request's category+slot).")]
        public PartType partType = PartType.Generic;

        public string displayName;

        [Header("Form criteria (this part's specific shape rules)")]
        [TextArea(2, 8)] public string criteria;

        [Header("Floating-limb / cut instruction")]
        [Tooltip("e.g. 'floating, cut at the wrist with a smooth rounded stump'. Leave blank for non-limb parts.")]
        [TextArea(1, 4)] public string limbCut;

        [Header("Notes")]
        [TextArea(2, 5)] public string notes;
    }
}
