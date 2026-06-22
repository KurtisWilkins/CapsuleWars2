using System.Collections.Generic;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using CapsuleWars.UI.Inspection;
using CapsuleWars.Units.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// Between-rounds armor customization for the run party (shown in the map scene).
    /// Since the map scene has no live battle units, it spawns a live PREVIEW unit
    /// from the selected party member's DTO so the real stat pipeline drives the
    /// display: equipping armor from the catalog fires UnitStatusController.OnStatsChanged
    /// and the shared UnitInspectionPanel updates live. On close it writes the
    /// preview's equipment back into the party DTO (preserving identity/parts) and
    /// saves the run, so the loadout persists between rounds.
    ///
    /// Armor carries the stats; body parts are cosmetic. Equipment ids resolve via
    /// the EquipmentCatalog (the run's available armor). Build the panel hierarchy +
    /// wire refs in-scene; QA in Play mode.
    /// </summary>
    public class CustomizationScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        [Header("Preview spawn")]
        [SerializeField] private UnitRoot baseUnitPrefab;
        [SerializeField] private UnitDefinitionCatalog_SO definitionCatalog;
        [SerializeField] private PartCatalog_SO partCatalog;
        [SerializeField] private EquipmentCatalog_SO equipmentCatalog;
        [SerializeField] private Transform previewAnchor;

        [Tooltip("Default items the player starts with (testing). Shown in the list in addition to the catalog " +
                 "(deduped by id). Add these to the EquipmentCatalog too so they resolve when units spawn in combat.")]
        [SerializeField] private List<Equipment_SO> starterItems = new();

        [Header("UI")]
        [SerializeField] private UnitInspectionPanel inspectionPanel;
        [SerializeField] private Transform equipmentListRoot;   // parent for the generated equip buttons
        [SerializeField] private Button equipButtonPrefab;      // button with a child Text label
        [SerializeField] private Button closeButton;

        [Header("Selection feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color equippedColor = new Color(0.3f, 0.8f, 0.4f, 1f);

        private UnitRoot preview;
        private string currentUnitId;
        private readonly List<(Equipment_SO item, Button button)> itemButtons = new();

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        /// <summary>Open the screen for one party member (by unit id).</summary>
        public void Show(string unitId)
        {
            if (!RunSession.IsActive) return;
            var dto = FindParty(unitId);
            if (dto == null) return;

            currentUnitId = unitId;
            SpawnPreview(dto);
            if (panelRoot != null) panelRoot.SetActive(true);
            EnsureForeground();
            if (inspectionPanel != null && preview != null) inspectionPanel.Show(preview);
            BuildEquipmentList();
        }

        /// <summary>Persist the current loadout and close.</summary>
        public void Close()
        {
            Capture();
            if (inspectionPanel != null) inspectionPanel.Hide();
            if (preview != null) Destroy(preview.gameObject);
            preview = null;
            currentUnitId = null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // Guarantee the panel renders and raycasts above all other map UI, regardless of
        // scene wiring: a nested Canvas with overrideSorting + a high sortingOrder, its own
        // GraphicRaycaster, and a CanvasGroup that accepts input.
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
        }

        private void BuildEquipmentList()
        {
            if (equipmentListRoot == null || equipButtonPrefab == null) return;

            for (int i = equipmentListRoot.childCount - 1; i >= 0; i--)
                Destroy(equipmentListRoot.GetChild(i).gameObject);
            itemButtons.Clear();

            foreach (var item in AvailableItems())
            {
                var btn = Instantiate(equipButtonPrefab, equipmentListRoot);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{item.Slot}: {item.EquipmentId}";

                var captured = item;   // avoid closure capturing the loop variable
                btn.onClick.AddListener(() => ToggleItem(captured));
                itemButtons.Add((item, btn));
            }
            RefreshHighlights();
        }

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

        // Toggle: clicking an item equips it (replacing its slot), or unequips it if
        // it's already the item in that slot. Either way fires OnStatsChanged, so the
        // inspection panel + the unit's equipment visuals update live.
        private void ToggleItem(Equipment_SO item)
        {
            if (preview == null || item == null || preview.Status == null) return;
            if (IsEquipped(item)) preview.Status.UnequipSlot(item.Slot);
            else preview.Status.Equip(item.Slot, item);
            RefreshHighlights();
        }

        private bool IsEquipped(Equipment_SO item)
        {
            if (preview == null || preview.Status == null || item == null) return false;
            foreach (var eq in preview.Status.Equipment)
                if (eq.slot == item.Slot && eq.item == item) return true;
            return false;
        }

        private void RefreshHighlights()
        {
            foreach (var (item, button) in itemButtons)
            {
                if (button == null) continue;
                var img = (button.targetGraphic as Image) ?? button.GetComponent<Image>();
                if (img != null) img.color = IsEquipped(item) ? equippedColor : normalColor;
            }
        }

        // Write the preview's equipment back into the (existing) party DTO in place,
        // preserving its identity/parts/palette, then persist the run.
        private void Capture()
        {
            if (preview == null || preview.Status == null || !RunSession.IsActive) return;
            var dto = FindParty(currentUnitId);
            if (dto == null) return;

            dto.Equipment.Clear();
            foreach (var eq in preview.Status.Equipment)
                if (eq.instance?.definition != null) dto.Equipment.Add(UnitEquipmentDTO.From(eq.slot, eq.instance));

            RunSession.Save();
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
