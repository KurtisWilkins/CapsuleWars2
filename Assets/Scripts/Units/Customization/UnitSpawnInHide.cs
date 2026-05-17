using DG.Tweening;
using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Spawn-in animation that doubles as a mask for the 1-frame
    /// NavMeshAgent + Animator initialization clip. At Awake the unit
    /// is squashed to zero scale (invisible); at Start a DOTween scale-up
    /// brings it to its authored scale over <see cref="scaleInDuration"/>
    /// seconds. By the time the unit is large enough to see, the agent
    /// and rig have settled.
    /// Attach to the unit root prefab.
    /// </summary>
    public class UnitSpawnInHide : MonoBehaviour
    {
        [Tooltip("Seconds to scale from 0 to the prefab's authored scale.")]
        [SerializeField, Range(0.05f, 1.5f)] private float scaleInDuration = 0.3f;

        [Tooltip("Ease curve for the scale-up. OutBack gives a satisfying overshoot bounce.")]
        [SerializeField] private Ease ease = Ease.OutBack;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        private void Start()
        {
            transform.DOScale(originalScale, scaleInDuration)
                .SetEase(ease)
                .SetLink(gameObject);
        }
    }
}
