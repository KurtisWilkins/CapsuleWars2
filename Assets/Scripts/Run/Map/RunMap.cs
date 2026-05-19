using System.Collections.Generic;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// Ordered list of map nodes for a run. M7 is linear (next = index+1).
    /// </summary>
    public class RunMap
    {
        public List<MapNode> Nodes { get; }

        public RunMap(List<MapNode> nodes)
        {
            Nodes = nodes ?? new List<MapNode>();
        }

        public int Count => Nodes.Count;

        public MapNode Get(int index)
        {
            if (index < 0 || index >= Nodes.Count) return null;
            return Nodes[index];
        }
    }
}
