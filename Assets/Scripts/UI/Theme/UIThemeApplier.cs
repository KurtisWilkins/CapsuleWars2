using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Theme
{
    /// <summary>
    /// Drop on a panel root to give it (and its children) a consistent look from
    /// a shared <see cref="UIThemePalette"/>. Applies on enable — at runtime and,
    /// because of <c>[ExecuteAlways]</c>, in the editor too, so tweaking the
    /// palette asset reskins every panel live. Also exposes a context-menu
    /// "Apply Theme" for manual refresh.
    ///
    /// Rules (intentionally simple so the result is predictable):
    ///  - This object's own Image (if it is not itself a Button) → panelBackground.
    ///  - Every child Button's target Image → buttonNormal; its label Text → buttonText.
    ///  - Every other Text (not inside a button) → primaryText.
    /// Layout (positions/sizes/sprites) is never touched — only colors.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIThemeApplier : MonoBehaviour
    {
        [SerializeField] private UIThemePalette palette;

        [Tooltip("Recolor this object's own Image as the panel background.")]
        [SerializeField] private bool colorOwnBackground = true;

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        // Re-apply in the editor whenever this component's fields change (e.g. the
        // palette is assigned) or scripts recompile. Deferred via delayCall so we
        // don't mutate other components during OnValidate.
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Apply();
            };
        }
#endif

        [ContextMenu("Apply Theme")]
        public void Apply()
        {
            if (palette == null) return;

            // Panel background (only if this root isn't itself a button).
            if (colorOwnBackground && GetComponent<Button>() == null
                && TryGetComponent<Image>(out var bg))
            {
                bg.color = palette.panelBackground;
            }

            // Buttons: graphic + label.
            var buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var b = buttons[i];
                if (b == null) continue;
                if (b.targetGraphic is Image img) img.color = palette.buttonNormal;
                var label = b.GetComponentInChildren<Text>(true);
                if (label != null) label.color = palette.buttonText;
            }

            // Remaining text (skip labels already colored inside buttons).
            var texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i];
                if (t == null) continue;
                if (t.GetComponentInParent<Button>() != null) continue;
                t.color = palette.primaryText;
            }
        }
    }
}
