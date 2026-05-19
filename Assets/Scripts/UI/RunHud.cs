using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Top-of-screen HUD for a run: current gold + current floor info.
    /// Refreshed by RunController whenever state changes.
    /// </summary>
    public class RunHud : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private Text floorText;
        [SerializeField] private Text nodeTypeText;
        [SerializeField] private Button enterNodeButton;

        private RunController controller;

        private void OnEnable()
        {
            if (enterNodeButton != null)
            {
                enterNodeButton.onClick.RemoveAllListeners();
                enterNodeButton.onClick.AddListener(EnterCurrentNode);
            }
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

            if (state.IsComplete)
            {
                if (floorText != null) floorText.text = "Run complete";
                if (nodeTypeText != null) nodeTypeText.text = string.Empty;
                return;
            }

            var node = state.CurrentNode;
            if (floorText != null) floorText.text = $"Floor {state.CurrentFloor + 1} of {state.Map.Count}";
            if (nodeTypeText != null) nodeTypeText.text = node != null ? node.DisplayLabel : string.Empty;
        }

        private void EnterCurrentNode()
        {
            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.EnterCurrentNode();
        }
    }
}
