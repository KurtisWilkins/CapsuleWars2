using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
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
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public class BattlePartySpawner : MonoBehaviour
    {
        [Tooltip("Base unit prefab instantiated per drafted party member (Unit_Sample_Prefab). Must be Team.Player.")]
        [SerializeField] private UnitRoot baseUnitPrefab;

        [Tooltip("Catalog used to resolve each party member's UnitDefinition by id.")]
        [SerializeField] private UnitDefinitionCatalog_SO definitionCatalog;

        [Tooltip("Spawn transforms for player units, one per party slot. Extra party members fall back to spacing offsets.")]
        [SerializeField] private Transform[] playerSpawnPoints;

        [Tooltip("X-spacing used when there are more party members than spawn points.")]
        [SerializeField, Min(0.1f)] private float fallbackSpacing = 2f;

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

            var database = definitionCatalog != null ? definitionCatalog.BuildDatabase() : null;
            if (definitionCatalog == null)
                Debug.LogWarning("[BattlePartySpawner] No definition catalog assigned; draftees keep base-prefab visuals.", this);

            // Retire scene placeholders BEFORE spawning so we don't remove our own units.
            RetireScenePlayerUnits();

            for (int i = 0; i < party.Count; i++)
            {
                UnitFactory.Spawn(party[i], baseUnitPrefab, database, SpawnPosition(i), SpawnRotation(i));
            }
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

        private Vector3 SpawnPosition(int index)
        {
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
