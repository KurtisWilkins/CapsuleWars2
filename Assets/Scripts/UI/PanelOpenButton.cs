using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Tiny reusable helper: wires a Button to activate a target panel GameObject.
    /// Lives on an always-active button (e.g. a "Customize" button on the Canvas)
    /// so it can open a panel that closes itself. Pairs with panels whose own
    /// close button calls SetActive(false).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PanelOpenButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject panel;

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Open);
            }
        }

        private void Open()
        {
            if (panel != null) panel.SetActive(true);
        }
    }
}
