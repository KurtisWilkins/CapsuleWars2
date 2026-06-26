using CapsuleWars.Combat.Deployment;
using UnityEngine;

namespace CapsuleWars.Run.Encounters
{
    /// <summary>
    /// Battle-scene driver for the encounter generator (Slice C, iteration 1). On Awake — before the
    /// DeploymentManager stamps terrain and well before ArenaBuilder builds (negative execution order) — it reads
    /// the active run (seed + node id + floor), generates a seeded terrain layout via <see cref="EncounterGenerator"/>,
    /// and applies it with <c>DeploymentManager.SetTerrain</c>. ArenaBuilder then renders + NavMesh-bakes those
    /// obstacles, so each combat node has its own reproducible obstacle field.
    ///
    /// With no active run it leaves the scene's authored terrain in place (so the scene stays playable standalone),
    /// unless <see cref="requireActiveRun"/> is off (uses the fallback seed — handy for editor preview).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class EncounterBuilder : MonoBehaviour
    {
        [SerializeField] private DeploymentManager manager;
        [SerializeField] private EncounterDefinition definition;
        [Tooltip("Only generate when a run is active; otherwise keep the scene's authored terrain.")]
        [SerializeField] private bool requireActiveRun = true;
        [Tooltip("Seed/node/floor used when there's no active run (editor preview / standalone).")]
        [SerializeField] private int fallbackSeed = 12345;

        private void Awake()
        {
            if (RunSession.IsActive)
            {
                var s = RunSession.Current;
                Generate(s.Seed, s.CurrentNodeId, s.CurrentFloor);
            }
            else if (!requireActiveRun)
            {
                Generate(fallbackSeed, 0, 0);
            }
        }

        /// <summary>Generate + apply a terrain layout for the given run inputs. Public so the editor preview can call it.</summary>
        public void Generate(int seed, int nodeId, int floor)
        {
            if (manager == null || definition == null) return;
            var layout = EncounterGenerator.GenerateTerrain(definition, manager.Config, seed, nodeId, floor);
            manager.SetTerrain(layout);
        }
    }
}
