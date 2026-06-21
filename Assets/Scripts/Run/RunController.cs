using System;
using System.Collections.Generic;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Orchestrates a run within the Map scene. Builds a seeded branching map, lets the
    /// map UI travel to a chosen node (<see cref="TravelToNode"/>), routes node entry to
    /// the right handler (battle scene / shop / event panel), stitches a new segment on
    /// when the player clears a top-row boss (infinite climb), and shows the run-end
    /// panel on loss. UI subscribes to <see cref="OnStateRefreshed"/>.
    /// </summary>
    public class RunController : MonoBehaviour
    {
        public event Action OnStateRefreshed;

        [Tooltip("Scene to load for Combat/Elite/Boss nodes.")]
        [SerializeField] private string battleSceneName = "Test_M3_Battle";

        [Tooltip("Starting gold for a new run.")]
        [SerializeField, Min(0)] private int startingGold = 0;

        [Header("Map generation")]
        [SerializeField] private MapGenConfig mapConfig = new MapGenConfig();
        [Tooltip("Fixed RNG seed for reproducible runs. 0 = random seed each new run.")]
        [SerializeField] private int fixedSeed = 0;
        [Tooltip("Difficulty added per row of depth (encounter setup reads RunState.DifficultyMultiplier).")]
        [SerializeField, Min(0f)] private float difficultyPerDepth = 0.05f;

        [Header("Panels")]
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private GameObject runEndPanel;
        [Tooltip("Run-start draft screen. If wired, a fresh run shows this before the map. Optional.")]
        [SerializeField] private GameObject draftPanel;
        [Tooltip("End-of-run recruit screen (legacy win-flow; dormant for infinite runs). Optional.")]
        [SerializeField] private GameObject recruitPanel;

        public RunState State => RunSession.Current;

        private void Start()
        {
            // Returning to the map mid-run (e.g. after a battle).
            if (RunSession.Current != null) { ResumeOnMap(); return; }

            // Resume a persisted run across app restarts.
            if (RunSession.TryLoad()) { ResumeOnMap(); return; }

            // Fresh entry: draft first if wired, else start immediately.
            if (draftPanel != null) ShowPanel(draftPanel);
            else StartRunWithParty(null);
        }

        // Show the map (or the run-end panel on loss), stitching a new segment if the
        // player just cleared the current top-row boss.
        private void ResumeOnMap()
        {
            if (State != null && State.IsLost) { ShowRunEnd(); return; }

            if (State != null && State.HasStarted && State.IsAtTopRow && State.CurrentNode.Visited)
            {
                State.AppendNextSegment(mapConfig);   // infinite: grow the map upward
                RunSession.Save();
            }

            ShowPanel(mapPanel);
            RefreshUi();
        }

        /// <summary>Begin a new run with the given drafted party (may be null/empty).</summary>
        public void StartRunWithParty(IEnumerable<UnitDTO> party)
        {
            int seed = fixedSeed != 0 ? fixedSeed : Environment.TickCount;
            var map = MapGenerator.GenerateInitial(mapConfig, seed);
            var state = new RunState(map, startingGold, seed) { DifficultyPerDepth = difficultyPerDepth };
            state.SetParty(party);
            RunSession.StartNew(state);
            ShowPanel(mapPanel);
            RefreshUi();
        }

        /// <summary>
        /// Travel to a reachable node (called by the map UI) and enter its encounter.
        /// No-op if the node isn't currently reachable.
        /// </summary>
        public void TravelToNode(int nodeId)
        {
            if (State == null || State.IsLost) return;
            if (!State.TravelTo(nodeId)) return;
            RunSession.Save();
            EnterCurrentNode();
        }

        /// <summary>Dispatch the current node to its handler (battle scene or in-map panel).</summary>
        public void EnterCurrentNode()
        {
            if (State == null) return;
            if (State.IsLost) { ShowRunEnd(); return; }

            var node = State.CurrentNode;
            if (node == null) return;

            switch (node.Type)
            {
                case NodeType.Combat:
                case NodeType.Elite:
                    State.IsBossEncounter = false;
                    SceneManager.LoadScene(battleSceneName);
                    break;
                case NodeType.Boss:
                    State.IsBossEncounter = true;
                    SceneManager.LoadScene(battleSceneName);
                    break;
                case NodeType.Shop:
                    ShowPanel(shopPanel);
                    break;
                case NodeType.Event:
                case NodeType.Rest:
                case NodeType.Treasure:
                    ShowPanel(eventPanel);
                    break;
            }
        }

        /// <summary>Called by non-combat panels (Shop / Event) on Continue: clear the node, back to the map.</summary>
        public void CompleteCurrentNode()
        {
            if (State == null) return;
            State.MarkCurrentCleared();
            // Non-combat nodes are never the top-row boss, but keep the stitch check
            // here too so the flow is uniform.
            if (State.HasStarted && State.IsAtTopRow && State.CurrentNode.Visited)
                State.AppendNextSegment(mapConfig);
            RunSession.Save();
            ShowPanel(mapPanel);
            RefreshUi();
        }

        public void StartNewRun()
        {
            RunSession.Clear();
            if (draftPanel != null) ShowPanel(draftPanel);
            else StartRunWithParty(null);
        }

        private void ShowRunEnd()
        {
            AwardUnlockPoints();
            ShowPanel(runEndPanel);
        }

        /// <summary>Called by the recruit panel when the player finishes (or skips) recruiting.</summary>
        public void FinishRecruiting() => ShowPanel(runEndPanel);

        /// <summary>
        /// Award meta-progression unlock points once, when a run ends (on loss for an
        /// infinite run). Points scale with depth reached.
        /// </summary>
        private void AwardUnlockPoints()
        {
            var s = RunSession.Current;
            if (s == null || s.RewardsGranted || !s.IsLost) return;

            int pts = UnlockRewards.PointsForRun(s.CurrentFloor, won: false);
            if (pts > 0)
            {
                LegacyStore.Current.PlayerProfile.AddPoints(pts);
                LegacyStore.Save();
            }
            s.RewardsGranted = true;
        }

        private void ShowPanel(GameObject panel)
        {
            if (mapPanel != null) mapPanel.SetActive(panel == mapPanel);
            if (shopPanel != null) shopPanel.SetActive(panel == shopPanel);
            if (eventPanel != null) eventPanel.SetActive(panel == eventPanel);
            if (runEndPanel != null) runEndPanel.SetActive(panel == runEndPanel);
            if (draftPanel != null) draftPanel.SetActive(panel == draftPanel);
            if (recruitPanel != null) recruitPanel.SetActive(panel == recruitPanel);
        }

        private void RefreshUi() => OnStateRefreshed?.Invoke();
    }
}
