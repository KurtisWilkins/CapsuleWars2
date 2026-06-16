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
    /// Orchestrates a run within the Map scene. Reads <see cref="RunSession.Current"/>
    /// (creates a new run if none exists), routes node entry to the right
    /// handler (load battle scene, show shop panel, show event panel, etc.),
    /// and shows the run-end panel when the run completes or is lost.
    ///
    /// UI components (RunHud etc.) subscribe to <see cref="OnStateRefreshed"/>
    /// instead of being referenced directly — keeps Run independent of UI.
    /// </summary>
    public class RunController : MonoBehaviour
    {
        public event Action OnStateRefreshed;

        [Tooltip("Scene to load for Combat/Elite/Boss nodes.")]
        [SerializeField] private string battleSceneName = "Test_M3_Battle";

        [Tooltip("Total floors for a new run. Min 2 (first floor + boss).")]
        [SerializeField, Min(2)] private int defaultFloors = 5;

        [Tooltip("Starting gold for a new run.")]
        [SerializeField, Min(0)] private int startingGold = 0;

        [Header("Panels")]
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private GameObject runEndPanel;
        [Tooltip("Run-start draft screen. If wired, a fresh run shows this before the map. Optional.")]
        [SerializeField] private GameObject draftPanel;
        [Tooltip("End-of-run recruit screen, shown on a win before the run-end panel when recruits are pending. Optional.")]
        [SerializeField] private GameObject recruitPanel;

        public RunState State => RunSession.Current;

        private void Start()
        {
            // Returning to the map mid-run (e.g. after a battle): just show it.
            if (RunSession.Current != null)
            {
                ShowPanel(mapPanel);
                RefreshUi();
                return;
            }

            // Resume a persisted run across app restarts (run-scoped party +
            // equipment survive via RunStore). Only when no in-memory run exists.
            if (RunSession.TryLoad())
            {
                ShowPanel(mapPanel);
                RefreshUi();
                return;
            }

            // Fresh entry: draft first if a draft panel is wired; otherwise start
            // immediately with no drafted party (battle uses scene-placed units).
            if (draftPanel != null) ShowPanel(draftPanel);
            else StartRunWithParty(null);
        }

        /// <summary>
        /// Begin a new run with the given drafted party (may be null/empty, in
        /// which case the battle scene falls back to its scene-placed players).
        /// Called by the draft screen.
        /// </summary>
        public void StartRunWithParty(IEnumerable<UnitDTO> party)
        {
            var map = MapGenerator.Generate(defaultFloors);
            var state = new RunState(map, startingGold);
            state.SetParty(party);
            RunSession.StartNew(state);
            ShowPanel(mapPanel);
            RefreshUi();
        }

        public void EnterCurrentNode()
        {
            if (State == null) return;
            if (State.IsLost || State.IsComplete) { ShowRunEnd(); return; }

            var node = State.CurrentNode;
            if (node == null) { ShowRunEnd(); return; }

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

        /// <summary>Called by non-combat panels (Shop / Event) when the player presses Continue.</summary>
        public void CompleteCurrentNode()
        {
            if (State == null) return;
            State.AdvanceNode();
            RunSession.Save();
            ShowPanel(mapPanel);
            if (State.IsComplete) ShowRunEnd();
            else RefreshUi();
        }

        public void StartNewRun()
        {
            // Route a brand-new run back through the draft when one is wired.
            RunSession.Clear();
            if (draftPanel != null) ShowPanel(draftPanel);
            else StartRunWithParty(null);
        }

        private void ShowRunEnd()
        {
            AwardUnlockPoints();

            // On a win with pending roguelike-only recruits, offer recruitment
            // first; the recruit panel calls FinishRecruiting() to continue.
            var s = RunSession.Current;
            bool won = s != null && s.IsComplete && !s.IsLost;
            if (won && recruitPanel != null && s.Recruits != null && s.Recruits.Count > 0)
            {
                ShowPanel(recruitPanel);
                return;
            }
            ShowPanel(runEndPanel);
        }

        /// <summary>Called by the recruit panel when the player finishes (or skips) recruiting.</summary>
        public void FinishRecruiting()
        {
            ShowPanel(runEndPanel);
        }

        /// <summary>
        /// Award meta-progression unlock points for a finished run, exactly once
        /// (Docs/12_RoguelikeRun.md §82). Points scale with floors reached, plus a
        /// completion bonus on a win.
        /// </summary>
        private void AwardUnlockPoints()
        {
            var s = RunSession.Current;
            if (s == null || s.RewardsGranted) return;
            if (!s.IsComplete && !s.IsLost) return;   // run not actually finished

            int pts = UnlockRewards.PointsForRun(s.CurrentFloor, s.IsComplete && !s.IsLost);
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

        private void RefreshUi()
        {
            OnStateRefreshed?.Invoke();
        }
    }
}
