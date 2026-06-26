using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.UI.Arena;
using UnityEngine;

namespace CapsuleWars.UI.Deployment
{
    /// <summary>
    /// Renders the deployment grid as flat world-space cell tiles, recolored by
    /// <see cref="CellState"/> for valid/occupied/blocked feedback. Instantiates one
    /// <see cref="cellPrefab"/> per cell at the config's CellToWorld position; call
    /// <see cref="Build"/> once, then <see cref="Refresh"/> whenever placements change.
    ///
    /// cellPrefab should be a flat tile (e.g. a Quad laid on the ground) with a
    /// Renderer; its material color is set per state. Outside-zone cells are hidden
    /// by default (only the player's deploy zone is shown) — toggle with
    /// <see cref="showOutsideZone"/>.
    /// </summary>
    public class DeploymentGridRenderer : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [Tooltip("Gap above the floor/ground SURFACE. Resolved relative to the ArenaBuilder checkerboard top " +
                 "(floorSurfaceY) when present, so the runtime floor can never bury the highlight; falls back to " +
                 "ground (Y 0) with no arena.")]
        [SerializeField] private float yOffset = 0.02f;
        [SerializeField] private bool showOutsideZone = false;

        [Header("State colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.8f, 0.3f, 0.5f);
        [SerializeField] private Color occupiedColor = new Color(0.2f, 0.5f, 0.9f, 0.5f);
        [SerializeField] private Color blockedColor = new Color(0.9f, 0.2f, 0.2f, 0.5f);
        [Tooltip("Hazard terrain (lava/trap): placeable but harmful. Minimal Slice-A feedback; Slice B themes it.")]
        [SerializeField] private Color hazardColor = new Color(0.95f, 0.6f, 0.1f, 0.55f);
        [SerializeField] private Color outsideColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        [Tooltip("Far-side enemy zone tiles, shown so the two sides read as clearly separated.")]
        [SerializeField] private Color enemyZoneColor = new Color(0.9f, 0.35f, 0.2f, 0.4f);

        private DeploymentManager manager;
        private readonly Dictionary<GridCoord, Renderer> cells = new Dictionary<GridCoord, Renderer>();

        /// <summary>Instantiate tiles for every in-bounds cell and colour them once.</summary>
        public void Build(DeploymentManager deploymentManager)
        {
            manager = deploymentManager;
            Clear();
            if (manager == null || cellPrefab == null) return;

            var cfg = manager.Config;
            // Float the overlay above the ArenaBuilder checkerboard floor (top = floorSurfaceY) so the runtime
            // floor can't bury the highlight; with no arena it sits just above the legacy ground (Y 0).
            var arena = FindFirstObjectByType<ArenaBuilder>();
            float surfaceY = arena != null ? arena.FloorSurfaceY : 0f;
            for (int row = 0; row < cfg.rows; row++)
            {
                for (int col = 0; col < cfg.columns; col++)
                {
                    var coord = new GridCoord(col, row);
                    Vector3 pos = cfg.CellToWorld(coord) + new Vector3(0f, surfaceY + yOffset, 0f);
                    var go = Instantiate(cellPrefab, pos, Quaternion.Euler(90f, 0f, 0f), transform);
                    go.name = $"Cell_{col}_{row}";
                    if (go.TryGetComponent<Renderer>(out var r)) cells[coord] = r;
                }
            }
            Refresh();
        }

        /// <summary>Recolour every tile from the grid's current state.</summary>
        public void Refresh()
        {
            if (manager == null) return;
            foreach (var kv in cells)
            {
                var state = manager.Grid.GetState(kv.Key);
                // The enemy zone is "outside" the player zone but we still show it
                // (distinct colour) so the near/far sides read as separated.
                bool isEnemyZone = state == CellState.OutsideZone && manager.Config.InEnemyZone(kv.Key);
                bool hidden = state == CellState.OutsideZone && !isEnemyZone && !showOutsideZone;
                kv.Value.enabled = !hidden;
                kv.Value.material.color = isEnemyZone ? enemyZoneColor : ColorFor(state);
            }
        }

        private Color ColorFor(CellState state) => state switch
        {
            CellState.Empty => emptyColor,
            CellState.Occupied => occupiedColor,
            CellState.Blocked => blockedColor,
            CellState.Hazard => hazardColor,
            _ => outsideColor,
        };

        private void Clear()
        {
            foreach (var kv in cells)
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            cells.Clear();
        }

        private void OnDestroy() => Clear();
    }
}
