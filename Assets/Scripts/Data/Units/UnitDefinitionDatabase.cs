using System.Collections.Generic;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// In-memory <see cref="IUnitDefinitionDatabase"/> backed by a dictionary
    /// keyed on <see cref="UnitDefinition_SO.UnitId"/>. Populate it from wherever
    /// definitions are sourced (a bootstrap that loads assets, Addressables, a
    /// manifest SO, etc.) — that asset-loading convention is deliberately out of
    /// scope here and left to the caller.
    /// </summary>
    public class UnitDefinitionDatabase : IUnitDefinitionDatabase
    {
        private readonly Dictionary<string, UnitDefinition_SO> byId = new();

        public UnitDefinitionDatabase() { }

        public UnitDefinitionDatabase(IEnumerable<UnitDefinition_SO> definitions)
        {
            if (definitions == null) return;
            foreach (var def in definitions) Register(def);
        }

        /// <summary>
        /// Add (or replace) a definition. Ignores nulls and definitions with no
        /// UnitId — those can't be looked up and would collide under "".
        /// </summary>
        public void Register(UnitDefinition_SO def)
        {
            if (def == null || string.IsNullOrEmpty(def.UnitId)) return;
            byId[def.UnitId] = def;
        }

        public UnitDefinition_SO GetUnitDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return byId.TryGetValue(id, out var def) ? def : null;
        }
    }
}
