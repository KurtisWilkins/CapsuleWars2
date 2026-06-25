using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using CapsuleWars.Run;
using CapsuleWars.UI.Inspection;
using CapsuleWars.Units.Controllers;
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
        [Tooltip("Spawns the visible unit at each placed cell (and despawns on bench/clear).")]
        [SerializeField] private CapsuleWars.Run.BattlePartySpawner spawner;

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

        [Header("Enemy inspection")]
        [Tooltip("Panel shown (read-only) when an enemy-zone cell holding an enemy is tapped. Optional.")]
        [SerializeField] private UnitInspectionPanel enemyInspectionPanel;

        private string selectedUnitId;

        private void Awake()
        {
            if (manager == null) manager = FindAnyObjectByType<DeploymentManager>();
            if (phase == null) phase = FindAnyObjectByType<DeploymentPhaseController>();
            if (spawner == null) spawner = FindAnyObjectByType<CapsuleWars.Run.BattlePartySpawner>();
            if (enemyInspectionPanel == null) enemyInspectionPanel = FindAnyObjectByType<UnitInspectionPanel>(FindObjectsInactive.Include);
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
            RestoreSavedPlacements();   // auto-deploy the player's last saved layout
            RebuildBench();
            UpdateSelectionLabel();
        }

        // Re-deploy the player's last saved layout (RunState.Placements) so they don't have to
        // place units every combat: each saved placement for a CURRENT party member is re-placed
        // via the normal place path (token + live preview) and left off the bench. Units with no
        // saved cell stay benched; placements for units no longer in the party are dropped. The
        // player can still bench/move any unit or hit Clear to start over.
        private void RestoreSavedPlacements()
        {
            if (manager == null || !RunSession.IsActive) return;
            var placements = RunSession.Current.Placements;
            if (placements == null || placements.Count == 0) return;

            var partyIds = new HashSet<string>();
            foreach (var u in RunSession.Current.Party)
                if (u != null && !string.IsNullOrEmpty(u.Id)) partyIds.Add(u.Id);

            // Snapshot first — ClearPlacement mutates the dictionary we're iterating.
            var saved = new List<KeyValuePair<string, GridCoord>>(placements);
            foreach (var kv in saved)
            {
                if (!partyIds.Contains(kv.Key)) { RunSession.Current.ClearPlacement(kv.Key); continue; }
                if (manager.PlaceToken(kv.Key, kv.Value) && spawner != null)
                    spawner.SpawnOrMoveAt(kv.Key, kv.Value);
            }
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
            // Enemy zone → inspect the enemy standing there (read-only; never places/benches).
            if (manager.Config.InEnemyZone(coord))
            {
                var enemy = FindEnemyAtCell(coord);
                if (enemy != null && enemyInspectionPanel != null) enemyInspectionPanel.Show(enemy);
                return;
            }

            // Occupied cell → send that unit back to the bench.
            if (manager.Grid.TryGetOccupant(coord, out var occupant))
            {
                manager.RemoveToken(occupant);
                if (spawner != null) spawner.Despawn(occupant);
                if (RunSession.IsActive) RunSession.Current.ClearPlacement(occupant);
                Deselect();
                RebuildBench();
                return;
            }

            // Empty cell + a bench unit selected → place it (no-op if not deployable).
            if (!string.IsNullOrEmpty(selectedUnitId) && manager.PlaceToken(selectedUnitId, coord))
            {
                if (spawner != null) spawner.SpawnOrMoveAt(selectedUnitId, coord);
                if (RunSession.IsActive) RunSession.Current.SetPlacement(selectedUnitId, coord);
                Deselect();
                RebuildBench();
            }
        }

        // Nearest Team.Enemy unit on the tapped cell (exact cell wins, else the closest
        // enemy within ~one cell), for read-only inspection. Null if none.
        private UnitRoot FindEnemyAtCell(GridCoord coord)
        {
            var cfg = manager.Config;
            Vector3 cellWorld = cfg.CellToWorld(coord);
            var roots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            UnitRoot best = null;
            float bestSqr = cfg.cellSize * cfg.cellSize;   // accept within ~one cell
            for (int i = 0; i < roots.Length; i++)
            {
                var r = roots[i];
                if (r == null || r.Team != Team.Enemy) continue;
                var c = cfg.WorldToCell(r.transform.position);
                if (c.col == coord.col && c.row == coord.row) return r;   // exact cell match
                float d = (r.transform.position - cellWorld).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = r; }
            }
            return best;
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
            if (spawner != null) spawner.DespawnAll();
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
