using System.Collections.Generic;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// A branching run map: a graph of <see cref="MapNode"/>s arranged in rows, with
    /// directed edges from each node to nodes in the next row up. Nodes are stored in
    /// id order (a node's <see cref="MapNode.Index"/> equals its list position), so the
    /// map can grow upward indefinitely as new segments are stitched on.
    /// </summary>
    public class RunMap
    {
        public List<MapNode> Nodes { get; }

        public RunMap(List<MapNode> nodes)
        {
            Nodes = nodes ?? new List<MapNode>();
        }

        public int Count => Nodes.Count;

        /// <summary>Node by id. Fast path assumes id == list position; falls back to a scan.</summary>
        public MapNode Get(int id)
        {
            if (id < 0) return null;
            if (id < Nodes.Count && Nodes[id] != null && Nodes[id].Index == id) return Nodes[id];
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] != null && Nodes[i].Index == id) return Nodes[i];
            return null;
        }

        /// <summary>Highest row index present, or -1 for an empty map.</summary>
        public int TopRowIndex
        {
            get
            {
                int max = -1;
                for (int i = 0; i < Nodes.Count; i++)
                    if (Nodes[i] != null && Nodes[i].Row > max) max = Nodes[i].Row;
                return max;
            }
        }

        /// <summary>Number of rows (TopRowIndex + 1).</summary>
        public int RowCount => TopRowIndex + 1;

        public List<MapNode> NodesInRow(int row)
        {
            var result = new List<MapNode>();
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] != null && Nodes[i].Row == row) result.Add(Nodes[i]);
            return result;
        }

        public List<MapNode> BottomRow() => NodesInRow(0);
        public List<MapNode> TopRow() => NodesInRow(TopRowIndex);

        /// <summary>Whether a node sits in the current top row (a segment gate / stitch point).</summary>
        public bool IsTopRow(MapNode node) => node != null && node.Row == TopRowIndex;

        /// <summary>The nodes reachable by one move from the given node (its outgoing edges).</summary>
        public List<MapNode> Outgoing(MapNode node)
        {
            var result = new List<MapNode>();
            if (node == null) return result;
            for (int i = 0; i < node.Edges.Count; i++)
            {
                var n = Get(node.Edges[i]);
                if (n != null) result.Add(n);
            }
            return result;
        }

        /// <summary>Append already-built nodes (e.g. a freshly generated next segment).</summary>
        public void AppendNodes(IEnumerable<MapNode> nodes)
        {
            if (nodes == null) return;
            foreach (var n in nodes) if (n != null) Nodes.Add(n);
        }
    }
}
