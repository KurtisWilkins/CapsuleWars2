using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using CapsuleWars.UI.Inspection;
using CapsuleWars.UI.Theme;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// Between-rounds customization for the run party (shown in the map scene), as a PAPER-DOLL:
    /// a centered live preview unit flanked by framed equipment slots, a body/cosmetic slot row, a
    /// headline stat footer (HP / DAMAGE / ARMOR) + a Stats button, and a scrollable item BAG with
    /// Gear / Body tabs (paper-doll customization — extends ADR-012, supersedes the old list view).
    ///
    /// The screen spawns a live PREVIEW unit from the party DTO so the real pipeline drives the
    /// display: GEAR routes through <see cref="UnitStatusController"/> (stats + sockets, fires
    /// OnStatsChanged); BODY parts route through <see cref="UnitCustomization.ApplyParts"/> (cosmetic
    /// meshes). On close it writes equipment back to the DTO, and — only if a body part was actually
    /// edited — the parts too, then saves the run so the loadout persists between rounds.
    ///
    /// INTERACTION: tap a bag item → auto-equip to ITS slot (slot read from the item); drag a bag
    /// item onto its slot (or anywhere on the doll) → equip, wrong slot → reject; tap a filled slot →
    /// unequip. Slots/bag are generated at runtime from the slot enums + the preview's mounted slots,
    /// so only the layout containers need wiring in-scene. Drop a UIThemeApplier on the root to theme.
    /// </summary>
    public class CustomizationScreen : MonoBehaviour
    {
        private enum BagTab { Gear, Body }

        [SerializeField] private GameObject panelRoot;

        [Header("Preview spawn")]
        [SerializeField] private UnitRoot baseUnitPrefab;
        [SerializeField] private UnitDefinitionCatalog_SO definitionCatalog;
        [SerializeField] private PartCatalog_SO partCatalog;
        [SerializeField] private EquipmentCatalog_SO equipmentCatalog;
        [SerializeField] private Transform previewAnchor;

        [Tooltip("Default items the player starts with (testing). Shown in the bag in addition to the catalog " +
                 "(deduped by id). Add these to the EquipmentCatalog too so they resolve when units spawn in combat.")]
        [SerializeField] private List<Equipment_SO> starterItems = new();

        [Header("Paper-doll layout (containers; slots/bag are generated into these)")]
        [Tooltip("Vertical container for the left column of gear slots.")]
        [SerializeField] private Transform leftColumnRoot;
        [Tooltip("Vertical container for the right column of gear slots.")]
        [SerializeField] private Transform rightColumnRoot;
        [Tooltip("Container for the cosmetic body-part slots (only the preview's mounted slots appear).")]
        [SerializeField] private Transform bodyRoot;
        [Tooltip("ScrollRect content (with a GridLayoutGroup) the bag items are generated into.")]
        [SerializeField] private Transform bagContentRoot;

        [Header("Footer + tabs + inspection")]
        [SerializeField] private Text hpText;
        [SerializeField] private Text damageText;
        [SerializeField] private Text armorText;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button bagGearTabButton;
        [SerializeField] private Button bagBodyTabButton;
        [SerializeField] private UnitInspectionPanel inspectionPanel;
        [SerializeField] private Button closeButton;

        [Header("Theme (optional — slot/bag colours)")]
        [SerializeField] private UIThemePalette palette;

        private UnitRoot preview;
        private UnitStatusController status;
        private string currentUnitId;
        private bool partsDirty;
        private BagTab bagTab = BagTab.Gear;

        private readonly List<PaperDollSlot> gearSlots = new();
        private readonly List<PaperDollSlot> bodySlots = new();
        private readonly List<BagItemWidget> bagItems = new();

        private Image dragGhost;
        private RectTransform ghostRect;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (statsButton != null) statsButton.onClick.AddListener(ShowStats);
            if (bagGearTabButton != null) bagGearTabButton.onClick.AddListener(() => SetBagTab(BagTab.Gear));
            if (bagBodyTabButton != null) bagBodyTabButton.onClick.AddListener(() => SetBagTab(BagTab.Body));
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDisable() => Unsubscribe();

        /// <summary>Open the screen for one party member (by unit id).</summary>
        public void Show(string unitId)
        {
            if (!RunSession.IsActive) return;
            var dto = FindParty(unitId);
            if (dto == null) return;

            currentUnitId = unitId;
            partsDirty = false;
            bagTab = BagTab.Gear;
            SpawnPreview(dto);
            if (panelRoot != null) panelRoot.SetActive(true);
            EnsureForeground();
            EnsureDropZone();

            BuildSlots();
            RebuildBag();
            UpdateTabVisuals();
            RefreshAll();
        }

        /// <summary>Persist the current loadout and close.</summary>
        public void Close()
        {
            Capture();
            Unsubscribe();
            EndDragVisual();
            if (inspectionPanel != null) inspectionPanel.Hide();
            if (preview != null) Destroy(preview.gameObject);
            preview = null;
            status = null;
            currentUnitId = null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        /// <summary>Open the shared inspection panel on the preview for the full stat breakdown.</summary>
        private void ShowStats()
        {
            if (inspectionPanel != null && preview != null) inspectionPanel.Show(preview);
        }

        // Guarantee the panel renders and raycasts above all other map UI (ADR-012): a nested Canvas
        // with overrideSorting + a high sortingOrder, its own GraphicRaycaster, and an input-accepting
        // CanvasGroup.
        private void EnsureForeground()
        {
            if (panelRoot == null) return;
            var canvas = panelRoot.GetComponent<Canvas>();
            if (canvas == null) canvas = panelRoot.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            if (panelRoot.GetComponent<GraphicRaycaster>() == null) panelRoot.AddComponent<GraphicRaycaster>();
            var cg = panelRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelRoot.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            cg.interactable = true;
            cg.alpha = 1f;
        }

        // The root needs a PaperDollDropZone (with a raycast-target Image) so drops that miss a slot
        // auto-route. Add it once, lazily, so the scene only needs the panel root wired.
        private void EnsureDropZone()
        {
            if (panelRoot == null) return;
            if (panelRoot.GetComponent<Image>() == null)
            {
                var img = panelRoot.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.001f); // effectively invisible, still a raycast target
            }
            var zone = panelRoot.GetComponent<PaperDollDropZone>();
            if (zone == null) zone = panelRoot.AddComponent<PaperDollDropZone>();
            zone.Configure(this);
        }

        private void SpawnPreview(UnitDTO dto)
        {
            if (preview != null) Destroy(preview.gameObject);
            if (baseUnitPrefab == null) return;

            var defDb = definitionCatalog != null ? definitionCatalog.BuildDatabase() : null;
            Vector3 pos = previewAnchor != null ? previewAnchor.position : Vector3.zero;
            Quaternion rot = previewAnchor != null ? previewAnchor.rotation : Quaternion.identity;

            preview = UnitFactory.Spawn(dto, baseUnitPrefab, defDb, pos, rot,
                                        parent: previewAnchor, partDatabase: partCatalog,
                                        equipmentDatabase: equipmentCatalog);

            status = preview != null ? preview.Status : null;
            Subscribe();
            ApplyPreviewLayer();
        }

        private void Subscribe()
        {
            if (status != null) status.OnStatsChanged += RefreshStats;
        }

        private void Unsubscribe()
        {
            if (status != null) status.OnStatsChanged -= RefreshStats;
        }

        // ---- slot + bag generation -------------------------------------------------------------

        private Color SlotEmptyColor => palette != null ? palette.buttonNormal : new Color(0.16f, 0.18f, 0.24f, 1f);
        private Color SlotFilledColor => palette != null ? palette.accent : new Color(0.20f, 0.55f, 0.30f, 1f);

        private void BuildSlots()
        {
            ClearChildren(leftColumnRoot);
            ClearChildren(rightColumnRoot);
            ClearChildren(bodyRoot);
            gearSlots.Clear();
            bodySlots.Clear();

            // Gear: all 8 EquipmentSlots, split evenly between the two flanking columns.
            var slots = (EquipmentSlot[])System.Enum.GetValues(typeof(EquipmentSlot));
            int half = Mathf.CeilToInt(slots.Length / 2f);
            for (int i = 0; i < slots.Length; i++)
            {
                var parent = i < half ? leftColumnRoot : rightColumnRoot;
                if (parent == null) continue;
                var w = CreateSlot(parent);
                w.ConfigureGear(this, slots[i]);
                w.SetThemeColors(SlotEmptyColor, SlotFilledColor);
                gearSlots.Add(w);
            }

            // Body: only the slots the preview prefab actually exposes a mount for.
            var custom = PreviewCustom;
            if (bodyRoot != null && custom != null)
            {
                var seen = new HashSet<PartSlot>();
                foreach (var ps in custom.MountedSlots)
                {
                    if (!seen.Add(ps)) continue;
                    var w = CreateSlot(bodyRoot);
                    w.ConfigureBody(this, ps);
                    w.SetThemeColors(SlotEmptyColor, SlotFilledColor);
                    bodySlots.Add(w);
                }
            }
        }

        private void RebuildBag()
        {
            ClearChildren(bagContentRoot);
            bagItems.Clear();
            if (bagContentRoot == null) return;

            if (bagTab == BagTab.Gear)
            {
                foreach (var item in AvailableItems())
                {
                    var w = CreateBagItem(bagContentRoot);
                    w.ConfigureGear(this, item, $"{item.Slot}\n{ItemName(item)}");
                    w.SetThemeColors(SlotEmptyColor, SlotFilledColor);
                    bagItems.Add(w);
                }
            }
            else
            {
                foreach (var part in AvailableParts())
                {
                    var w = CreateBagItem(bagContentRoot);
                    w.ConfigureBody(this, part, $"{part.Slot}\n{PartName(part)}");
                    w.SetThemeColors(SlotEmptyColor, SlotFilledColor);
                    bagItems.Add(w);
                }
            }
            RefreshBagHighlights();
        }

        private void SetBagTab(BagTab tab)
        {
            if (bagTab == tab) return;
            bagTab = tab;
            UpdateTabVisuals();
            RebuildBag();
        }

        private void UpdateTabVisuals()
        {
            if (bagGearTabButton != null) bagGearTabButton.interactable = bagTab != BagTab.Gear;
            if (bagBodyTabButton != null) bagBodyTabButton.interactable = bagTab != BagTab.Body;
        }

        private PaperDollSlot CreateSlot(Transform parent)
        {
            var go = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(PaperDollSlot));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 70; le.preferredHeight = 70;
            le.minWidth = 48; le.minHeight = 48;
            return go.GetComponent<PaperDollSlot>();
        }

        private BagItemWidget CreateBagItem(Transform parent)
        {
            var go = new GameObject("BagItem", typeof(RectTransform), typeof(Image),
                                    typeof(CanvasGroup), typeof(BagItemWidget));
            go.transform.SetParent(parent, false);
            return go.GetComponent<BagItemWidget>();
        }

        private static void ClearChildren(Transform t)
        {
            if (t == null) return;
            for (int i = t.childCount - 1; i >= 0; i--)
                Destroy(t.GetChild(i).gameObject);
        }

        // ---- equip / unequip (the public API the widgets call) ---------------------------------

        /// <summary>Tap/auto-route a gear item to its own slot.</summary>
        public void RouteEquipGear(Equipment_SO item)
        {
            if (item != null) EquipGearToSlot(item, item.Slot);
        }

        /// <summary>Drop a gear item on a specific slot — equips only if the slot matches (else reject).</summary>
        public bool TryEquipGearToSlot(Equipment_SO item, EquipmentSlot target)
        {
            if (item == null || item.Slot != target) return false;
            EquipGearToSlot(item, target);
            return true;
        }

        private void EquipGearToSlot(Equipment_SO item, EquipmentSlot slot)
        {
            if (status == null) return;
            status.Equip(slot, item);   // compat overload → default instance (stats preserved, ADR-019)
            RefreshAll();
        }

        public void UnequipGear(EquipmentSlot slot)
        {
            if (status == null) return;
            status.UnequipSlot(slot);
            RefreshAll();
        }

        /// <summary>Tap/auto-route a body part to its own slot.</summary>
        public void RouteEquipPart(BodyPart_SO part)
        {
            if (part != null) EquipPartToSlot(part, part.Slot);
        }

        /// <summary>Drop a body part on a specific slot — equips only if the slot matches (else reject).</summary>
        public bool TryEquipPartToSlot(BodyPart_SO part, PartSlot target)
        {
            if (part == null || part.Slot != target) return false;
            EquipPartToSlot(part, target);
            return true;
        }

        private void EquipPartToSlot(BodyPart_SO part, PartSlot slot)
        {
            var custom = PreviewCustom;
            if (custom == null) return;

            var set = new List<PartAssignment>(custom.AppliedParts);
            bool replaced = false;
            for (int i = 0; i < set.Count; i++)
                if (set[i].slot == slot) { set[i] = new PartAssignment { slot = slot, part = part }; replaced = true; break; }
            if (!replaced) set.Add(new PartAssignment { slot = slot, part = part });

            custom.ApplyParts(set, custom.AppliedPalette);
            partsDirty = true;
            RefreshAll();
        }

        public void UnequipPart(PartSlot slot)
        {
            var custom = PreviewCustom;
            if (custom == null) return;

            var set = new List<PartAssignment>(custom.AppliedParts);
            set.RemoveAll(pa => pa.slot == slot);
            custom.ApplyParts(set, custom.AppliedPalette);
            partsDirty = true;
            RefreshAll();
        }

        // ---- refresh ---------------------------------------------------------------------------

        private void RefreshAll()
        {
            RefreshSlots();
            RefreshBagHighlights();
            RefreshStats();
            ApplyPreviewLayer();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < gearSlots.Count; i++)
            {
                var slot = gearSlots[i];
                var eq = FindEquipped(slot.GearSlot);
                if (eq.HasValue)
                    slot.SetGear(eq.Value.item != null ? eq.Value.item.Icon : null, EquippedName(eq.Value), true);
                else
                    slot.SetGear(null, "", false);
            }

            var custom = PreviewCustom;
            for (int i = 0; i < bodySlots.Count; i++)
            {
                var slot = bodySlots[i];
                var part = custom != null ? FindAppliedPart(custom, slot.BodySlot) : null;
                slot.SetBody(part != null ? part.Icon : null, part != null ? PartName(part) : "", part != null);
            }
        }

        private void RefreshBagHighlights()
        {
            for (int i = 0; i < bagItems.Count; i++)
            {
                var w = bagItems[i];
                bool equipped = w.IsGear ? IsGearEquipped(w.Gear) : IsPartEquipped(w.Part);
                w.SetEquipped(equipped);
            }
        }

        private void RefreshStats()
        {
            if (hpText != null) hpText.text = status != null ? $"HP\n{status.MaxHp}" : "HP\n—";
            if (damageText != null) damageText.text = status != null ? $"DAMAGE\n{status.Atk}" : "DAMAGE\n—";
            if (armorText != null) armorText.text = status != null ? $"ARMOR\n{status.Def}" : "ARMOR\n—";
        }

        // ---- drag ghost ------------------------------------------------------------------------

        public void BeginDragVisual(Sprite icon, string label, Vector2 screenPos)
        {
            EnsureGhost();
            if (dragGhost == null) return;
            if (icon != null) { dragGhost.sprite = icon; dragGhost.color = Color.white; }
            else { dragGhost.sprite = null; dragGhost.color = new Color(0.85f, 0.88f, 0.95f, 0.6f); }
            dragGhost.transform.SetAsLastSibling();
            dragGhost.gameObject.SetActive(true);
            UpdateDragVisual(screenPos);
        }

        public void UpdateDragVisual(Vector2 screenPos)
        {
            if (ghostRect == null) return;
            var canvas = panelRoot != null ? panelRoot.GetComponent<Canvas>() : null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                var cam = canvas.worldCamera;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        (RectTransform)canvas.transform, screenPos, cam, out var local))
                    ghostRect.localPosition = local;
            }
            else
            {
                ghostRect.position = screenPos;
            }
        }

        public void EndDragVisual()
        {
            if (dragGhost != null) dragGhost.gameObject.SetActive(false);
        }

        private void EnsureGhost()
        {
            if (dragGhost != null) return;
            var canvas = panelRoot != null ? panelRoot.GetComponent<Canvas>() : null;
            Transform parent = canvas != null ? canvas.transform : (panelRoot != null ? panelRoot.transform : transform);
            var go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            ghostRect = (RectTransform)go.transform;
            ghostRect.sizeDelta = new Vector2(64, 64);
            dragGhost = go.GetComponent<Image>();
            dragGhost.raycastTarget = false;
            dragGhost.preserveAspect = true;
            var gcg = go.GetComponent<CanvasGroup>();
            gcg.blocksRaycasts = false; gcg.interactable = false; gcg.alpha = 0.85f;
            go.SetActive(false);
        }

        // ---- capture / persistence -------------------------------------------------------------

        // Write the preview's loadout back into the (existing) party DTO in place, preserving its
        // identity, then persist. Body parts are only overwritten when the player actually edited one
        // (partsDirty), so a gear-only session doesn't freeze a definition-driven unit into explicit
        // dto.Parts. Load already round-trips dto.Parts (UnitFactory.FromDTO).
        private void Capture()
        {
            if (preview == null || status == null || !RunSession.IsActive) return;
            var dto = FindParty(currentUnitId);
            if (dto == null) return;

            dto.Equipment.Clear();
            foreach (var eq in status.Equipment)
                if (eq.instance?.definition != null) dto.Equipment.Add(UnitEquipmentDTO.From(eq.slot, eq.instance));

            if (partsDirty)
            {
                var custom = PreviewCustom;
                if (custom != null)
                {
                    dto.Parts.Clear();
                    foreach (var pa in custom.AppliedParts)
                        if (pa.part != null) dto.Parts.Add(new UnitPartDTO(pa.slot, pa.part.PartId));
                }
            }

            RunSession.Save();
        }

        // ---- helpers ---------------------------------------------------------------------------

        private UnitCustomization PreviewCustom =>
            preview != null ? preview.GetComponentInChildren<UnitCustomization>() : null;

        // Put the preview unit — and any equipment/body meshes added lazily on equip — onto the preview
        // rig's layer so the dedicated preview Camera renders it to the RawImage (ADR-034). Re-applied
        // after every change because those child meshes are spawned on demand. Backward-compatible: when
        // no rig has been built the anchor sits on the Default layer, so this is a no-op and the unit
        // renders in-world exactly as before (ADR-032).
        private void ApplyPreviewLayer()
        {
            if (preview == null || previewAnchor == null) return;
            SetLayerRecursively(preview.gameObject, previewAnchor.gameObject.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go.layer != layer) go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        private EquippedItem? FindEquipped(EquipmentSlot slot)
        {
            if (status == null) return null;
            foreach (var eq in status.Equipment)
                if (eq.slot == slot && eq.item != null) return eq;
            return null;
        }

        private bool IsGearEquipped(Equipment_SO item)
        {
            if (status == null || item == null) return false;
            foreach (var eq in status.Equipment)
                if (eq.slot == item.Slot && eq.item == item) return true;
            return false;
        }

        private static BodyPart_SO FindAppliedPart(UnitCustomization custom, PartSlot slot)
        {
            var parts = custom.AppliedParts;
            for (int i = 0; i < parts.Count; i++)
                if (parts[i].slot == slot) return parts[i].part;
            return null;
        }

        private bool IsPartEquipped(BodyPart_SO part)
        {
            var custom = PreviewCustom;
            if (custom == null || part == null) return false;
            return FindAppliedPart(custom, part.Slot) == part;
        }

        private static string EquippedName(EquippedItem eq) =>
            eq.instance != null ? eq.instance.Name : (eq.item != null ? eq.item.EquipmentId : "");

        private static string ItemName(Equipment_SO item) =>
            item == null ? "" : (string.IsNullOrEmpty(item.NameTermKey) ? item.EquipmentId : item.NameTermKey);

        private static string PartName(BodyPart_SO part) =>
            part == null ? "" : (string.IsNullOrEmpty(part.NameTermKey) ? part.PartId : part.NameTermKey);

        // Catalog items plus serialized starter items, deduped by id (catalog wins).
        private IEnumerable<Equipment_SO> AvailableItems()
        {
            var seen = new HashSet<string>();
            if (equipmentCatalog != null)
                foreach (var it in equipmentCatalog.Items)
                    if (it != null && seen.Add(it.EquipmentId)) yield return it;
            if (starterItems != null)
                foreach (var it in starterItems)
                    if (it != null && seen.Add(it.EquipmentId)) yield return it;
        }

        // Catalog body parts, restricted to the slots the preview prefab can actually show.
        private IEnumerable<BodyPart_SO> AvailableParts()
        {
            if (partCatalog == null) yield break;

            HashSet<PartSlot> mounted = null;
            var custom = PreviewCustom;
            if (custom != null)
            {
                mounted = new HashSet<PartSlot>();
                foreach (var ps in custom.MountedSlots) mounted.Add(ps);
            }

            var seen = new HashSet<string>();
            foreach (var entry in partCatalog.Parts)
            {
                var p = entry?.part;
                if (p == null) continue;
                if (mounted != null && !mounted.Contains(p.Slot)) continue;
                if (seen.Add(p.PartId)) yield return p;
            }
        }

        private UnitDTO FindParty(string unitId)
        {
            if (string.IsNullOrEmpty(unitId)) return null;
            foreach (var u in RunSession.Current.Party)
                if (u != null && u.Id == unitId) return u;
            return null;
        }
    }
}
