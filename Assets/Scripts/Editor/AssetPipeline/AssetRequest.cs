using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Which part of the existing item system a finished asset becomes:
    /// Weapon/EquipmentArmor → <c>Equipment_SO</c>; BodyPart → <c>BodyPart_SO</c>.
    /// </summary>
    public enum AssetCategory
    {
        Undecided = 0,
        Weapon = 1,
        EquipmentArmor = 2,
        BodyPart = 3
    }

    /// <summary>
    /// The ordered stages an asset moves through in the creation pipeline.
    /// The Asset Pipeline window groups the queue by this value.
    /// </summary>
    public enum PipelineStage
    {
        Requested = 0,
        ConceptsReady = 1,
        ConceptChosen = 2,
        ImagePrompt = 3,
        ImageChosen = 4,
        MeshyPrompt = 5,
        ModelImported = 6,
        Reviewed = 7,
        Categorized = 8,
        Described = 9,
        Done = 10
    }

    /// <summary>
    /// Working state of a request, separate from its pipeline <see cref="PipelineStage"/>:
    /// Active = in the working queue; Archived = completed/wired, kept as a record out of the
    /// active view; Rejected = abandoned, kept for reference. Stage is preserved so Restore
    /// returns it to where it was. Lifecycle changes never delete the produced game asset.
    /// </summary>
    public enum Lifecycle
    {
        Active = 0,
        Archived = 1,
        Rejected = 2
    }

    /// <summary>One generated concept idea the designer can pick between.</summary>
    [Serializable]
    public class ConceptOption
    {
        public string name;
        [TextArea(2, 5)] public string visualDesc;
        [TextArea(1, 4)] public string styleFit;
    }

    /// <summary>
    /// Editor-only pipeline record: one per asset being designed. Persists as a
    /// <c>.asset</c> under <c>Assets/Editor/AssetPipeline/Requests/</c> (an Editor
    /// folder, stripped from player builds) so the queue survives across sessions.
    /// Holds every stage's artifact; the Asset Pipeline window + Claude (over the
    /// MCP bridge) read/write these fields and advance <see cref="stage"/>.
    /// Fields are public on purpose — this is editor data the window edits directly.
    /// </summary>
    public class AssetRequest : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable id (slug). Used for asset/file names and the eventual equipmentId/partId.")]
        public string id;
        public string title;
        [TextArea(2, 6)]
        [Tooltip("What you want to build, in your words. Drives the concept step.")]
        public string requestText;
        public AssetCategory category = AssetCategory.Undecided;
        public PipelineStage stage = PipelineStage.Requested;

        [Header("1. Concepts")]
        public List<ConceptOption> concepts = new List<ConceptOption>();
        [Tooltip("Index into 'concepts' of the option you picked (-1 = none).")]
        public int chosenConceptIndex = -1;

        [Header("2. Image (Grok)")]
        [TextArea(3, 14)] public string grokImagePrompt;
        [Tooltip("The image you picked from Grok and brought back (drag the imported texture here).")]
        public Texture2D chosenImage;
        [Tooltip("Optional path/URL note for the chosen image.")]
        public string imagePath;

        [Header("3. Model (Meshy)")]
        [TextArea(3, 14)] public string meshyPrompt;

        [Header("4. Import + categorize")]
        [Tooltip("The model imported from Meshy (drag the imported .fbx/.glb root here).")]
        public GameObject importedModel;
        [Tooltip("Prefab created from the imported model by 'Create / Wire item'.")]
        public GameObject generatedPrefab;
        [Tooltip("Read as EquipmentSlot (Weapon/Armor) or PartSlot (BodyPart) per category.")]
        public int targetSlot;
        [Tooltip("Socket name on the unit's UnitEquipmentVisuals (equipment only), e.g. RightHand.")]
        public string attachSocketName;
        [Tooltip("The created Equipment_SO or BodyPart_SO this request produced.")]
        public UnityEngine.Object createdItem;

        [Header("5. Description")]
        [TextArea(4, 14)] public string description;

        [Header("Lifecycle")]
        [Tooltip("Active = in the working queue; Archived = done/wired (kept as a record); Rejected = abandoned. " +
                 "Changing this never deletes the produced game asset. Stage is preserved for Restore.")]
        public Lifecycle lifecycle = Lifecycle.Active;
        [Tooltip("Optional short note on why it was archived/rejected.")]
        public string lifecycleReason;
        [Tooltip("When it was archived/rejected (stamped automatically).")]
        public string lifecycleDate;

        [Header("Mirror")]
        [Tooltip("If this request was produced by 'Mirror to opposite side', the original sided request it mirrors.")]
        public AssetRequest mirrorOf;
        [Tooltip("Set if a plain horizontal mirror would break this part (text, logo, one-sided detail) — the mirror action warns harder.")]
        public bool asymmetric;

        [Header("Notes")]
        [TextArea(2, 6)] public string notes;

        /// <summary>The picked concept, or null if none chosen yet.</summary>
        public ConceptOption ChosenConcept =>
            (chosenConceptIndex >= 0 && chosenConceptIndex < concepts.Count) ? concepts[chosenConceptIndex] : null;
    }
}
