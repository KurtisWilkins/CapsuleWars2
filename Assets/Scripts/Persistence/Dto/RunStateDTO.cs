using System;
using System.Collections.Generic;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>One serialized map node. Stores <c>NodeType</c> as an int so the
    /// Persistence assembly stays free of any reference to the Run assembly
    /// (the DTO &lt;-&gt; runtime mapping lives in Run, on <c>RunState</c>).</summary>
    [Serializable]
    public class MapNodeDTO
    {
        public int Index;
        public int Type;
        public string DisplayLabel;
        public bool Visited;

        public MapNodeDTO() { }

        public MapNodeDTO(int index, int type, string displayLabel, bool visited)
        {
            Index = index;
            Type = type;
            DisplayLabel = displayLabel;
            Visited = visited;
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
        /// <summary>Save schema version. Bump on breaking change.</summary>
        public int SaveVersion = 1;

        public int CurrentFloor;
        public int Gold;
        public bool IsLost;
        public bool IsBossEncounter;
        public bool RewardsGranted;

        /// <summary>The run map, flattened. Order matches node index (M7 linear map).</summary>
        public List<MapNodeDTO> Nodes = new List<MapNodeDTO>();

        /// <summary>The drafted party for this run (identity + run-scoped equipment).</summary>
        public List<UnitDTO> Party = new List<UnitDTO>();

        /// <summary>Roguelike-only units picked up this run; offered for recruitment at run end.</summary>
        public List<UnitDTO> Recruits = new List<UnitDTO>();

        /// <summary>The player's last deployment-grid placement per unit (spawn-then-arrange persistence).</summary>
        public List<UnitPlacementDTO> Placements = new List<UnitPlacementDTO>();
    }
}
