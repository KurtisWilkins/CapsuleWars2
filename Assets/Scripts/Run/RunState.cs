using System.Collections.Generic;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run.Map;

namespace CapsuleWars.Run
{
    /// <summary>
    /// In-memory state for one in-progress run. Owned by <see cref="RunSession"/>
    /// (static holder so state survives scene transitions between Map and Battle).
    /// M8 promotes this to a serializable DTO with JSON persistence.
    /// </summary>
    public class RunState
    {
        public RunMap Map { get; }
        public int CurrentFloor { get; private set; }
        public int Gold { get; private set; }
        public bool IsLost { get; set; }
        public bool IsBossEncounter { get; set; }

        /// <summary>Set once unlock points have been awarded for this run, so the run-end flow grants them exactly once.</summary>
        public bool RewardsGranted { get; set; }

        /// <summary>
        /// The player's drafted party for this run, as serializable identity DTOs.
        /// Set at run start by the draft flow; read in the battle scene by
        /// BattlePartySpawner to spawn the player team. Empty until a draft occurs
        /// (battle scene then falls back to its scene-placed player units).
        /// </summary>
        private readonly List<UnitDTO> party = new();
        public IReadOnlyList<UnitDTO> Party => party;

        public RunState(RunMap map, int startingGold = 0)
        {
            Map = map;
            CurrentFloor = 0;
            Gold = startingGold;
            IsLost = false;
        }

        /// <summary>Replace the drafted party (nulls are dropped). Called by the draft flow.</summary>
        public void SetParty(IEnumerable<UnitDTO> units)
        {
            party.Clear();
            if (units == null) return;
            foreach (var u in units)
                if (u != null) party.Add(u);
        }

        /// <summary>
        /// Roguelike-only units picked up during the run (combat/elite drops).
        /// Offered for legacy recruitment at run end on a win (Docs/13_LegacyMode.md).
        /// Discarded on a loss. Distinct from <see cref="Party"/> (the drafted
        /// legacy units).
        /// </summary>
        private readonly List<UnitDTO> recruits = new();
        public IReadOnlyList<UnitDTO> Recruits => recruits;

        public void AddRecruit(UnitDTO unit)
        {
            if (unit != null) recruits.Add(unit);
        }

        public void RemoveRecruit(UnitDTO unit)
        {
            if (unit != null) recruits.Remove(unit);
        }

        public MapNode CurrentNode => Map?.Get(CurrentFloor);
        public bool IsComplete => Map != null && CurrentFloor >= Map.Count;
        public bool IsBossFloor => CurrentNode != null && CurrentNode.Type == NodeType.Boss;

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || amount > Gold) return false;
            Gold -= amount;
            return true;
        }

        public void AdvanceNode()
        {
            var node = CurrentNode;
            if (node != null) node.Visited = true;
            CurrentFloor++;
        }

        // -----------------------------------------------------------------
        // Persistence mapping (lives here, in Run, so it can see both the
        // RunMap/NodeType runtime types and the Persistence DTOs — keeping the
        // Persistence assembly free of any reference to Run).
        // -----------------------------------------------------------------

        /// <summary>Flatten this run into its serializable DTO.</summary>
        public RunStateDTO ToDTO()
        {
            var dto = new RunStateDTO
            {
                CurrentFloor = CurrentFloor,
                Gold = Gold,
                IsLost = IsLost,
                IsBossEncounter = IsBossEncounter,
                RewardsGranted = RewardsGranted,
            };

            if (Map != null)
                foreach (var n in Map.Nodes)
                    if (n != null)
                        dto.Nodes.Add(new MapNodeDTO(n.Index, (int)n.Type, n.DisplayLabel, n.Visited));

            foreach (var u in party) if (u != null) dto.Party.Add(u);
            foreach (var u in recruits) if (u != null) dto.Recruits.Add(u);
            return dto;
        }

        /// <summary>Rebuild a run from its DTO. Returns null for a null DTO.</summary>
        public static RunState FromDTO(RunStateDTO dto)
        {
            if (dto == null) return null;

            var nodes = new List<MapNode>();
            if (dto.Nodes != null)
                foreach (var n in dto.Nodes)
                    if (n != null)
                        nodes.Add(new MapNode(n.Index, (NodeType)n.Type, n.DisplayLabel) { Visited = n.Visited });

            var state = new RunState(new RunMap(nodes), dto.Gold)
            {
                IsLost = dto.IsLost,
                IsBossEncounter = dto.IsBossEncounter,
                RewardsGranted = dto.RewardsGranted,
            };
            state.CurrentFloor = dto.CurrentFloor;   // private set — accessible within the class
            if (dto.Party != null) state.SetParty(dto.Party);
            if (dto.Recruits != null)
                foreach (var u in dto.Recruits) state.AddRecruit(u);
            return state;
        }
    }
}
