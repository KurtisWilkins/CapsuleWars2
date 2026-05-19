using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// End-of-run panel: shows Victory if the run completed, Defeat if
    /// IsLost is set, with a Replay button that starts a new run.
    /// </summary>
    public class RunEndPanel : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button replayButton;

        private void OnEnable()
        {
            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(OnReplay);
            }
            Populate();
        }

        private void Populate()
        {
            var state = RunSession.Current;
            if (state == null)
            {
                if (titleText != null) titleText.text = "NO RUN";
                if (summaryText != null) summaryText.text = string.Empty;
                return;
            }

            bool won = state.IsComplete && !state.IsLost;
            if (titleText != null) titleText.text = won ? "VICTORY" : "DEFEAT";
            if (summaryText != null)
            {
                summaryText.text = $"Floors reached: {state.CurrentFloor} / {state.Map.Count}\nGold earned: {state.Gold}";
            }
        }

        private void OnReplay()
        {
            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.StartNewRun();
        }
    }
}
