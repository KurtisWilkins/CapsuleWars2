using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Authoring catalog of all spawnable <see cref="UnitDefinition_SO"/> assets.
    /// A single reference to this asset populates a <see cref="UnitDefinitionDatabase"/>
    /// at runtime (no Resources-folder scan), which the draft-into-run spawner uses
    /// to resolve a drafted unit's definition by id. Wire every draftable/spawnable
    /// definition here; each must have a unique <c>UnitId</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDefinitionCatalog", menuName = "CapsuleWars/Unit Definition Catalog", order = 2)]
    public class UnitDefinitionCatalog_SO : ScriptableObject
    {
        [Tooltip("All unit definitions that can be drafted or spawned. Each must have a unique UnitId.")]
        [SerializeField] private List<UnitDefinition_SO> definitions = new();

        public IReadOnlyList<UnitDefinition_SO> Definitions => definitions;

        /// <summary>Build an in-memory database keyed by <c>UnitId</c> from this catalog.</summary>
        public UnitDefinitionDatabase BuildDatabase() => new UnitDefinitionDatabase(definitions);
    }
}
