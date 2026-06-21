using System;
using System.Collections.Generic;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// One node in the branching run map (Slay-the-Spire style). <see cref="Index"/>
    /// is the node's unique id within the run; <see cref="Row"/> (0 = bottom) and
    /// <see cref="Column"/> place it for layout; <see cref="Edges"/> lists the ids of
    /// the nodes in the next row up that this node connects to. The player may only
    /// move along an outgoing edge from their current node.
    /// </summary>
    [Serializable]
    public class MapNode
    {
        /// <summary>Unique node id within the run map.</summary>
        public int Index;
        public NodeType Type;
        public string DisplayLabel;
        public bool Visited;

        /// <summary>Row in the map (0 = bottom / start). Higher = further up / later.</summary>
        public int Row;
        /// <summary>Column within the row, used for layout and adjacency-biased edges.</summary>
        public int Column;

        /// <summary>Ids of nodes in the next row up reachable from this node.</summary>
        public List<int> Edges = new List<int>();

        public MapNode(int index, NodeType type, string label)
        {
            Index = index;
            Type = type;
            DisplayLabel = label;
            Visited = false;
        }

        public MapNode(int index, int row, int column, NodeType type, string label)
        {
            Index = index;
            Row = row;
            Column = column;
            Type = type;
            DisplayLabel = label;
            Visited = false;
        }

        /// <summary>Add a one-way edge to a next-row node (id), ignoring duplicates.</summary>
        public void AddEdge(int toNodeId)
        {
            if (!Edges.Contains(toNodeId)) Edges.Add(toNodeId);
        }
    }
}
