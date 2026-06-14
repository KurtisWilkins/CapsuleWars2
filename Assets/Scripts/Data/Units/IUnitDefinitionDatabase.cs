namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Resolves <see cref="UnitDefinition_SO"/> assets by their stable
    /// <see cref="UnitDefinition_SO.UnitId"/>. This is the minimal slice of the
    /// "Database" service described in Docs/14_Persistence.md — the full
    /// <c>Database.GetById&lt;T&gt;(id)</c> across all SO types is a later task,
    /// blocked on those SOs gaining stable IDs (today only UnitDefinition_SO
    /// has one). A broader database can implement this interface so callers
    /// (e.g. UnitFactory) don't have to change.
    /// </summary>
    public interface IUnitDefinitionDatabase
    {
        /// <summary>Returns the definition for <paramref name="id"/>, or null if unknown.</summary>
        UnitDefinition_SO GetUnitDefinition(string id);
    }
}
