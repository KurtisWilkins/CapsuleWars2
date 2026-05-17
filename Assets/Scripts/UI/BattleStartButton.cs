using CapsuleWars.Combat.State;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Drop this on a UI Button to wire it to <see cref="BattleStateManager.StartBattle"/>.
    /// Hides itself on click. M3 utility; later milestones replace this with
    /// a deployment screen → confirm flow.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattleStartButton : MonoBehaviour
    {
        [SerializeField] private BattleStateManager stateManager;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (stateManager == null) stateManager = FindAnyObjectByType<BattleStateManager>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (stateManager != null) stateManager.StartBattle();
            gameObject.SetActive(false);
        }
    }
}
