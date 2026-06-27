using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Editor-tunable placement for the default floating sphere head (Slice 1, Head-as-part-type).
    /// Re-seats the head mount transform from serialized fields so the Rayman/Rabbids feel — sphere
    /// SIZE vs the capsule, the FLOAT GAP above the body, and FACE-FORWARD orientation — can be dialed
    /// without entering Play: tweak a field (OnValidate re-applies) or use the "Apply Head Preview"
    /// context menu. Applied on enable so the tuned pose also holds at runtime.
    ///
    /// This only positions the head mount (the MeshFilter/MeshRenderer the SlotMount drives); the
    /// mesh itself comes from the head BodyPart_SO via UnitCustomization, so the head still swaps and
    /// animates with the rig like any other part.
    /// </summary>
    public class HeadPreviewTuner : MonoBehaviour
    {
        [Tooltip("The floating-sphere head mount transform (the child of the B_Head bone that carries the head MeshFilter/MeshRenderer).")]
        [SerializeField] private Transform headMount;

        [Tooltip("Uniform scale of the sphere head. ~0.6 reads as ~0.6x the capsule width for the default Unity sphere primitive.")]
        [SerializeField] private float sphereSize = 0.6f;

        [Tooltip("Vertical gap (local units) of the sphere above the head bone — the detached 'floating head' look.")]
        [SerializeField] private float floatGap = 0.15f;

        [Tooltip("Face-forward orientation (local euler). Identity = the sphere's forward matches the unit's facing.")]
        [SerializeField] private Vector3 faceForwardEuler = Vector3.zero;

        public Transform HeadMount => headMount;

        private void OnEnable() => ApplyHeadPreview();

        /// <summary>Re-seat the head mount from the serialized size / float gap / orientation. Safe to call any time.</summary>
        public void ApplyHeadPreview()
        {
            if (headMount == null) return;
            headMount.localScale = Vector3.one * Mathf.Max(0.01f, sphereSize);
            var p = headMount.localPosition;
            headMount.localPosition = new Vector3(p.x, floatGap, p.z);
            headMount.localRotation = Quaternion.Euler(faceForwardEuler);
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Head Preview")]
        private void ApplyHeadPreviewMenu() => ApplyHeadPreview();

        private void OnValidate()
        {
            if (headMount != null) ApplyHeadPreview();
        }
#endif
    }
}
