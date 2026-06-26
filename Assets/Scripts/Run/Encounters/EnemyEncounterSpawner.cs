using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Run.Encounters
{
    /// <summary>
    /// Spawns a generated, obstacle-aware enemy roster for a combat node (Slice C, iteration C2+C3). On Awake —
    /// after <see cref="EncounterBuilder"/> has stamped the terrain (lower execution order) so placement can see
    /// the obstacles, and before the player spawner — it reads the run, asks <see cref="EncounterGenerator"/> how
    /// many enemies and which enemy-zone cells (Passable, avoiding Impassable), retires the scene's placeholder
    /// enemies, and spawns the roster from <see cref="enemyPrefab"/> (a Team.Enemy base prefab) via
    /// <see cref="UnitFactory.Spawn"/>. Mirrors <c>BattlePartySpawner</c>'s run-gated retire-then-spawn pattern.
    ///
    /// v1 spawns clones of the base enemy (visual variety via the part generator is a later pass). With no active
    /// run it leaves the scene's authored enemies in place, so the battle scene stays playable standalone.
    /// </summary>
    [DefaultExecutionOrder(-75)]
    [DisallowMultipleComponent]
    public class EnemyEncounterSpawner : MonoBehaviour
    {
        [SerializeField] private DeploymentManager manager;
        [SerializeField] private EncounterDefinition definition;
        [Tooltip("Team.Enemy base prefab cloned per generated enemy (Unit_Enemy.prefab).")]
        [SerializeField] private UnitRoot enemyPrefab;
        [Tooltip("Only generate enemies when a run is active; otherwise keep the scene's authored enemies.")]
        [SerializeField] private bool requireActiveRun = true;
        [SerializeField] private int fallbackSeed = 12345;

        private void Awake()
        {
            if (requireActiveRun && !RunSession.IsActive) return;
            if (manager == null || definition == null || enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyEncounterSpawner] Missing manager/definition/enemyPrefab; leaving scene enemies.", this);
                return;
            }

            int seed, nodeId, floor; bool boss;
            if (RunSession.IsActive)
            {
                var s = RunSession.Current;
                seed = s.Seed; nodeId = s.CurrentNodeId; floor = s.CurrentFloor; boss = s.IsBossEncounter;
            }
            else { seed = fallbackSeed; nodeId = 0; floor = 0; boss = false; }

            int count = EncounterGenerator.RosterSize(definition, seed, nodeId, floor, boss);
            if (count <= 0) return;

            var cells = EncounterGenerator.EnemyCells(manager.Config, manager.Grid, count, seed, nodeId);
            if (cells.Count == 0) return;

            RetireSceneEnemies();

            // Face the player side (near rows are -Z of the enemy zone).
            var rotation = Quaternion.Euler(0f, 180f, 0f);
            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 pos = manager.Config.CellToWorld(cells[i]);
                var dto = new UnitDTO($"enemy_{nodeId}_{i}", "Enemy", null);   // base visuals (no definition/parts)
                UnitFactory.Spawn(dto, enemyPrefab, null, pos, rotation);
            }
        }

        private void RetireSceneEnemies()
        {
            var roots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
            {
                var r = roots[i];
                if (r == null || r.Team != Team.Enemy) continue;
                // Deactivate before destroying so the BattleStateManager registration sweep can't grab a dying unit.
                r.gameObject.SetActive(false);
                Destroy(r.gameObject);
            }
        }
    }
}
