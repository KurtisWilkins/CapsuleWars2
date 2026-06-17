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
        private readonly HashSet<GridCoord> blocked = new HashSet<GridCoord>();

        public DeploymentGrid(DeploymentGridConfig config)
        {
            this.config = config ?? new DeploymentGridConfig();
        }

        public DeploymentGridConfig Config => config;

        /// <summary>Current cell → occupant-id placements.</summary>
        public IReadOnlyDictionary<GridCoord, string> Occupants => occupants;

        public void SetBlocked(GridCoord c, bool isBlocked)
        {
            if (isBlocked) blocked.Add(c);
            else blocked.Remove(c);
        }

        public bool IsBlocked(GridCoord c) => blocked.Contains(c);
        public bool IsOccupied(GridCoord c) => occupants.ContainsKey(c);

        /// <summary>A cell the player can place on: in the player zone and not blocked.</summary>
        public bool IsDeployable(GridCoord c) => config.InPlayerZone(c) && !IsBlocked(c);

        public CellState GetState(GridCoord c)
        {
            if (!config.InBounds(c)) return CellState.OutOfBounds;
            if (IsBlocked(c)) return CellState.Blocked;
            if (IsOccupied(c)) return CellState.Occupied;
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
