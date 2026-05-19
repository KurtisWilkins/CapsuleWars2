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
    /// </summary>
    public class RunController : MonoBehaviour
    {
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

        public RunState State => RunSession.Current;

        private void Awake()
        {
            if (RunSession.Current == null)
            {
                var map = MapGenerator.Generate(defaultFloors);
                RunSession.StartNew(new RunState(map, startingGold));
            }
        }

        private void Start()
        {
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
            ShowPanel(mapPanel);
            if (State.IsComplete) ShowRunEnd();
            else RefreshUi();
        }

        public void StartNewRun()
        {
            var map = MapGenerator.Generate(defaultFloors);
            RunSession.StartNew(new RunState(map, startingGold));
            ShowPanel(mapPanel);
            RefreshUi();
        }

        private void ShowRunEnd()
        {
            ShowPanel(runEndPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            if (mapPanel != null) mapPanel.SetActive(panel == mapPanel);
            if (shopPanel != null) shopPanel.SetActive(panel == shopPanel);
            if (eventPanel != null) eventPanel.SetActive(panel == eventPanel);
            if (runEndPanel != null) runEndPanel.SetActive(panel == runEndPanel);
        }

        private void RefreshUi()
        {
            var hud = FindAnyObjectByType<UI.RunHud>();
            if (hud != null) hud.Refresh();
        }
    }
}
