using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>One authored terrain cell: a coord + its <see cref="TerrainType"/>.</summary>
    [Serializable]
    public struct TerrainCell
    {
        public GridCoord coord;
        public TerrainType type;

        public TerrainCell(GridCoord coord, TerrainType type)
        {
            this.coord = coord;
            this.type = type;
        }
    }

    /// <summary>
    /// A hand-authored (or, later, generated) set of non-Passable terrain cells for a board.
    /// Pure, serializable data with no scene deps: <see cref="ApplyTo"/> stamps it onto a
    /// <see cref="DeploymentGrid"/> before deployment. Slice B (theming) reads <see cref="Cells"/>
    /// to place themed props; Slice C (encounter builder) generates one per encounter and reads it
    /// to place enemies around the obstacles. Authored inline on <c>DeploymentManager</c> for now.
    /// </summary>
    [Serializable]
    public class TerrainLayout
    {
        [Tooltip("Obstacle/hazard cells stamped onto the grid at startup. Cells not listed are Passable.")]
        [SerializeField] private List<TerrainCell> cells = new List<TerrainCell>();

        public IReadOnlyList<TerrainCell> Cells => cells;

        /// <summary>Add a terrain cell to the layout (e.g. for generation/tests).</summary>
        public void Add(GridCoord coord, TerrainType type) => cells.Add(new TerrainCell(coord, type));

        /// <summary>Stamp every listed cell onto the grid.</summary>
        public void ApplyTo(DeploymentGrid grid)
        {
            if (grid == null) return;
            for (int i = 0; i < cells.Count; i++)
                grid.SetTerrain(cells[i].coord, cells[i].type);
        }
    }
}
