using System;
using System.Collections.Generic;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Runtime authority for deployment placement in the battle scene. Owns the
    /// <see cref="DeploymentGrid"/>, tracks the player's units by id, and snaps a
    /// unit's transform to a cell's world position when placed. Raises
    /// <see cref="OnPlacementsChanged"/> so a higher layer (UI/Run) can persist the
    /// arrangement. Combat-layer: no UI or Run dependency.
    ///
    /// Spawn-then-arrange: units already exist (BattlePartySpawner); register them,
    /// optionally seed the grid from their current positions, then let the player
    /// rearrange during PreBattle. Placement is restricted to the player deploy zone.
    /// </summary>
    [DisallowMultipleComponent]
    public class DeploymentManager : MonoBehaviour
    {
        [SerializeField] private DeploymentGridConfig config = new DeploymentGridConfig();

        private DeploymentGrid grid;
        private readonly Dictionary<string, UnitRoot> units = new Dictionary<string, UnitRoot>();

        public DeploymentGridConfig Config => config;

        /// <summary>The occupancy grid (lazily created, so it works without Awake — e.g. EditMode tests).</summary>
        public DeploymentGrid Grid => grid ??= new DeploymentGrid(config);

        /// <summary>Raised on any placement change (place / move / apply). Payload = current placements.</summary>
        public event Action<IReadOnlyDictionary<string, GridCoord>> OnPlacementsChanged;

        /// <summary>Register a player unit so it can be placed. Idempotent; ignores null/idless units.</summary>
        public void RegisterUnit(UnitRoot unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.UnitId)) return;
            units[unit.UnitId] = unit;
        }

        public bool TryGetUnit(string unitId, out UnitRoot unit) => units.TryGetValue(unitId, out unit);

        /// <summary>Current placements as unitId → cell (inverted from the grid's cell → occupant map).</summary>
        public IReadOnlyDictionary<string, GridCoord> GetPlacements()
        {
            var result = new Dictionary<string, GridCoord>(Grid.Occupants.Count);
            foreach (var kv in Grid.Occupants) result[kv.Value] = kv.Key;
            return result;
        }

        // -----------------------------------------------------------------
        // Token placement (place-then-spawn): occupy cells by unit id with NO
        // live unit, for the deployment bench. The UI mirrors these into
        // RunState.Placements so the spawner spawns each unit at its cell.
        // -----------------------------------------------------------------

        /// <summary>Place an occupant id on a deployable, empty cell (no transform). Raises OnPlacementsChanged on success.</summary>
        public bool PlaceToken(string unitId, GridCoord coord)
        {
            if (!Grid.TryPlace(coord, unitId)) return false;
            OnPlacementsChanged?.Invoke(GetPlacements());
            return true;
        }

        /// <summary>Remove an occupant id from the grid (back to the bench). Raises OnPlacementsChanged if it removed anything.</summary>
        public bool RemoveToken(string unitId)
        {
            if (!Grid.RemoveOccupant(unitId)) return false;
            OnPlacementsChanged?.Invoke(GetPlacements());
            return true;
        }

        /// <summary>Clear every placement. Raises OnPlacementsChanged.</summary>
        public void ClearAll()
        {
            Grid.Clear();
            OnPlacementsChanged?.Invoke(GetPlacements());
        }

        /// <summary>
        /// Place (or move) a registered unit onto a cell, snapping its transform to
        /// the cell's world position (preserving the unit's own ground height).
        /// Returns false (no change) if the unit isn't registered or the cell isn't
        /// deployable / is occupied by another unit.
        /// </summary>
        public bool PlaceUnit(string unitId, GridCoord coord)
        {
            if (string.IsNullOrEmpty(unitId)) return false;
            if (!units.TryGetValue(unitId, out var unit) || unit == null) return false;
            if (!Grid.TryPlace(coord, unitId)) return false;

            SnapToCell(unit, coord);
            OnPlacementsChanged?.Invoke(GetPlacements());
            return true;
        }

        /// <summary>Apply a set of saved placements (e.g. from RunState) in one batch.</summary>
        public void ApplyPlacements(IReadOnlyDictionary<string, GridCoord> saved)
        {
            if (saved == null) return;
            foreach (var kv in saved)
            {
                if (!units.TryGetValue(kv.Key, out var unit) || unit == null) continue;
                if (!Grid.TryPlace(kv.Value, kv.Key)) continue;
                SnapToCell(unit, kv.Value);
            }
            OnPlacementsChanged?.Invoke(GetPlacements());
        }

        /// <summary>
        /// Ensure every registered unit occupies a deployable cell so it can be
        /// selected and repositioned: a unit already over a free deployable cell
        /// keeps it, otherwise it's assigned the next free cell in the player zone.
        /// Units are snapped to their assigned cell. Used at deployment start when
        /// there's no saved arrangement to restore.
        /// </summary>
        public void AutoArrange()
        {
            foreach (var kv in units)
            {
                var unit = kv.Value;
                if (unit == null) continue;

                Grid.RemoveOccupant(kv.Key);   // clear any stale placement first

                var near = config.WorldToCell(unit.transform.position);
                GridCoord? target = Grid.IsDeployable(near) && !Grid.IsOccupied(near)
                    ? near
                    : FirstFreeCell();

                if (target.HasValue && Grid.TryPlace(target.Value, kv.Key))
                    SnapToCell(unit, target.Value);
            }
            OnPlacementsChanged?.Invoke(GetPlacements());
        }

        /// <summary>First free, deployable cell scanning the player zone row by row, or null if the zone is full.</summary>
        public GridCoord? FirstFreeCell()
        {
            for (int row = config.playerRowMin; row <= config.playerRowMax && row < config.rows; row++)
                for (int col = 0; col < config.columns; col++)
                {
                    var c = new GridCoord(col, row);
                    if (Grid.IsDeployable(c) && !Grid.IsOccupied(c)) return c;
                }
            return null;
        }

        private void SnapToCell(UnitRoot unit, GridCoord coord)
        {
            Vector3 p = config.CellToWorld(coord);
            p.y = unit.transform.position.y;   // keep the unit's own ground height
            unit.transform.position = p;
        }
    }
}
