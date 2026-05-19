using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Generic "press continue" panel for Event / Rest / Treasure nodes.
    /// Applies a flat gold reward on continue and advances the run.
    /// M8+ will replace this with proper per-node-type content (Rest heals,
    /// Treasure drops equipment, Event presents choices).
    /// </summary>
    public class EventPanel : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button continueButton;

        [Tooltip("Gold awarded when player clicks Continue.")]
        [SerializeField, Min(0)] private int goldReward = 15;

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }
            Populate();
        }

        private void Populate()
        {
            var state = RunSession.Current;
            var node = state?.CurrentNode;
            if (node == null) return;

            string title = node.Type.ToString().ToUpper();
            string body = node.Type switch
            {
                NodeType.Event => $"A mysterious event grants you {goldReward} gold.",
                NodeType.Rest => $"You rest. Take {goldReward} gold and recover.",
                NodeType.Treasure => $"You find {goldReward} gold in a hidden cache.",
                _ => $"You gain {goldReward} gold."
            };

            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
        }

        private void OnContinue()
        {
            var state = RunSession.Current;
            if (state != null) state.AddGold(goldReward);

            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.CompleteCurrentNode();
        }
    }
}
