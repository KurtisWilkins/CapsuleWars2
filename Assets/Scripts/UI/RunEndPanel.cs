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

            // Runs are infinite and end only on loss.
            if (titleText != null) titleText.text = "DEFEAT";
            if (summaryText != null)
                summaryText.text = $"Depth reached: {state.CurrentFloor}\nGold earned: {state.Gold}";
        }

        private void OnReplay()
        {
            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.StartNewRun();
        }
    }
}
