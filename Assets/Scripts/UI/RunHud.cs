using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Top-of-screen HUD for a run: current gold + depth/segment. Navigation now
    /// happens by clicking nodes on the map (see MapView), so the old "enter node"
    /// button is hidden if still wired. Refreshed by RunController on state change.
    /// </summary>
    public class RunHud : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private Text floorText;
        [SerializeField] private Text nodeTypeText;
        [Tooltip("Legacy enter-node button; hidden at runtime (map clicks drive travel now).")]
        [SerializeField] private Button enterNodeButton;

        private RunController controller;

        private void OnEnable()
        {
            if (enterNodeButton != null) enterNodeButton.gameObject.SetActive(false);
            controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.OnStateRefreshed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null) controller.OnStateRefreshed -= Refresh;
        }

        public void Refresh()
        {
            var state = RunSession.Current;
            if (goldText != null) goldText.text = state != null ? $"Gold: {state.Gold}" : "Gold: 0";

            if (state == null)
            {
                if (floorText != null) floorText.text = "No active run";
                if (nodeTypeText != null) nodeTypeText.text = string.Empty;
                return;
            }

            if (floorText != null)
                floorText.text = $"Depth {state.CurrentFloor}  ·  Segment {state.SegmentIndex + 1}";

            if (nodeTypeText != null)
            {
                if (!state.HasStarted) nodeTypeText.text = "Choose your starting node";
                else nodeTypeText.text = state.CurrentNode != null ? state.CurrentNode.DisplayLabel : string.Empty;
            }
        }
    }
}
