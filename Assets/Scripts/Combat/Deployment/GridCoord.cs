using System;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// A column/row address on the deployment grid. Value type with value
    /// equality so it can key dictionaries and be compared directly.
    /// </summary>
    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int col;
        public int row;

        public GridCoord(int col, int row)
        {
            this.col = col;
            this.row = row;
        }

        public bool Equals(GridCoord other) => col == other.col && row == other.row;
        public override bool Equals(object obj) => obj is GridCoord o && Equals(o);
        public override int GetHashCode() => unchecked((col * 397) ^ row);
        public override string ToString() => $"({col},{row})";

        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
    }
}
