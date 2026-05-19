using System.Collections.Generic;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// Generates a run map. M7 ships a fixed 5-floor linear template:
    /// Combat → Event → Shop → Combat → Boss.
    /// M8+ will support branching graphs with weighted node types per floor.
    /// </summary>
    public static class MapGenerator
    {
        public static RunMap GenerateDefault()
        {
            var nodes = new List<MapNode>
            {
                new MapNode(0, NodeType.Combat,   "Floor 1: Combat"),
                new MapNode(1, NodeType.Event,    "Floor 2: Event"),
                new MapNode(2, NodeType.Shop,     "Floor 3: Shop"),
                new MapNode(3, NodeType.Combat,   "Floor 4: Combat"),
                new MapNode(4, NodeType.Boss,     "Floor 5: Boss"),
            };
            return new RunMap(nodes);
        }

        /// <summary>
        /// Generate a custom-length map. Pattern: alternating Combat with
        /// non-combat filler, last floor is always Boss.
        /// </summary>
        public static RunMap Generate(int totalFloors)
        {
            if (totalFloors < 2) totalFloors = 2;
            var nodes = new List<MapNode>();
            for (int i = 0; i < totalFloors - 1; i++)
            {
                NodeType type;
                if (i % 4 == 1) type = NodeType.Event;
                else if (i % 4 == 2) type = NodeType.Shop;
                else if (i % 4 == 3) type = NodeType.Rest;
                else type = NodeType.Combat;

                nodes.Add(new MapNode(i, type, $"Floor {i + 1}: {type}"));
            }
            nodes.Add(new MapNode(totalFloors - 1, NodeType.Boss, $"Floor {totalFloors}: Boss"));
            return new RunMap(nodes);
        }
    }
}
