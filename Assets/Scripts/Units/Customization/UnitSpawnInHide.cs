using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Spawn-in animation that doubles as a mask for the 1-frame NavMeshAgent + Animator
    /// initialization clip. At Awake the unit is squashed to zero scale (invisible); an
    /// Update-driven ease-out then grows it back to its authored scale over
    /// <see cref="scaleInDuration"/> seconds, by which time the agent and rig have settled.
    ///
    /// Deliberately Update-driven (not a DOTween): a tween can silently fail to restore the
    /// unit — DOTween capacity exhaustion when many units spawn at once, an uninitialized
    /// engine, or a SetLink kill on re-parent/destroy — leaving the unit PERMANENTLY squashed
    /// and (because it never settles) looking like it floats and never animates. This version
    /// is guaranteed to reach full scale and self-restores on disable, so a freshly spawned
    /// unit can never be left compressed. Attach to the unit root prefab.
    /// </summary>
    public class UnitSpawnInHide : MonoBehaviour
    {
        [Tooltip("Seconds to scale from 0 to the prefab's authored scale.")]
        [SerializeField, Range(0.05f, 1.5f)] private float scaleInDuration = 0.3f;

        private Vector3 originalScale = Vector3.one;
        private float elapsed;
        private bool revealing;

        private void Awake()
        {
            originalScale = transform.localScale;
            // Guard: never capture a zero scale (double-Awake / pre-zeroed prefab) or the unit
            // could be permanently squashed with nothing to grow back to.
            if (originalScale == Vector3.zero) originalScale = Vector3.one;
            transform.localScale = Vector3.zero;
            elapsed = 0f;
            revealing = true;
        }

        private void Update()
        {
            if (!revealing) return;

            elapsed += Time.deltaTime;
            float t = scaleInDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / scaleInDuration);
            // Ease-out cubic: fast then settle, no overshoot (so it never reads as squashed mid-reveal).
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = originalScale * eased;

            if (t >= 1f)
            {
                transform.localScale = originalScale;   // land exactly on the authored scale
                revealing = false;
            }
        }

        private void OnDisable()
        {
            // If disabled mid-reveal (pooling, scene unload, etc.), never leave the unit squashed.
            transform.localScale = originalScale;
            revealing = false;
        }
    }
}
