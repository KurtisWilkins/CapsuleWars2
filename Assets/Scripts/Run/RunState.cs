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
    }
}
