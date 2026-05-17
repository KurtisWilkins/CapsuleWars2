using System.Collections;
using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Hides every child Renderer for a couple of frames at spawn to mask
    /// the one-frame initialization clip that happens while NavMeshAgent
    /// and the humanoid Animator settle into their canonical positions.
    /// Attach to the unit root prefab.
    /// </summary>
    public class UnitSpawnInHide : MonoBehaviour
    {
        [Tooltip("How many frames to keep renderers hidden after Start. 2 is enough for most rigs; bump higher if you still see a flicker.")]
        [SerializeField, Range(1, 10)] private int hideFrames = 2;

        private Renderer[] renderers;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Start()
        {
            StartCoroutine(HideForFrames());
        }

        private IEnumerator HideForFrames()
        {
            SetVisible(false);
            for (int i = 0; i < hideFrames; i++) yield return null;
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (renderers == null) return;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }
}
