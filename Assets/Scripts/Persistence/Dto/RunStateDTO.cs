using System;
using System.Collections.Generic;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>One serialized map node. Stores <c>NodeType</c> as an int so the
    /// Persistence assembly stays free of any reference to the Run assembly
    /// (the DTO &lt;-&gt; runtime mapping lives in Run, on <c>RunState</c>).
    /// <see cref="Row"/>/<see cref="Column"/>/<see cref="Edges"/> carry the branching
    /// graph (M8+); <see cref="Edges"/> holds outgoing node ids.</summary>
    [Serializable]
    public class MapNodeDTO
    {
        public int Index;
        public int Type;
        public string DisplayLabel;
        public bool Visited;
        public int Row;
        public int Column;
        public List<int> Edges = new List<int>();

        public MapNodeDTO() { }

        public MapNodeDTO(int index, int type, string displayLabel, bool visited)
        {
            Index = index;
            Type = type;
            DisplayLabel = displayLabel;
            Visited = visited;
        }

        public MapNodeDTO(int index, int type, string displayLabel, bool visited, int row, int column, List<int> edges)
        {
            Index = index;
            Type = type;
            DisplayLabel = displayLabel;
            Visited = visited;
            Row = row;
            Column = column;
            Edges = edges ?? new List<int>();
        }
    }

    /// <summary>One serialized deployment placement: which grid cell (col/row) a
    /// unit was last positioned on, so an arrangement persists between battles.
    /// Col/row are ints to keep Persistence free of any Combat (GridCoord) reference.</summary>
    [Serializable]
    public class UnitPlacementDTO
    {
        public string unitId;
        public int col;
        public int row;

        public UnitPlacementDTO() { }

        public UnitPlacementDTO(string unitId, int col, int row)
        {
            this.unitId = unitId;
            this.col = col;
            this.row = row;
        }
    }

    /// <summary>
    /// Serialized form of an in-progress run (Docs/12_RoguelikeRun.md,
    /// Docs/14_Persistence.md). Persisted to <c>run.json</c> via <c>RunStore</c>
    /// and reloaded to resume mid-run; carries the party as <see cref="UnitDTO"/>
    /// so run-scoped equipment/customization survives an app restart.
    /// No Unity or Run references — map nodes are flattened to <see cref="MapNodeDTO"/>.
    /// </summary>
    [Serializable]
    public class RunStateDTO
    {
        /// <summary>Save schema version. v2 = branching/infinite map (graph + seed + node id).</summary>
        public int SaveVersion = 2;

        /// <summary>Depth reached (row of the current node). Display + scoring + difficulty.</summary>
        public int CurrentFloor;
        public int Gold;
        public bool IsLost;
        public bool IsBossEncounter;
        public bool RewardsGranted;

        /// <summary>Per-run RNG seed (reproducible generation + segment stitching).</summary>
        public int Seed;
        /// <summary>Current graph position (node id); -1 = run not started (pick a bottom-row node).</summary>
        public int CurrentNodeId = -1;
        /// <summary>How many segments have been generated/stitched so far.</summary>
        public int SegmentIndex;

        /// <summary>The run map, flattened to a graph of nodes (with edges).</summary>
        public List<MapNodeDTO> Nodes = new List<MapNodeDTO>();

        /// <summary>The drafted party for this run (identity + run-scoped equipment).</summary>
        public List<UnitDTO> Party = new List<UnitDTO>();

        /// <summary>Roguelike-only units picked up this run; offered for recruitment at run end.</summary>
        public List<UnitDTO> Recruits = new List<UnitDTO>();

        /// <summary>The player's last deployment-grid placement per unit (spawn-then-arrange persistence).</summary>
        public List<UnitPlacementDTO> Placements = new List<UnitPlacementDTO>();
    }
}
