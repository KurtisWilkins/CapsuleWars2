using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Battle-scene component that turns the run's drafted party
    /// (<see cref="RunState.Party"/>) into the player team.
    ///
    /// On Awake (ahead of BattleStateManager's default-order registration sweep)
    /// it retires the scene-placed placeholder player units and instantiates one
    /// unit per drafted DTO from <see cref="baseUnitPrefab"/>, configuring each via
    /// <see cref="UnitFactory.Spawn"/> (identity + visuals from its
    /// <c>UnitDefinition</c>). The base prefab is authored as Team.Player, so
    /// spawned units join the player team; BattleStateManager's sweep registers
    /// them, and their <c>UnitId</c> (= the legacy unit id) keeps equipment
    /// drain and legacy stat flow-back working.
    ///
    /// No-ops when there's no active run or the party is empty, leaving the
    /// scene's own player units in place so the battle scene stays playable
    /// standalone.
    ///
    /// Spawn-on-place: when a deployment phase is present, units are NOT spawned here —
    /// the deployment UI calls <see cref="SpawnOrMoveAt"/> as the player places each
    /// unit (so they're visible during setup) and those same instances carry into combat
    /// when Assemble flips the phase to Active (no double-spawn). The immediate
    /// <see cref="SpawnParty"/> path is the standalone fallback when no deployment phase exists.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public class BattlePartySpawner : MonoBehaviour
    {
        [Tooltip("Base unit prefab instantiated per drafted party member (Unit_Sample_Prefab). Must be Team.Player.")]
        [SerializeField] private UnitRoot baseUnitPrefab;

        [Tooltip("Catalog used to resolve each party member's UnitDefinition by id.")]
        [SerializeField] private UnitDefinitionCatalog_SO definitionCatalog;

        [Tooltip("Part catalog used to resolve a party member's explicit parts (generated/recruited units). Optional.")]
        [SerializeField] private PartCatalog_SO partCatalog;

        [Tooltip("Spawn transforms for player units, one per party slot. Extra party members fall back to spacing offsets.")]
        [SerializeField] private Transform[] playerSpawnPoints;

        [Tooltip("X-spacing used when there are more party members than spawn points.")]
        [SerializeField, Min(0.1f)] private float fallbackSpacing = 2f;

        [Tooltip("Grid config for spawn-then-arrange: a party member with a saved placement " +
                 "(RunState.Placements) spawns at that cell's world position. Keep in sync with the deployment UI grid.")]
        [SerializeField] private DeploymentGridConfig deploymentGrid = new DeploymentGridConfig();

        private DeploymentPhaseController deployment;

        // Live deployment-preview instances, keyed by unit id; these become the combat units.
        private readonly Dictionary<string, UnitRoot> placed = new Dictionary<string, UnitRoot>();
        private Transform deployedContainer;
        private UnitDefinitionDatabase cachedDatabase;
        private bool databaseBuilt;

        private void Awake()
        {
            if (!RunSession.IsActive) return;
            var party = RunSession.Current.Party;
            if (party == null || party.Count == 0) return;

            if (baseUnitPrefab == null)
            {
                Debug.LogWarning("[BattlePartySpawner] No base unit prefab assigned; using scene player units.", this);
                return;
            }

            // Clear scene placeholder player units now; in deployment mode the field
            // starts empty and the party spawns as it's placed.
            RetireScenePlayerUnits();

            // Spawn-on-place: with a deployment phase, the deployment UI spawns each unit
            // via SpawnOrMoveAt as it's placed, and those instances carry into combat on
            // Assemble (no spawn here). Without a deployment phase, spawn immediately so the
            // battle scene stays playable standalone.
            deployment = FindAnyObjectByType<DeploymentPhaseController>();
            if (deployment == null)
                SpawnParty();
        }

        private void SpawnParty()
        {
            if (!RunSession.IsActive) return;
            var party = RunSession.Current.Party;
            if (party == null || party.Count == 0 || baseUnitPrefab == null) return;

            var database = definitionCatalog != null ? definitionCatalog.BuildDatabase() : null;
            if (definitionCatalog == null)
                Debug.LogWarning("[BattlePartySpawner] No definition catalog assigned; draftees keep base-prefab visuals.", this);

            for (int i = 0; i < party.Count; i++)
            {
                UnitFactory.Spawn(party[i], baseUnitPrefab, database, SpawnPosition(i, party[i]?.Id), SpawnRotation(i),
                                  parent: null, partDatabase: partCatalog);
            }
        }

        // -----------------------------------------------------------------
        // Deployment preview API (driven by the deployment UI as units are placed).
        // -----------------------------------------------------------------

        /// <summary>
        /// Spawn the drafted unit with this id at the cell, or move its existing instance
        /// there. The instance is the real unit, but spawned during PreBattle its combat
        /// controllers stay idle (they gate on Phase == Active) until Assemble — so it reads
        /// as a placed "preview" and then simply joins combat. Returns false if the id isn't
        /// in the party.
        /// </summary>
        public bool SpawnOrMoveAt(string unitId, GridCoord cell)
        {
            if (string.IsNullOrEmpty(unitId) || baseUnitPrefab == null || !RunSession.IsActive) return false;

            Vector3 pos = deploymentGrid != null ? deploymentGrid.CellToWorld(cell) : transform.position;

            if (placed.TryGetValue(unitId, out var existing) && existing != null)
            {
                pos.y = existing.transform.position.y;   // keep its own ground height
                existing.transform.position = pos;
                return true;
            }

            var dto = FindPartyMember(unitId);
            if (dto == null) return false;

            var unit = UnitFactory.Spawn(dto, baseUnitPrefab, Database(), pos, Quaternion.identity,
                                         parent: DeployedContainer(), partDatabase: partCatalog);
            if (unit == null) return false;
            placed[unitId] = unit;
            return true;
        }

        /// <summary>Destroy the placed instance for this id (sent back to the bench). No-op if not placed.</summary>
        public bool Despawn(string unitId)
        {
            if (!placed.TryGetValue(unitId, out var unit)) return false;
            placed.Remove(unitId);
            DestroyInstance(unit);
            return true;
        }

        /// <summary>Destroy every placed instance.</summary>
        public void DespawnAll()
        {
            foreach (var kv in placed) DestroyInstance(kv.Value);
            placed.Clear();
        }

        private static void DestroyInstance(UnitRoot unit)
        {
            if (unit == null) return;
            // Deactivate before destroy so a registration sweep can't grab a dying unit.
            unit.gameObject.SetActive(false);
            Destroy(unit.gameObject);
        }

        private UnitDTO FindPartyMember(string unitId)
        {
            var party = RunSession.Current.Party;
            if (party == null) return null;
            for (int i = 0; i < party.Count; i++)
                if (party[i] != null && party[i].Id == unitId) return party[i];
            return null;
        }

        private UnitDefinitionDatabase Database()
        {
            if (!databaseBuilt)
            {
                cachedDatabase = definitionCatalog != null ? definitionCatalog.BuildDatabase() : null;
                if (definitionCatalog == null)
                    Debug.LogWarning("[BattlePartySpawner] No definition catalog assigned; draftees keep base-prefab visuals.", this);
                databaseBuilt = true;
            }
            return cachedDatabase;
        }

        private Transform DeployedContainer()
        {
            if (deployedContainer == null) deployedContainer = new GameObject("DeployedUnits").transform;
            return deployedContainer;
        }

        private void RetireScenePlayerUnits()
        {
            var roots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
            {
                var r = roots[i];
                if (r != null && r.Team == Team.Player)
                {
                    // Deactivate BEFORE destroying: Destroy() is deferred to end of
                    // frame, so BattleStateManager.Start's FindObjectsByType sweep
                    // (which excludes inactive) would otherwise register a unit that's
                    // about to be destroyed, leaving a dangling ref in the registry.
                    r.gameObject.SetActive(false);
                    Destroy(r.gameObject);
                }
            }
        }

        private Vector3 SpawnPosition(int index, string unitId)
        {
            // Spawn-then-arrange: a saved deployment placement for this unit wins.
            if (deploymentGrid != null && !string.IsNullOrEmpty(unitId)
                && RunSession.IsActive
                && RunSession.Current.Placements.TryGetValue(unitId, out var coord))
                return deploymentGrid.CellToWorld(coord);

            if (playerSpawnPoints != null && index < playerSpawnPoints.Length && playerSpawnPoints[index] != null)
                return playerSpawnPoints[index].position;

            // Fallback: offset from the last valid spawn point (or this object) along X.
            Vector3 origin = transform.position;
            if (playerSpawnPoints != null && playerSpawnPoints.Length > 0 && playerSpawnPoints[0] != null)
                origin = playerSpawnPoints[0].position;
            return origin + new Vector3(index * fallbackSpacing, 0f, 0f);
        }

        private Quaternion SpawnRotation(int index)
        {
            if (playerSpawnPoints != null && index < playerSpawnPoints.Length && playerSpawnPoints[index] != null)
                return playerSpawnPoints[index].rotation;
            return Quaternion.identity;
        }
    }
}
