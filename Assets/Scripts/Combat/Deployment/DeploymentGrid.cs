using System.Collections.Generic;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Runtime occupancy model for the deployment grid. Tracks which cells are
    /// blocked and which hold a unit (keyed by a stable occupant id, e.g.
    /// UnitRoot.UnitId), and validates placement against the config's player
    /// deploy zone. Pure model — no Unity scene dependencies — so it's fully
    /// unit-testable; the UI layer renders it and the spawner reads its
    /// placements to position units at battle start.
    /// </summary>
    public class DeploymentGrid
    {
        private readonly DeploymentGridConfig config;
        private readonly Dictionary<GridCoord, string> occupants = new Dictionary<GridCoord, string>();
        private readonly Dictionary<GridCoord, TerrainType> terrain = new Dictionary<GridCoord, TerrainType>();

        public DeploymentGrid(DeploymentGridConfig config)
        {
            this.config = config ?? new DeploymentGridConfig();
        }

        public DeploymentGridConfig Config => config;

        /// <summary>Current cell → occupant-id placements.</summary>
        public IReadOnlyDictionary<GridCoord, string> Occupants => occupants;

        // --- Terrain (generalizes the old binary "blocked" flag; blocked == Impassable) ---

        /// <summary>Set a cell's terrain. Passable removes the entry (sparse map: absent = Passable).</summary>
        public void SetTerrain(GridCoord c, TerrainType type)
        {
            if (type == TerrainType.Passable) terrain.Remove(c);
            else terrain[c] = type;
        }

        public TerrainType GetTerrain(GridCoord c) => terrain.TryGetValue(c, out var t) ? t : TerrainType.Passable;
        public bool IsImpassable(GridCoord c) => GetTerrain(c) == TerrainType.Impassable;
        public bool IsHazard(GridCoord c) => GetTerrain(c) == TerrainType.Hazard;

        /// <summary>Non-Passable cells only (obstacles + hazards) — for Slice B theming + the NavMesh carver.</summary>
        public IReadOnlyDictionary<GridCoord, TerrainType> TerrainCells => terrain;

        // Back-compat: "blocked" is Impassable terrain, so existing callers/tests keep working.
        public void SetBlocked(GridCoord c, bool isBlocked)
        {
            if (isBlocked) SetTerrain(c, TerrainType.Impassable);
            else if (IsImpassable(c)) SetTerrain(c, TerrainType.Passable);   // clear only impassable, leave a hazard
        }

        public bool IsBlocked(GridCoord c) => IsImpassable(c);
        public bool IsOccupied(GridCoord c) => occupants.ContainsKey(c);

        /// <summary>
        /// A cell the player can place on: in the player zone, not Impassable, and — unless
        /// <see cref="DeploymentGridConfig.allowPlaceOnHazard"/> is set — not a Hazard.
        /// </summary>
        public bool IsDeployable(GridCoord c) =>
            config.InPlayerZone(c) && !IsImpassable(c) && (config.allowPlaceOnHazard || !IsHazard(c));

        public CellState GetState(GridCoord c)
        {
            if (!config.InBounds(c)) return CellState.OutOfBounds;
            if (IsImpassable(c)) return CellState.Blocked;
            if (IsOccupied(c)) return CellState.Occupied;
            if (IsHazard(c)) return CellState.Hazard;            // hazards read everywhere (like Blocked)
            if (!config.InPlayerZone(c)) return CellState.OutsideZone;
            return CellState.Empty;
        }

        /// <summary>
        /// Place an occupant on a deployable, empty cell. An occupant lives in at
        /// most one cell, so any prior placement of the same id is cleared first.
        /// Returns false (no change) for an empty id or a non-deployable/occupied cell.
        /// </summary>
        public bool TryPlace(GridCoord c, string occupantId)
        {
            if (string.IsNullOrEmpty(occupantId)) return false;
            if (!IsDeployable(c) || IsOccupied(c)) return false;

            RemoveOccupant(occupantId);
            occupants[c] = occupantId;
            return true;
        }

        /// <summary>Move the occupant at <paramref name="from"/> to <paramref name="to"/> if the target is deployable and empty.</summary>
        public bool TryMove(GridCoord from, GridCoord to)
        {
            if (from == to) return false;
            if (!occupants.TryGetValue(from, out var id)) return false;
            if (!IsDeployable(to) || IsOccupied(to)) return false;

            occupants.Remove(from);
            occupants[to] = id;
            return true;
        }

        public bool RemoveAt(GridCoord c) => occupants.Remove(c);

        /// <summary>Remove an occupant by id wherever it sits. Returns true if found.</summary>
        public bool RemoveOccupant(string occupantId)
        {
            if (string.IsNullOrEmpty(occupantId)) return false;
            foreach (var kv in occupants)
            {
                if (kv.Value == occupantId)
                {
                    occupants.Remove(kv.Key);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetOccupant(GridCoord c, out string occupantId) => occupants.TryGetValue(c, out occupantId);

        public void Clear() => occupants.Clear();
    }
}
