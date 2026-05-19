using System;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// One node on the run map. M7 is linear (no branching); the index
    /// determines order. M8+ will add a connections list for branching maps.
    /// </summary>
    [Serializable]
    public class MapNode
    {
        public int Index;
        public NodeType Type;
        public string DisplayLabel;
        public bool Visited;

        public MapNode(int index, NodeType type, string label)
        {
            Index = index;
            Type = type;
            DisplayLabel = label;
            Visited = false;
        }
    }
}
