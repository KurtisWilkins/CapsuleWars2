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

        /// <summary>Seed the grid from each registered unit's current position (nearest cell).</summary>
        public void SeedFromCurrentPositions()
        {
            foreach (var kv in units)
            {
                if (kv.Value == null) continue;
                Grid.TryPlace(config.WorldToCell(kv.Value.transform.position), kv.Key);
            }
            OnPlacementsChanged?.Invoke(GetPlacements());
        }

        private void SnapToCell(UnitRoot unit, GridCoord coord)
        {
            Vector3 p = config.CellToWorld(coord);
            p.y = unit.transform.position.y;   // keep the unit's own ground height
            unit.transform.position = p;
        }
    }
}
