using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run.Map;

namespace CapsuleWars.Run
{
    /// <summary>
    /// In-memory state for one in-progress run. Owned by <see cref="RunSession"/>
    /// (static holder so state survives scene transitions between Map and Battle) and
    /// persisted to <c>run.json</c> via its DTO.
    ///
    /// The map is a branching graph (<see cref="RunMap"/>). Position is the current
    /// node id (<see cref="CurrentNodeId"/>; -1 before the player picks a start). The
    /// player may only travel along an outgoing edge of the current node (or, before
    /// starting, to any bottom-row node). The run is infinite — new segments stitch on
    /// when a top-row node is cleared — and ends only on loss.
    /// </summary>
    public class RunState
    {
        public RunMap Map { get; }

        /// <summary>Per-run RNG seed (reproducible generation + stitching).</summary>
        public int Seed { get; }

        /// <summary>Current graph position; -1 = not started (pick a bottom-row node).</summary>
        public int CurrentNodeId { get; private set; } = -1;

        /// <summary>How many segments have been generated/stitched (0 = just the first).</summary>
        public int SegmentIndex { get; private set; }

        /// <summary>Depth reached = the row of the current node. 0 before starting.</summary>
        public int CurrentFloor { get; private set; }

        public int Gold { get; private set; }
        public bool IsLost { get; set; }
        public bool IsBossEncounter { get; set; }
        public bool RewardsGranted { get; set; }

        /// <summary>Difficulty added per row of depth; <see cref="DifficultyMultiplier"/> = 1 + depth*this.</summary>
        public float DifficultyPerDepth { get; set; } = 0.05f;

        private readonly List<UnitDTO> party = new();
        public IReadOnlyList<UnitDTO> Party => party;

        private readonly List<UnitDTO> recruits = new();
        public IReadOnlyList<UnitDTO> Recruits => recruits;

        private readonly Dictionary<string, GridCoord> placements = new();
        public IReadOnlyDictionary<string, GridCoord> Placements => placements;

        private readonly List<UnitEquipmentDTO> inventory = new();
        /// <summary>Loose run-scoped items the player owns but hasn't equipped (combat/treasure drops; BTS-G).</summary>
        public IReadOnlyList<UnitEquipmentDTO> Inventory => inventory;

        public RunState(RunMap map, int startingGold = 0, int seed = 0)
        {
            Map = map;
            Gold = startingGold;
            Seed = seed;
            CurrentNodeId = -1;
            CurrentFloor = 0;
            IsLost = false;
        }

        // --- Party / recruits / placements (unchanged) ---

        public void SetParty(IEnumerable<UnitDTO> units)
        {
            party.Clear();
            if (units == null) return;
            foreach (var u in units) if (u != null) party.Add(u);
        }

        public void AddRecruit(UnitDTO unit) { if (unit != null) recruits.Add(unit); }
        public void RemoveRecruit(UnitDTO unit) { if (unit != null) recruits.Remove(unit); }

        public void AddItem(UnitEquipmentDTO item) { if (item != null) inventory.Add(item); }
        public void RemoveItem(UnitEquipmentDTO item) { if (item != null) inventory.Remove(item); }

        public void SetPlacement(string unitId, GridCoord coord)
        {
            if (!string.IsNullOrEmpty(unitId)) placements[unitId] = coord;
        }
        public void ClearPlacement(string unitId)
        {
            if (!string.IsNullOrEmpty(unitId)) placements.Remove(unitId);
        }
        public void ClearPlacements() => placements.Clear();

        public void AddGold(int amount) { if (amount > 0) Gold += amount; }
        public bool SpendGold(int amount)
        {
            if (amount <= 0 || amount > Gold) return false;
            Gold -= amount;
            return true;
        }

        // --- Graph navigation ---

        public bool HasStarted => CurrentNodeId >= 0;
        public MapNode CurrentNode => Map?.Get(CurrentNodeId);
        public bool IsBossNode => CurrentNode != null && CurrentNode.Type == NodeType.Boss;
        public bool IsAtTopRow => CurrentNode != null && Map != null && Map.IsTopRow(CurrentNode);

        /// <summary>Difficulty multiplier for the current depth (≥ 1).</summary>
        public float DifficultyMultiplier => 1f + CurrentFloor * DifficultyPerDepth;

        /// <summary>
        /// Node ids the player may move to right now: any bottom-row node before the
        /// run has started, otherwise the current node's outgoing edges.
        /// </summary>
        public List<int> ReachableNodeIds()
        {
            var result = new List<int>();
            if (Map == null) return result;
            if (!HasStarted)
            {
                foreach (var n in Map.BottomRow()) result.Add(n.Index);
            }
            else
            {
                foreach (var n in Map.Outgoing(CurrentNode)) result.Add(n.Index);
            }
            return result;
        }

        public bool IsReachable(int nodeId) => ReachableNodeIds().Contains(nodeId);

        /// <summary>
        /// Move to a reachable node (sets position + depth). Does NOT mark it cleared —
        /// call <see cref="MarkCurrentCleared"/> after its encounter resolves. Returns
        /// false (no change) if the node isn't currently reachable.
        /// </summary>
        public bool TravelTo(int nodeId)
        {
            if (!IsReachable(nodeId)) return false;
            var node = Map.Get(nodeId);
            if (node == null) return false;
            CurrentNodeId = nodeId;
            CurrentFloor = node.Row;
            return true;
        }

        /// <summary>Mark the current node visited (its encounter resolved/cleared).</summary>
        public void MarkCurrentCleared()
        {
            var node = CurrentNode;
            if (node != null) node.Visited = true;
        }

        /// <summary>
        /// Generate the next segment and stitch it onto the current top row (called when
        /// the player clears a top-row / boss node). Deterministic from <see cref="Seed"/>.
        /// </summary>
        public void AppendNextSegment(MapGenConfig config)
        {
            if (Map == null || CurrentNode == null) return;
            MapGenerator.AppendSegment(Map, config, Seed, ++SegmentIndex, CurrentNode);
        }

        // --- Persistence mapping (Run-side, so Persistence stays free of Run refs) ---

        public RunStateDTO ToDTO()
        {
            var dto = new RunStateDTO
            {
                CurrentFloor = CurrentFloor,
                Gold = Gold,
                IsLost = IsLost,
                IsBossEncounter = IsBossEncounter,
                RewardsGranted = RewardsGranted,
                Seed = Seed,
                CurrentNodeId = CurrentNodeId,
                SegmentIndex = SegmentIndex,
            };

            if (Map != null)
                foreach (var n in Map.Nodes)
                    if (n != null)
                        dto.Nodes.Add(new MapNodeDTO(n.Index, (int)n.Type, n.DisplayLabel, n.Visited,
                                                     n.Row, n.Column, new List<int>(n.Edges)));

            foreach (var u in party) if (u != null) dto.Party.Add(u);
            foreach (var u in recruits) if (u != null) dto.Recruits.Add(u);
            foreach (var kv in placements)
                dto.Placements.Add(new UnitPlacementDTO(kv.Key, kv.Value.col, kv.Value.row));
            foreach (var item in inventory) if (item != null) dto.Inventory.Add(item);
            return dto;
        }

        /// <summary>Rebuild a run from its DTO. Returns null for a null DTO or a pre-v2 (linear) save.</summary>
        public static RunState FromDTO(RunStateDTO dto)
        {
            if (dto == null || dto.SaveVersion < 2) return null;

            var nodes = new List<MapNode>();
            if (dto.Nodes != null)
                foreach (var n in dto.Nodes)
                    if (n != null)
                    {
                        var node = new MapNode(n.Index, n.Row, n.Column, (NodeType)n.Type, n.DisplayLabel)
                        {
                            Visited = n.Visited,
                            Edges = n.Edges != null ? new List<int>(n.Edges) : new List<int>(),
                        };
                        nodes.Add(node);
                    }

            var state = new RunState(new RunMap(nodes), dto.Gold, dto.Seed)
            {
                IsLost = dto.IsLost,
                IsBossEncounter = dto.IsBossEncounter,
                RewardsGranted = dto.RewardsGranted,
            };
            state.CurrentNodeId = dto.CurrentNodeId;
            state.SegmentIndex = dto.SegmentIndex;
            state.CurrentFloor = dto.CurrentFloor;

            if (dto.Party != null) state.SetParty(dto.Party);
            if (dto.Recruits != null)
                foreach (var u in dto.Recruits) state.AddRecruit(u);
            if (dto.Placements != null)
                foreach (var p in dto.Placements)
                    if (p != null) state.SetPlacement(p.unitId, new GridCoord(p.col, p.row));
            if (dto.Inventory != null)
                foreach (var item in dto.Inventory) state.AddItem(item);
            return state;
        }
    }
}
