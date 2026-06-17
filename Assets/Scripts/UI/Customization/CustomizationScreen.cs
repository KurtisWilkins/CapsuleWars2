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

        [Header("UI")]
        [SerializeField] private UnitInspectionPanel inspectionPanel;
        [SerializeField] private Transform equipmentListRoot;   // parent for the generated equip buttons
        [SerializeField] private Button equipButtonPrefab;      // button with a child Text label
        [SerializeField] private Button closeButton;

        private UnitRoot preview;
        private string currentUnitId;

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
            if (equipmentListRoot == null || equipButtonPrefab == null || equipmentCatalog == null) return;

            for (int i = equipmentListRoot.childCount - 1; i >= 0; i--)
                Destroy(equipmentListRoot.GetChild(i).gameObject);

            foreach (var item in equipmentCatalog.Items)
            {
                if (item == null) continue;
                var btn = Instantiate(equipButtonPrefab, equipmentListRoot);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{item.Slot}: {item.EquipmentId}";

                var captured = item;   // avoid closure capturing the loop variable
                btn.onClick.AddListener(() => EquipItem(captured));
            }
        }

        private void EquipItem(Equipment_SO item)
        {
            if (preview == null || item == null || preview.Status == null) return;
            // Fires OnStatsChanged -> the inspection panel refreshes its stats live.
            preview.Status.Equip(item.Slot, item);
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
                if (eq.item != null) dto.Equipment.Add(new UnitEquipmentDTO(eq.slot, eq.item.EquipmentId));

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
