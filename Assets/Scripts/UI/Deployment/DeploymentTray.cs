using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CapsuleWars.UI.Deployment
{
    /// <summary>
    /// Screen Space–Overlay deployment HUD. Shows a bench of the run party's
    /// not-yet-placed units; tap a bench unit to select it, then tap a deploy-zone
    /// cell to place it; tap an occupied cell to send that unit back to the bench.
    /// "Assemble" confirms (DeploymentPhaseController.Confirm → spawn party + start
    /// combat); "Clear" empties the board. Placements are mirrored into
    /// RunState.Placements so BattlePartySpawner spawns each unit at its cell.
    ///
    /// Cell taps use the new Input System + a ground raycast (the cell tiles / ground
    /// have colliders); taps over the bench/buttons are ignored. The grid renderer
    /// colours cells by state (green = valid empty, blue = occupied, dim = out of zone),
    /// which is the valid/invalid feedback while placing.
    /// </summary>
    public class DeploymentTray : MonoBehaviour
    {
        [Header("Deployment refs")]
        [SerializeField] private DeploymentManager manager;
        [SerializeField] private DeploymentGridRenderer gridRenderer;
        [SerializeField] private DeploymentPhaseController phase;

        [Header("Bench / buttons")]
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private Transform benchRoot;       // parent for the generated bench buttons
        [SerializeField] private Button benchItemPrefab;    // button with a child Text label
        [SerializeField] private Button readyButton;        // "Assemble"
        [SerializeField] private Button clearButton;
        [SerializeField] private Text selectionLabel;

        [Header("Cell input")]
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;

        private string selectedUnitId;

        private void Awake()
        {
            if (manager == null) manager = FindAnyObjectByType<DeploymentManager>();
            if (phase == null) phase = FindAnyObjectByType<DeploymentPhaseController>();
            if (raycastCamera == null) raycastCamera = Camera.main;
            if (readyButton != null) readyButton.onClick.AddListener(OnReady);
            if (clearButton != null) clearButton.onClick.AddListener(OnClear);
        }

        private void Start()
        {
            if (manager != null)
            {
                if (gridRenderer != null) gridRenderer.Build(manager);
                manager.OnPlacementsChanged += HandlePlacementsChanged;
            }
            RebuildBench();
            UpdateSelectionLabel();
        }

        private void OnDestroy()
        {
            if (manager != null) manager.OnPlacementsChanged -= HandlePlacementsChanged;
            if (readyButton != null) readyButton.onClick.RemoveListener(OnReady);
            if (clearButton != null) clearButton.onClick.RemoveListener(OnClear);
        }

        private bool DeploymentActive => phase == null || !phase.IsConfirmed;

        private void Update()
        {
            if (!DeploymentActive || manager == null || raycastCamera == null) return;
            if (!TryGetPress(out Vector2 screenPos)) return;
            // Don't treat a tap on the bench/buttons as a board tap.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (!Physics.Raycast(raycastCamera.ScreenPointToRay(screenPos),
                                 out RaycastHit hit, maxRayDistance, groundMask))
                return;

            HandleCellTap(manager.Config.WorldToCell(hit.point));
        }

        private void HandleCellTap(GridCoord coord)
        {
            // Occupied cell → send that unit back to the bench.
            if (manager.Grid.TryGetOccupant(coord, out var occupant))
            {
                manager.RemoveToken(occupant);
                if (RunSession.IsActive) RunSession.Current.ClearPlacement(occupant);
                Deselect();
                RebuildBench();
                return;
            }

            // Empty cell + a bench unit selected → place it (no-op if not deployable).
            if (!string.IsNullOrEmpty(selectedUnitId) && manager.PlaceToken(selectedUnitId, coord))
            {
                if (RunSession.IsActive) RunSession.Current.SetPlacement(selectedUnitId, coord);
                Deselect();
                RebuildBench();
            }
        }

        private void RebuildBench()
        {
            if (benchRoot == null || benchItemPrefab == null) return;
            for (int i = benchRoot.childCount - 1; i >= 0; i--)
                Destroy(benchRoot.GetChild(i).gameObject);
            if (!RunSession.IsActive) return;

            var placed = manager != null ? manager.GetPlacements() : null;
            foreach (var unit in RunSession.Current.Party)
            {
                if (unit == null) continue;
                if (placed != null && placed.ContainsKey(unit.Id)) continue;   // already on the board

                var btn = Instantiate(benchItemPrefab, benchRoot);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null) label.text = string.IsNullOrEmpty(unit.DisplayName) ? unit.Id : unit.DisplayName;

                string id = unit.Id;
                btn.onClick.AddListener(() => Select(id));
            }
        }

        private void Select(string unitId) { selectedUnitId = unitId; UpdateSelectionLabel(); }
        private void Deselect() { selectedUnitId = null; UpdateSelectionLabel(); }

        private void UpdateSelectionLabel()
        {
            if (selectionLabel == null) return;
            selectionLabel.text = string.IsNullOrEmpty(selectedUnitId)
                ? "Select a unit, then tap a green cell"
                : $"Placing: {selectedUnitId} — tap a cell (or tap a placed unit to bench it)";
        }

        private void HandlePlacementsChanged(IReadOnlyDictionary<string, GridCoord> _)
        {
            if (gridRenderer != null) gridRenderer.Refresh();
        }

        private void OnReady()
        {
            if (RunSession.IsActive) RunSession.Save();
            if (phase != null) phase.Confirm();
            if (hudRoot != null) hudRoot.SetActive(false);
        }

        private void OnClear()
        {
            if (manager != null) manager.ClearAll();
            if (RunSession.IsActive) RunSession.Current.ClearPlacements();
            if (phase != null) phase.NotifyCleared();
            Deselect();
            RebuildBench();
        }

        private static bool TryGetPress(out Vector2 position)
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                position = mouse.position.ReadValue();
                return true;
            }
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame)
            {
                position = ts.primaryTouch.position.ReadValue();
                return true;
            }
            position = default;
            return false;
        }
    }
}
