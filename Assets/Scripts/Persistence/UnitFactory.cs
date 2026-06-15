using CapsuleWars.Data.Units;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using UnityEngine;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Conversion layer between <see cref="UnitDTO"/> (serialized) and the
    /// runtime unit (<see cref="UnitRoot"/> + controllers) — the
    /// <c>UnitFactory</c> referenced by Docs/02_UnitSystem.md.
    ///
    /// Lives in CapsuleWars.Persistence because that's the only assembly that
    /// already sees all three layers it bridges (Persistence DTOs, Data SOs,
    /// Units runtime); the dependency direction Persistence -> Units/Data is
    /// pre-existing.
    ///
    /// SCOPE: per the M8 keystone decision <see cref="FromDTO"/> configures an
    /// ALREADY-SPAWNED, caller-provided unit rather than instantiating one —
    /// there is no DTO-driven spawn path yet (units are scene-placed; the battle
    /// scene registers prefab-placed units via BattleContext). It therefore takes
    /// the target unit and returns it, matching the doc's <c>FromDTO(dto) -> Unit</c>
    /// (Docs/02_UnitSystem.md) in spirit; a true instantiating overload that
    /// resolves a unit prefab lands with the spawn-from-DTO work.
    /// <see cref="FromUnit"/> is the reverse capture (the doc's
    /// <c>UnitDTO.FromUnit(unit)</c>), kept here rather than on the DTO so UnitDTO
    /// stays free of runtime (Units) references.
    /// </summary>
    public static class UnitFactory
    {
        /// <summary>
        /// Capture a runtime unit's serializable identity into a new DTO
        /// (the doc's <c>UnitDTO.FromUnit</c>). Returns null if
        /// <paramref name="unit"/> is null.
        /// </summary>
        public static UnitDTO FromUnit(UnitRoot unit)
        {
            if (unit == null) return null;

            var dto = new UnitDTO
            {
                Id = unit.UnitId,
                DisplayName = unit.DisplayName,
            };

            if (unit.TryGetComponent<UnitCustomization>(out var custom) && custom.Definition != null)
                dto.UnitDefinitionId = custom.Definition.UnitId;

            return dto;
        }

        /// <summary>
        /// Configure a caller-provided runtime unit from a DTO (the doc's
        /// <c>FromDTO(dto) -> Unit</c>): sets identity and applies the referenced
        /// <c>UnitDefinition_SO</c> (resolved via <paramref name="database"/>) to
        /// the unit's <see cref="UnitCustomization"/>. Returns the same
        /// <paramref name="unit"/> for fluent use (or null/the unit unchanged when
        /// dto or unit is null). If the database is null or the definition ID is
        /// unknown, identity is still applied and visuals are left unchanged.
        /// </summary>
        public static UnitRoot FromDTO(UnitDTO dto, UnitRoot unit, IUnitDefinitionDatabase database)
        {
            if (dto == null || unit == null) return unit;

            unit.SetIdentity(dto.Id, dto.DisplayName);

            if (database == null || string.IsNullOrEmpty(dto.UnitDefinitionId)) return unit;

            var def = database.GetUnitDefinition(dto.UnitDefinitionId);
            if (def == null)
            {
                Debug.LogWarning(
                    $"UnitFactory: no UnitDefinition for UnitDefinitionId '{dto.UnitDefinitionId}' " +
                    $"(unit '{dto.Id}'); visuals left unchanged.");
                return unit;
            }

            if (unit.TryGetComponent<UnitCustomization>(out var custom))
                custom.Apply(def);

            return unit;
        }

        /// <summary>
        /// Instantiate <paramref name="prefab"/> and configure the new instance
        /// from <paramref name="dto"/> via <see cref="FromDTO"/> — the
        /// instantiating counterpart of the doc's spawn-from-DTO path. Returns
        /// the spawned <see cref="UnitRoot"/>, or null if <paramref name="prefab"/>
        /// is null. Team assignment, registration with the battle registry, and
        /// any post-spawn wiring stay the caller's job (e.g. the run's battle
        /// party spawner) — this method only creates and configures the instance.
        /// </summary>
        public static UnitRoot Spawn(UnitDTO dto, UnitRoot prefab, IUnitDefinitionDatabase database,
                                     Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("UnitFactory.Spawn: null prefab; nothing spawned.");
                return null;
            }

            var unit = Object.Instantiate(prefab, position, rotation, parent);
            FromDTO(dto, unit, database);
            return unit;
        }
    }
}
