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

        public RunState(RunMap map, int startingGold = 0)
        {
            Map = map;
            CurrentFloor = 0;
            Gold = startingGold;
            IsLost = false;
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
